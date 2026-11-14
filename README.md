# WebSharper Capacitor CapGo

[![NuGet](https://img.shields.io/nuget/v/WebSharper.Capacitor.CapGo.svg)](https://www.nuget.org/packages/WebSharper.Capacitor.CapGo)

F# bindings for the [CapGo](https://capgo.app/) Capacitor plugin collection, for use with
[WebSharper](https://websharper.com/). They let you build cross-platform web, Android and iOS
apps in F# on top of CapGo's plugins.

The surface is **generated** from each plugin's published TypeScript declarations
(`tools/extract.mjs` → `tools/model/*.json` → `CapGoGeneratorCommon`), so the bindings track
upstream instead of drifting: 148 of the 180 `@capgo` packages, across 11 packages, typed
method-by-method — 99% of 9,953 type positions are concrete, the rest being upstream
`any`/`unknown` JSON payloads.

## Live demos

**Try it live:** [Open the sample in your browser](https://dotnet-websharper.github.io/Capacitor.CapGo/)

![Capacitor CapGo sample](docs/sample.png)

The sample doubles as the binding's [test case](#running-the-test-case): one demo per family
package, calling the real plugin API and reporting the outcome — or the native-only error —
instead of failing silently.

## Packages

Each family ships as its own package so a small app only installs the npm packages it uses.

| Package | Plugins | Examples |
|---|---:|---|
| `WebSharper.Capacitor.CapGo` | 17 | updater, social-login, native-purchases, data-storage-sqlite, uploader, downloader, navigation-bar, native-audio |
| `WebSharper.Capacitor.CapGo.Auth` | 21 | passkey, native-biometric, app-attest, permissions, ssl-pinning, nfc |
| `WebSharper.Capacitor.CapGo.Media` | 16 | audio-recorder, audio-session, camera-preview, document-scanner, pdf-generator |
| `WebSharper.Capacitor.CapGo.Files` | 8 | file, file-picker, file-sharer, zip, asset-cache, fast-sql |
| `WebSharper.Capacitor.CapGo.Device` | 27 | accelerometer, pedometer, bluetooth-low-energy, health, screen-orientation, contacts |
| `WebSharper.Capacitor.CapGo.UI` | 16 | inappbrowser, native-navigation, date-picker, pretty-toast, kiosk |
| `WebSharper.Capacitor.CapGo.Commerce` | 7 | pay, stripe-*, admob, in-app-review |
| `WebSharper.Capacitor.CapGo.Analytics` | 20 | firebase-* (9), appsflyer, rudderstack, gtm, supabase |
| `WebSharper.Capacitor.CapGo.Messaging` | 9 | notifications, rich-notifications, twilio-voice, stream-call, intercom |
| `WebSharper.Capacitor.CapGo.AI` | 3 | llm, speech-recognition, speech-synthesis |
| `WebSharper.Capacitor.CapGo.Tooling` | 4 | env, sim, live-reload, auto |

The full roster — package, current npm version, method/event counts and any known gap — is in
[`tools/coverage.md`](tools/coverage.md).

Plugin types live under `WebSharper.Capacitor.CapGo.<Plugin>`; each package also exposes a root
class (`CapacitorCapGo`, `CapacitorCapGoAuth`, `CapacitorCapGoMedia`, …) with one property per
plugin, mirroring how the official `WebSharper.Capacitor` binding is used.

## Coverage: what is bound and what is not

The `@capgo` npm scope contains **180 packages**. **148 are bound** across the 11 packages above.
Typing coverage, per-plugin gaps and the remaining `obj` positions are tracked in
[`tools/coverage.md`](tools/coverage.md).

### Not bound (32)

None of these are missing capability by accident:

| Reason | Packages |
|---|---|
| **Not Capacitor plugins** — CLI, release tooling, JS/Angular libraries, DOM-only libraries, Electron/Cordova | `cli`, `cli-helper-darwin-arm64`, `cli-helper-darwin-x64`, `cli-keychain-darwin-arm64`, `cli-keychain-darwin-x64`, `standard-version`, `capacitor-standard-version`, `capacitor-plugin-standard-version`, `find-package-manager`, `manifest-packing`, `capgo-sec`, `capacitor-sec`, `seo-checker`, `capgo-skills`, `vite-capacitor`, `tailwind-capacitor`, `electron-updater`, `cordova-updater`, `angular-ng-stepper`, `ngx-intl-tel-input`, `ngx-intl-tel-input-app`, `google-libphonenumber`, `google-play-scraper`, `capacitor-sheets`, `capacitor-transitions`, `transitions` |
| **Legacy name, superseded by a bound package** | `keep-awake` → `capacitor-keep-awake`; `native-navigation` → `capacitor-native-navigation` |
| **No plugin instance to bind** (different export shape; needs a hand-written model) | `capacitor-native-map` (web-component class), `capacitor-patch` (function exports) |
| **Not usable on the Capacitor 8 line** | `walletconnect` (WIP `0.0.1`, peer `^3.0.0`) |
| **Discontinued upstream** | `capacitor-twilio-video` (Twilio Video end-of-life; `capacitor-twilio-voice` is bound) |

Known gaps *inside* bound plugins (generic or JSON-heavy members that need a hand-written
override) are listed with ⚠ in [`tools/coverage.md`](tools/coverage.md).

> The [CapGo plugin catalog](https://capgo.app/plugins/) advertises “158+ plugins”, but it mixes
> scopes: it includes `@revenuecat/purchases-capacitor` (not CapGo's) and the four
> `@capacitor-plus/*` packages (a Capacitor fork, not plugins). Within the `@capgo` scope the
> authoritative count is the 180 above.

## Install

```sh
dotnet add package WebSharper.Capacitor.CapGo        # or one of the family packages
```

Install the npm plugin you actually use in your Capacitor app:

```sh
npm i @capgo/capacitor-flash
npx cap sync
```

The binding imports the plugin from its npm package, so the bundler resolves it; there is no
CDN script.

## Usage

```fsharp
open WebSharper
open WebSharper.JavaScript
open WebSharper.Capacitor.CapGo

[<JavaScript>]
let checkTorch () =
    promise {
        let! available = CapacitorCapGo.CapacitorFlash.IsAvailable()
        let! on = CapacitorCapGo.CapacitorFlash.IsSwitchedOn()
        return if available.Value then sprintf "torch on=%b" on.Value else "no torch"
    }
```

Method and event shapes follow the plugin's declaration: optional options objects, typed
`Promise<'T>` results, and one `AddListener` overload per event (the event name is a single-case
enum), e.g.:

```fsharp
let! _ =
    CapacitorCapGo.CapacitorUpdater.AddListener(
        CapacitorUpdater.CapacitorUpdaterDownloadEventName.download,
        fun (e: CapacitorUpdater.DownloadEvent) -> printfn "%d%%" e.Percent)
```

## Running the test case

```sh
dotnet build WebSharper.Capacitor.CapGo.Sample/WebSharper.Capacitor.CapGo.Sample.fsproj -c Release
dotnet serve -d WebSharper.Capacitor.CapGo.Sample/dist -p 8080
```

Install the `dotnet-serve` tool once with `dotnet tool install -g dotnet-serve`.

Most CapGo plugins are native-only: in a plain browser their calls reject and the sample's log
reports that. To exercise the native paths, run the bundled site inside a Capacitor shell:

```sh
cd WebSharper.Capacitor.CapGo.Sample
npm i @capacitor/core @capacitor/cli
npx cap add android   # or ios
npx cap sync
```

## Regenerating the bindings

The bindings are not hand-written. After an upstream release:

```sh
cd tools
npm install
node extract.mjs            # refreshes tools/model/*.json and tools/coverage.md
node sync-projects.mjs      # refreshes the npm pins and package templates
```

`node extract.mjs --check` exits non-zero when a committed model is out of date; that is the CI
guard. Anything the extractor cannot flatten (generic methods, hand-curated unions) lives in
`tools/overrides/*.json` and is listed under `unsupported` in the coverage table.

## Build from source

```sh
dotnet tool restore
dotnet paket install
dotnet build WebSharper.Capacitor.CapGo.sln -c Release
```

WebSharper 11 needs the .NET 11 runtime for `wsfsc`.

## Pack

```sh
WSPackageFolder=/tmp/pkg dotnet fsi ./build.fsx -t WS-Package
```

One `.nupkg` is produced per project (`nuget/*.paket.template`), all on the same version line.

## Deployment

`.github/workflows/ghpages.yml` builds the sample in Release (which runs `npm install` and
`vite build`) and publishes `WebSharper.Capacitor.CapGo.Sample/dist` to the `gh-pages` branch.
Enable Pages per repo with the `gh-pages` source.

## Repository layout

| Path | Purpose |
|---|---|
| `tools/` | d.ts extractor, JSON models, overrides, roster, project/template sync |
| `CapGoGeneratorCommon/` | model → WIG assembly (not packed) |
| `WebSharper.Capacitor.CapGo*/` | the binding packages (one per family) |
| `WebSharper.Capacitor.CapGo.Sample/` | SPA sample + Vite build + test case |
| `nuget/` | paket templates (one per package) |

## License

Apache 2.0 — see [LICENSE.md](LICENSE.md).