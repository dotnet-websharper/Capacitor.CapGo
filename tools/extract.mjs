// Turns a CapGo plugin's TypeScript declarations into the flat JSON model that
// CapGoGeneratorCommon turns into the WIG code model.
//
// The model deliberately has no generics, no conditional types and no anonymous
// object types: everything reachable from the plugin instance is resolved, and
// anything that cannot be flattened is reported under `unsupported` so it can be
// handled by a hand-written override.
import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { join } from "node:path";
import {
  ModuleKind,
  ModuleResolutionKind,
  Project,
  ScriptTarget,
} from "ts-morph";
import { fetchPackage, indexDts, listDts, npmLatest } from "./lib/fetch.mjs";

const modelDir = new URL("./model/", import.meta.url).pathname;
const rosterPath = new URL("./plugins.json", import.meta.url).pathname;
const overridesDir = new URL("./overrides/", import.meta.url).pathname;

const PRIMITIVES = new Set(["string", "float", "int", "bool", "unit", "obj"]);

export function makeProject(dir) {
  const project = new Project({
    skipAddingFilesFromTsConfig: true,
    compilerOptions: {
      target: ScriptTarget.ES2022,
      module: ModuleKind.ESNext,
      moduleResolution: ModuleResolutionKind.Bundler,
      strict: false,
      skipLibCheck: true,
      noEmit: true,
    },
  });
  project.addSourceFilesAtPaths(listDts(dir));
  return project;
}

class Extractor {
  constructor(project, indexFile, hints, dir) {
    this.project = project;
    this.indexFile = indexFile;
    this.hints = hints;
    this.dir = dir;
    this.types = new Map();
    this.byShape = new Map();
    this.externalTypes = new Set();
    this.visiting = new Set();
    this.coreRefs = new Set();
  }

  /** The dependency path a symbol comes from, when it is not the plugin's own. */
  externalPath(symbol) {
    const decl = symbol.getDeclarations()[0];
    if (!decl) return null;
    const p = decl.getSourceFile().getFilePath();
    return p.startsWith(this.dir) ? null : p;
  }

  hintName(hint) {
    const clean = hint.replace(/[^A-Za-z0-9]/g, "");
    return clean.charAt(0).toUpperCase() + clean.slice(1);
  }

  registerAnonymous(name, shape) {
    const key = JSON.stringify(shape);
    if (this.byShape.has(key)) return this.byShape.get(key);
    let unique = name;
    let i = 2;
    while (this.types.has(unique)) unique = name + i++;
    this.types.set(unique, { name: unique, ...shape });
    this.byShape.set(key, unique);
    return unique;
  }

  registerNamed(name, type) {
    if (this.types.has(name) || this.visiting.has(name)) return name;
    const symbol = type.getSymbol();
    const decl = symbol?.getDeclarations()[0];
    // Only types declared by the plugin package itself; lib.d.ts and dependency
    // types (e.g. PluginListenerHandle) are handled elsewhere.
    if (!decl || !decl.getSourceFile().getFilePath().startsWith(this.dir)) return null;
    this.visiting.add(name);

    if (decl?.getKindName() === "TypeAliasDeclaration") {
      const target = decl.getType();
      if (this.isStringLiteralUnion(target) || this.isNumericLiteralUnion(target)) {
        const kind = this.isStringLiteralUnion(target) ? "enumStrings" : "enum";
        const cases = target.getUnionTypes().map(t => (kind === "enumStrings" ? t.getLiteralValue() : Number(t.getLiteralValue())));
        this.types.set(name, { name, kind, cases });
        this.visiting.delete(name);
        return name;
      }
      if (target.isUnion() && target.getUnionTypes().every(t => this.refNameOf(t))) {
        this.types.set(name, { name, kind: "union", of: target.getUnionTypes().map(t => this.refNameOf(t)) });
        this.visiting.delete(name);
        return name;
      }
      // Any other alias is substituted at the use site.
      this.visiting.delete(name);
      return null;
    }

    if (decl?.getKindName() === "InterfaceDeclaration" || decl?.getKindName() === "ClassDeclaration") {
      const { required, optional } = this.membersOf(type, name);
      this.types.set(name, { name, kind: "config", required, optional });
      this.visiting.delete(name);
      return name;
    }

    if (decl?.getKindName() === "EnumDeclaration") {
      const members = decl.getMembers();
      const values = members.map(m => m.getValue());
      const isString = values.every(v => typeof v === "string");
      const cases = isString
        ? values.map(v => String(v))
        : members.map((m, i) => ({ name: m.getName(), value: Number(values[i] ?? i) }));
      this.types.set(name, { name, kind: isString ? "enumStrings" : "enum", cases });
      this.visiting.delete(name);
      return name;
    }

    this.visiting.delete(name);
    return null;
  }

  membersOf(type, parentName) {
    const required = [];
    const optional = [];
    for (const prop of type.getProperties()) {
      if (prop.getName().startsWith("__")) continue;
      const decl = prop.getDeclarations()[0];
      const propType = prop.getTypeAtLocation(decl ?? this.indexFile);
      const hint = `${parentName}${cap(prop.getName())}`;
      const model = this.conv(propType, hint);
      const entry = { name: prop.getName(), type: model };
      (prop.isOptional() ? optional : required).push(entry);
    }
    return { required, optional };
  }

  refNameOf(type) {
    const symbol = type.getSymbol();
    if (!symbol) return null;
    const name = symbol.getName();
    if (!name || name === "__type" || name.startsWith("__")) return null;
    if (this.registerNamed(name, type) === null) return null;
    return name;
  }

  isEnumType(type) {
    const decl = type.getSymbol()?.getDeclarations()[0];
    return decl?.getKindName() === "EnumDeclaration" || type.isEnum?.();
  }

  isStringLiteralUnion(type) {
    return type.isUnion() && type.getUnionTypes().every(t => t.isStringLiteral());
  }

  isNumericLiteralUnion(type) {
    return type.isUnion() && type.getUnionTypes().every(t => t.isNumberLiteral());
  }

  conv(type, hint) {
    // Cyclic type graphs exist in a few plugins (self-referential aliases);
    // cut them at the first repeat rather than recursing forever.
    const key = type.getText();
    if (this.converting?.has(key)) return { k: "obj" };
    this.converting = this.converting ?? new Set();
    this.converting.add(key);
    try {
      return this.convInner(type, hint);
    } finally {
      this.converting.delete(key);
    }
  }

  convInner(type, hint) {
    if (type.getText() === "void") return { k: "unit" };
    if (type.isAny() || type.isUnknown()) return { k: "obj" };
    if (type.isStringLiteral() || type.isString()) return { k: "string" };
    if (type.isNumberLiteral()) return { k: "float" };
    // TS keeps `true | false` as literals inside unions, and CapGo uses
    // `boolean` in discriminated unions a lot.
    if (type.isBooleanLiteral()) return { k: "bool" };
    // Branded primitive, e.g. `string & Record<never, never>`: the intersection
    // is still a string at runtime and in the API.
    if (type.isIntersection()) {
      const parts = type.getIntersectionTypes();
      if (parts.some(t => t.isString())) return { k: "string" };
      if (parts.some(t => t.isNumber())) return { k: "float" };
      if (parts.some(t => t.isBoolean())) return { k: "bool" };
      const ref = parts.map(t => this.refNameOf(t)).find(Boolean);
      return ref ? { k: "ref", name: ref } : { k: "obj" };
    }
    if (this.isEnumType(type)) {
      const name = type.getSymbol()?.getName();
      if (name && this.registerNamed(name, type) === name) return { k: "ref", name };
    }
    if (type.isString()) return { k: "string" };
    if (type.isBoolean()) return { k: "bool" };
    if (type.isNumber()) return { k: "float" };
    if (type.isVoid() || type.isUndefined() || type.isNull()) return { k: "unit" };
    if (type.isNever()) return { k: "obj" };

    if (type.isArray()) return { k: "array", of: this.conv(type.getArrayElementType(), hint) };
    if (type.isTuple()) {
      const parts = type.getTupleElements().map(t => this.conv(t, hint));
      const seen = new Set();
      const of = parts.filter(t => {
        const key = JSON.stringify(t);
        if (seen.has(key)) return false;
        seen.add(key);
        return true;
      });
      return { k: "array", of: of.length === 1 ? of[0] : { k: "union", of } };
    }

    if (type.isUnion()) {
      const parts = type.getUnionTypes().filter(t => !t.isUndefined() && !t.isNull());
      if (parts.length === 1) return this.conv(parts[0], hint);
      if (parts.every(t => t.isStringLiteral())) {
        const cases = parts.map(t => t.getLiteralValue());
        return { k: "ref", name: this.registerAnonymous(this.hintName(hint), { kind: "enumStrings", cases }) };
      }
      if (parts.every(t => t.isNumberLiteral())) {
        const cases = parts.map(t => ({ name: String(t.getLiteralValue()), value: Number(t.getLiteralValue()) }));
        return { k: "ref", name: this.registerAnonymous(this.hintName(hint), { kind: "enum", cases }) };
      }
      const refs = parts.map(t => this.refNameOf(t));
      if (refs.every(Boolean)) return { k: "union", of: refs.map(n => ({ k: "ref", name: n })) };
      const converted = parts.map(t => this.conv(t, hint));
      if (converted.every(t => t)) {
        const seen = new Set();
        const of = converted.filter(t => {
          const key = JSON.stringify(t);
          if (seen.has(key)) return false;
          seen.add(key);
          return true;
        });
        return of.length === 1 ? of[0] : { k: "union", of };
      }
      return { k: "obj" };
    }

    if (type.isStringLiteral()) return { k: "string" };

    // Callback-valued fields and parameters: keep the signature.
    const callSignatures = type.getCallSignatures();
    if (callSignatures.length) {
      const sig = callSignatures[0];
      const args = sig.getParameters().map(p => {
        const d = p.getDeclarations()[0];
        return this.conv(p.getTypeAtLocation(d ?? this.indexFile), `${hint}Arg`);
      });
      const retType = sig.getReturnType();
      let ret;
      if (retType.getText().startsWith("Promise<")) {
        const a = retType.getTypeArguments();
        ret = a.length ? { k: "promise", of: this.conv(a[0], `${hint}Result`) } : { k: "promise", of: { k: "unit" } };
      } else {
        ret = this.conv(retType, `${hint}Result`);
      }
      return { k: "func", args, ret };
    }

    // Index signatures: {[key: string]: T} become a named map type, which the
    // generator emits as a class with a string indexer.
    const indexType = type.getStringIndexType();
    if (indexType) {
      const of = this.conv(indexType, `${hint}Value`);
      return {
        k: "ref",
        name: this.registerAnonymous(this.hintName(`${hint}Map`), { kind: "map", of }),
      };
    }

    // Object types: reuse the declared name when there is one.
    const symbol = type.getSymbol();
    if (symbol) {
      const name = symbol.getName();
      if (name === "__type" || !name || name.startsWith("__")) {
        const shape = this.membersOf(type, this.hintName(hint));
        return { k: "ref", name: this.registerAnonymous(this.hintName(hint), { kind: "config", ...shape }) };
      }
      const external = this.externalPath(symbol);
      if (external) {
        if (external.includes("@capacitor/core")) {
          this.coreRefs.add(name);
          return { k: "ref", name, core: true };
        }
        if (name === "Promise") {
          const args = type.getTypeArguments();
          if (args.length) return { k: "promise", of: this.conv(args[0], hint) };
        }
        this.externalTypes.add(name);
        return { k: "ext", name };
      }
      const registered = this.registerNamed(name, type);
      if (registered) return { k: "ref", name: registered };
    }

    return { k: "obj" };
  }

  seed(name) {
    for (const sf of this.project.getSourceFiles()) {
      if (!sf.getFilePath().startsWith(this.dir)) continue;
      const decl = sf.getInterface(name) ?? sf.getTypeAlias(name) ?? sf.getEnum(name);
      if (decl) {
        this.registerNamed(name, decl.getType());
        return;
      }
    }
  }

  methodOf(symbol) {
    const decl = symbol.getDeclarations()[0];
    const sig = symbol.getTypeAtLocation(decl ?? this.indexFile).getCallSignatures()[0];
    if (!sig) return null;
    const method = { name: symbol.getName(), params: [], returns: null, promise: false };
    const docs = decl?.getJsDocs?.() ?? [];
    if (docs.length) method.comment = docs.map(d => d.getDescription().trim()).filter(Boolean).join(" ");

    if (sig.getTypeParameters().length) {
      return { unsupported: { method: method.name, reason: "generic method cannot be flattened" } };
    }

    for (const p of sig.getParameters()) {
      const pDecl = p.getDeclarations()[0];
      const pType = p.getTypeAtLocation(pDecl ?? this.indexFile);
      method.params.push({
        name: p.getName(),
        type: this.conv(pType, `${method.name}${cap(p.getName())}`),
        optional: p.isOptional() || (pDecl && typeof pDecl.isOptional === "function" && pDecl.isOptional()) || false,
      });
    }

    const ret = sig.getReturnType();
    if (ret.getText().startsWith("Promise<")) {
      method.promise = true;
      const args = ret.getTypeArguments();
      method.returns = args.length ? this.conv(args[0], `${method.name}Result`) : { k: "unit" };
    } else {
      method.returns = this.conv(ret, `${method.name}Result`);
    }

    return { method };
  }

  eventsOf(symbol) {
    const decl = symbol.getDeclarations()[0];
    const sigs = symbol.getTypeAtLocation(decl ?? this.indexFile).getCallSignatures();
    const events = [];
    for (const sig of sigs) {
      if (sig.getParameters().length !== 2) continue;
      const [evtParam, listenerParam] = sig.getParameters();
      const evtType = evtParam.getTypeAtLocation(evtParam.getDeclarations()[0] ?? this.indexFile);
      if (!evtType.isStringLiteral() && !this.isStringLiteralUnion(evtType)) continue;
      const names = evtType.isUnion()
        ? evtType.getUnionTypes().filter(t => t.isStringLiteral()).map(t => t.getLiteralValue())
        : [evtType.getLiteralValue()];
      if (!names.length || names.some(n => typeof n !== "string")) continue;

      const listenerType = listenerParam.getTypeAtLocation(listenerParam.getDeclarations()[0] ?? this.indexFile);
      const lsig = listenerType.getCallSignatures()[0];
      let payload = { k: "unit" };
      if (lsig && lsig.getParameters().length === 1) {
        const p = lsig.getParameters()[0];
        payload = this.conv(p.getTypeAtLocation(p.getDeclarations()[0] ?? this.indexFile), `${cap(names[0])}Event`);
      }
      for (const n of names) events.push({ name: n, listener: payload });
    }
    return events.length ? events : null;
  }
}

const cap = s => s.charAt(0).toUpperCase() + s.slice(1);

function readOverride(plugin) {
  if (!plugin.overrides) return null;
  try {
    return JSON.parse(readFileSync(join(overridesDir, `${plugin.overrides}.json`), "utf8"));
  } catch {
    return null;
  }
}

export async function extract(plugin) {
  const { version, license } = await npmLatest(plugin.package);
  const dir = fetchPackage(plugin.package, version);
  const project = makeProject(dir);
  const indexPath = indexDts(dir);
  if (!indexPath) throw new Error(`no index.d.ts in ${plugin.package}@${version}`);
  const index = project.getSourceFile(indexPath);
  if (!index) throw new Error(`could not load ${indexPath}`);

  const exported = index.getExportedDeclarations();
  const candidates = [];
  for (const [name, decls] of exported) {
    for (const decl of decls) {
      // Plugin instances are exported as `const X: XPlugin = registerPlugin(...)`.
      if (decl.getKindName() !== "VariableDeclaration") continue;
      if (!decl.getSourceFile().getFilePath().startsWith(dir)) continue;
      const type = decl.getType();
      if (type.isString() || type.isNumber() || type.isBoolean() || type.isArray()) continue;
      const methodNames = type
        .getProperties()
        .filter(p => {
          if (p.getName().startsWith("__")) return false;
          const pd = p.getDeclarations()[0];
          if (!pd || !pd.getSourceFile().getFilePath().startsWith(dir)) return false;
          return p.getTypeAtLocation(pd).getCallSignatures().length > 0;
        })
        .map(p => p.getName());
      if (methodNames.length) candidates.push({ name, decl, type, methodNames });
    }
  }
  if (!candidates.length) throw new Error(`no plugin instance export in ${plugin.package}@${version}`);
  candidates.sort((a, b) => b.methodNames.length - a.methodNames.length);
  const chosen = plugin.export
    ? candidates.find(c => c.name === plugin.export) ?? candidates[0]
    : candidates[0];

  const ex = new Extractor(project, index, plugin, dir);
  const methods = [];
  const events = [];
  const unsupported = [];

  for (const prop of chosen.type.getProperties()) {
    const name = prop.getName();
    const type = prop.getTypeAtLocation(chosen.decl);
    if (!type.getCallSignatures().length) continue;

    if (name === "addListener") {
      const evs = ex.eventsOf(prop);
      if (evs) {
        events.push(...evs);
      } else {
        unsupported.push({ method: name, reason: "addListener signature not recognised" });
      }
      continue;
    }

    const { method, unsupported: u } = ex.methodOf(prop) ?? {};
    if (u) unsupported.push(u);
    if (method) methods.push(method);
  }

  const seed = readOverride(plugin)?.seedTypes ?? [];
  for (const name of seed) ex.seed(name);

  const model = {
    namespace: plugin.namespace,
    family: plugin.family,
    package: plugin.package,
    version,
    license,
    export: chosen.name,
    types: [...ex.types.values()].sort((a, b) => a.name.localeCompare(b.name)),
    methods,
    events,
    unsupported,
    coreRefs: [...ex.coreRefs].sort(),
    externalTypes: [...ex.externalTypes].sort(),
  };

  applyOverrides(model, plugin);
  return model;
}

function applyOverrides(model, plugin) {
  const ov = readOverride(plugin);
  if (!ov) return;
  if (ov.drop) model.methods = model.methods.filter(m => !ov.drop.includes(m.name));
  if (ov.dropTypes) model.types = model.types.filter(t => !ov.dropTypes.includes(t.name));
  if (ov.types) {
    for (const t of ov.types) {
      const i = model.types.findIndex(x => x.name === t.name);
      if (i >= 0) model.types[i] = t;
      else model.types.push(t);
    }
    model.types.sort((a, b) => a.name.localeCompare(b.name));
  }
  if (ov.methods) {
    for (const m of ov.methods) {
      const i = model.methods.findIndex(x => x.name === m.name);
      if (i >= 0) model.methods[i] = m;
      else model.methods.push(m);
    }
  }
  if (ov.unsupportedResolved) {
    model.unsupported = model.unsupported.filter(u => !ov.unsupportedResolved.includes(u.method));
  }
}

// --- CLI -------------------------------------------------------------------

const args = process.argv.slice(2);
const check = args.includes("--check");
const only = args.filter(a => !a.startsWith("--"));

const roster = JSON.parse(readFileSync(rosterPath, "utf8"));
const selected = roster.plugins.filter(p => !only.length || only.includes(p.namespace));

mkdirSync(modelDir, { recursive: true });
let changed = 0;
const summary = [];

const failures = [];
for (const plugin of selected) {
  let model;
  try {
    model = await extract(plugin);
  } catch (e) {
    failures.push({ plugin, error: e instanceof Error ? e.message : String(e) });
    console.error(`FAILED ${plugin.namespace} (${plugin.package}): ${e instanceof Error ? e.message : e}`);
    continue;
  }
  const path = join(modelDir, `${slugOf(plugin.namespace)}.json`);
  const text = JSON.stringify(model, null, 2) + "\n";
  let previous = null;
  try {
    previous = readFileSync(path, "utf8");
  } catch {}
  if (previous !== text) {
    changed++;
    if (!check) writeFileSync(path, text);
  }
  summary.push({
    plugin,
    model,
    status: previous === null ? "new" : previous === text ? "same" : check ? "drift" : "updated",
  });
}

function audit(model) {
  const stats = { nodes: 0, obj: 0, byParent: {}, external: new Set() };
  const walk = (t, parent) => {
    if (!t) return;
    stats.nodes++;
    if (t.k === "obj") {
      stats.obj++;
      stats.byParent[parent] = (stats.byParent[parent] || 0) + 1;
    }
    if (t.k === "ext") stats.external.add(t.name);
    if (t.k === "array" || t.k === "promise") walk(t.of, t.k);
    if (t.k === "map") walk(t.of, "map");
    if (t.k === "union") t.of.forEach(x => walk(x, "union"));
    if (t.k === "func") {
      t.args.forEach(a => walk(a, "callback arg"));
      walk(t.ret, "callback result");
    }
  };
  for (const t of model.types) {
    if (t.kind === "config") {
      [...(t.required || []), ...(t.optional || [])].forEach(x => walk(x.type, "field"));
    }
    if (t.kind === "map") walk(t.of, "map value");
  }
  model.methods.forEach(m => {
    m.params.forEach(p => walk(p.type, "parameter"));
    walk(m.returns, "return");
  });
  model.events.forEach(e => walk(e.listener, "event"));
  return stats;
}

const audits = summary.map(({ model }) => audit(model));
const totals = audits.reduce(
  (a, x) => {
    a.nodes += x.nodes;
    a.obj += x.obj;
    for (const [k, v] of Object.entries(x.byParent)) a.byParent[k] = (a.byParent[k] || 0) + v;
    x.external.forEach(n => a.external.add(n));
    return a;
  },
  { nodes: 0, obj: 0, byParent: {}, external: new Set() },
);

const rows = summary.map(({ plugin, model, status }, i) => {
  const methods = model.methods.length + model.events.length;
  const unsupported = model.unsupported.length;
  const a = audits[i];
  const typed = a.nodes === 0 ? 100 : Math.round(((a.nodes - a.obj) / a.nodes) * 100);
  return `| ${plugin.namespace} | \`${model.package}\` | ${model.version} | ${model.license} | ${methods} | ${
    unsupported ? unsupported + " ⚠" : "—"
  } | ${typed}% | ${a.obj} | ${status} |`;
});

const pct = Math.round(((totals.nodes - totals.obj) / totals.nodes) * 100);
const licenceTally = {};
for (const { model } of summary) {
  licenceTally[model.license] = (licenceTally[model.license] || 0) + 1;
}
const licences = Object.entries(licenceTally)
  .sort((a, b) => b[1] - a[1])
  .map(([k, v]) => `${k} × ${v}`)
  .join(", ");
const breakdown = Object.entries(totals.byParent)
  .sort((a, b) => b[1] - a[1])
  .map(([k, v]) => `| ${k} | ${v} |`)
  .join("\n");

const report = [
  "# CapGo plugin surface coverage",
  "",
  "Generated by `tools/extract.mjs` from each package's `dist/**/index.d.ts`.",
  "Methods and events are counted separately; `unsupported` needs a hand-written override.",
  `**Typed surface: ${pct}%** (${totals.nodes - totals.obj} of ${totals.nodes} type positions). ` +
    `${totals.obj} positions remain \`obj\`; they are either upstream \`any\`/\`unknown\` or ` +
    "JSON payloads, broken down below.",
  `Upstream plugin licences: ${licences}. The bindings themselves are Apache 2.0.`,
  "",
  "| Namespace | npm package | Version | License | Methods + events | Unsupported | Typed | obj | Status |",
  "|---|---|---:|---|---:|---:|---:|---:|---|",
  ...rows,
  "",
  "## Remaining `obj` positions",
  "",
  "| Where | Count |",
  "|---|---:|",
  breakdown,
  "",
  "External types referenced by the models (mapped to WebSharper.JavaScript types when " +
    "available, otherwise `obj`): " +
    (totals.external.size ? [...totals.external].sort().map(n => `\`${n}\``).join(", ") : "none"),
  "",
].join("\n");

const reportPath = new URL("./coverage.md", import.meta.url).pathname;
writeFileSync(reportPath, report);

if (failures.length) {
  console.log("\n## Extraction failures\n");
  for (const f of failures) console.log(`- ${f.plugin.namespace} (${f.plugin.package}): ${f.error}`);
}
console.log(report);
console.log(check ? (changed ? `${changed} plugin(s) drifted` : "no drift") : `${changed} plugin(s) updated`);
if (check) process.exitCode = changed ? 1 : 0;
if (failures.length) process.exitCode = 2;

function slugOf(namespace) {
  return namespace.replace(/([a-z0-9])([A-Z])/g, "$1-$2").replace(/([A-Z])([A-Z][a-z])/g, "$1-$2").toLowerCase();
}