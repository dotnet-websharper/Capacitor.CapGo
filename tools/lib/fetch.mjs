// Downloads a CapGo plugin package and locates its TypeScript declarations.
import { execFileSync } from "node:child_process";
import { existsSync, mkdirSync, readdirSync } from "node:fs";
import { join } from "node:path";

const cacheRoot = new URL("../.cache/", import.meta.url).pathname;

export const slug = p => p.replace(/^@/, "").replace(/\//g, "-");

export async function npmLatest(pkg) {
  const m = await fetch(`https://registry.npmjs.org/${pkg.replace("/", "%2F")}/latest`).then(r => r.json());
  const license =
    typeof m.license === "string"
      ? m.license
      : (m.license && m.license.type) || (m.licenses && m.licenses[0] && m.licenses[0].type) || "unknown";
  return { version: m.version, license };
}

// Framework wrappers under dist/{react,vue,...} redeclare the plugin types and
// only add noise; skip them.
const SKIP = /\/dist\/(react|vue|angular|svelte|solid)\//;

export function fetchPackage(pkg, version) {
  const dir = join(cacheRoot, `${slug(pkg)}@${version}`);
  if (existsSync(join(dir, "package.json"))) return dir;

  mkdirSync(cacheRoot, { recursive: true });
  const tarball = execFileSync(
    "npm",
    ["pack", `${pkg}@${version}`, "--pack-destination", cacheRoot, "--silent"],
    { encoding: "utf8" },
  )
    .trim()
    .split("\n")
    .pop();

  mkdirSync(dir, { recursive: true });
  execFileSync("tar", ["-xzf", join(cacheRoot, tarball), "-C", dir, "--strip-components=1"]);
  return dir;
}

export function listDts(dir) {
  const out = [];
  const walk = d => {
    for (const entry of readdirSync(d, { withFileTypes: true })) {
      const p = join(d, entry.name);
      if (entry.isDirectory()) {
        if (entry.name === "node_modules") continue;
        walk(p);
      } else if (entry.name.endsWith(".d.ts") && !SKIP.test(p)) out.push(p);
    }
  };
  walk(dir);
  return out;
}

/// The module entry that exports the plugin instance, for the layouts CapGo
/// packages use today (ESM bundle, flat dist, or dist/typings).
export function indexDts(dir) {
  const candidates = [
    "dist/esm/index.d.ts",
    "dist/index.d.ts",
    "dist/typings/index.d.ts",
    "index.d.ts",
  ];
  for (const c of candidates) {
    const p = join(dir, c);
    if (existsSync(p)) return p;
  }
  return null;
}
