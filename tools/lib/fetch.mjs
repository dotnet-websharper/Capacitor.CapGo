// Downloads a CapGo plugin package and extracts its TypeScript declarations.
import { execFileSync } from "node:child_process";
import { existsSync, mkdirSync, readdirSync } from "node:fs";
import { join } from "node:path";

const cacheRoot = new URL("../.cache/", import.meta.url).pathname;

export const slug = p => p.replace(/^@/, "").replace(/\//g, "-");

export function npmLatest(pkg) {
  const meta = JSON.parse(
    execFileSync("npm", ["view", pkg, "version", "--json"], { encoding: "utf8" }),
  );
  return meta;
}

/** Returns the package directory with dist/esm/*.d.ts extracted. */
export function fetchPackage(pkg, version) {
  const dir = join(cacheRoot, `${slug(pkg)}@${version}`);
  if (existsSync(join(dir, "package", "dist", "esm"))) return join(dir, "package");

  mkdirSync(join(cacheRoot), { recursive: true });
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
      if (entry.isDirectory()) walk(p);
      else if (entry.name.endsWith(".d.ts")) out.push(p);
    }
  };
  walk(join(dir, "dist", "esm"));
  return out;
}