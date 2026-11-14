# CapGo WIG generator

The bindings are generated from each plugin's published TypeScript declarations
rather than hand-written. This directory holds the tooling; the F# side that
turns these JSON models into a WebSharper WIG assembly lives in
`CapGoGeneratorCommon/`.

## Pipeline

```sh
npm install          # once
node extract.mjs     # refresh tools/model/*.json from the latest npm releases
node extract.mjs --check   # fail if any committed model is out of date (CI)
node extract.mjs NativeAudio CapacitorUpdater   # only some plugins
```

`extract.mjs` writes `tools/model/<plugin>.json` and `tools/coverage.md`.

## Roster (`plugins.json`)

The only hand-curated file. One entry per plugin:

```json
{
  "namespace": "NativeAudio",          // WIG namespace: WebSharper.Capacitor.CapGo.<namespace>
  "family": "Core",                    // package: WebSharper.Capacitor.CapGo[.<family>]
  "package": "@capgo/capacitor-native-audio",
  "export": "NativeAudio",             // optional: export symbol, detected when omitted
  "overrides": "native-audio"          // optional: file under overrides/
}
```

`families` maps a family name to its package id.

## Model (`model/*.json`)

A flat, resolved description of a plugin's surface. No generics, no conditional
types and no anonymous object types survive: inline types are hoisted to named
configs, unions of object types stay unions of refs, and literal unions become
`enumStrings`.

```jsonc
{
  "namespace": "CapacitorDownloader",
  "family": "Core",
  "package": "@capgo/capacitor-downloader",
  "version": "8.6.0",
  "export": "CapacitorDownloader",
  "types": [
    { "name": "DownloadOptions", "kind": "config",
      "required": [ { "name": "id", "type": { "k": "string" } } ],
      "optional": [] },
    { "name": "DownloadTaskState", "kind": "enumStrings", "cases": ["PENDING"] },
    { "name": "SomeNumericEnum", "kind": "enum", "cases": [ { "name": "A", "value": 0 } ] },
    { "name": "ARequest", "kind": "union", "of": ["BRequest", "CRequest"] }
  ],
  "methods": [
    { "name": "download",
      "params": [ { "name": "options", "type": { "k": "ref", "name": "DownloadOptions" },
                    "optional": false } ],
      "returns": { "k": "ref", "name": "DownloadTask" },
      "promise": true,
      "comment": "Start a new download task." }
  ],
  "events": [ { "name": "downloadProgress",
                "listener": { "k": "ref", "name": "DownloadProgressEvent" } } ],
  "unsupported": [],
  "coreRefs": []
}
```

### Type DSL

| Model | Meaning |
|---|---|
| `{ "k": "string" }` / `float` / `bool` / `unit` / `obj` | primitives (`obj` is `number`-agnostic JS value; used only for JSON maps/unknowns) |
| `{ "k": "ref", "name": "X" }` | a declared type |
| `{ "k": "array", "of": T }` | `T[]` |
| `{ "k": "union", "of": [T, …] }` | `A \| B` (WIG `A + B`) |
| `{ "k": "map", "of": T }` | string index signature `{ [k: string]: T }` |

Tuples become arrays of their element union; string/number literal unions become
`enumStrings`/`enum` types; `Record<string, unknown>` (and similar index
signatures over unknown JSON) is the only place `obj` is expected.

## Overrides (`overrides/*.json`)

For the constructs the extractor cannot flatten (generic methods, hand-curated
provider unions):

| Key | Effect |
|---|---|
| `drop` | remove generated methods by name |
| `dropTypes` | remove generated types by name |
| `types` | replace/add type definitions (same schema as the model) |
| `methods` | replace/add method definitions |
| `seedTypes` | register additional exported types the walk did not reach |
| `unsupportedResolved` | clear an `unsupported` entry once an override covers it |

Anything still listed under `unsupported` is a known gap and shows up with ⚠ in
`coverage.md` and in the generated binding's build log.

## Verification

The model is validated by construction: `CapGoGeneratorCommon` only ever sees
resolved refs, and a dangling ref fails the build rather than silently emitting
`obj`. `node extract.mjs --check` is the CI guard against upstream drift.