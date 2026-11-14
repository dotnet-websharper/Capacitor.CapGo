// Generates the per-family binding projects, their npm pins and their paket
// templates from tools/plugins.json + tools/model/*.json.
//
// Everything it writes is idempotent: rerunning after `extract.mjs` refreshes
// versions and pins without touching the generator itself.
import { mkdirSync, readFileSync, writeFileSync, existsSync } from "node:fs";
import { join } from "node:path";

const root = new URL("../", import.meta.url).pathname;
const roster = JSON.parse(readFileSync(join(root, "tools/plugins.json"), "utf8"));
const models = new Map();
for (const family of Object.keys(roster.families)) {
  models.set(family, []);
}

import { readdirSync } from "node:fs";
for (const f of readdirSync(join(root, "tools/model"))) {
  if (!f.endsWith(".json")) continue;
  const m = JSON.parse(readFileSync(join(root, "tools/model", f), "utf8"));
  if (models.has(m.family)) models.get(m.family).push(m);
}

const major = v => Number(v.split(".")[0]);
const esc = s => s.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");

function projectFiles(family) {
  const id = roster.families[family];
  const dir = join(root, id);
  mkdirSync(dir, { recursive: true });

  writeFileSync(join(dir, "Main.fs"), `namespace ${id}

open WebSharper
open WebSharper.InterfaceGenerator
open CapGoGeneratorCommon

module Definition =
    /// The plugins of this package. The surface itself comes from
    /// tools/model/*.json (see tools/README.md).
    let plugins =
        Model.loadDir (__SOURCE_DIRECTORY__ + "/../tools/model")
        |> List.filter (fun p -> p.Family = "${family}")

    let Assembly =
        Generator.getAssembly {
            RootNamespace = "${id}"
            RootClass = "CapacitorCapGo${family === "Core" ? "" : family}"
            Plugins = plugins
        }

[<Sealed>]
type Extension() =
    interface IExtension with
        member ext.Assembly = Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()
`);

  const pins = models
    .get(family)
    .sort((a, b) => a.package.localeCompare(b.package))
    .map(m => `\t\t\t<NpmPackage Name="${m.package}" Version="&gt;= ${m.version} &lt; ${major(m.version) + 1}.0.0" ResolutionStrategy="Max" />`)
    .join("\n");

  writeFileSync(join(dir, `${id}.fsproj`), `<?xml version="1.0" encoding="utf-8"?>
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <Product>${id}</Product>
    <Title>https://github.com/dotnet-websharper/capacitor.capgo/</Title>
    <Company>IntelliFactory</Company>
    <Copyright>(c) 2023 IntelliFactory</Copyright>
  </PropertyGroup>

  <PropertyGroup>
    <NpmDependencies>
${pins}
    </NpmDependencies>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include="Main.fs" />
    <None Include="wsconfig.json" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\\CapGoGeneratorCommon\\CapGoGeneratorCommon.fsproj" />
    <!-- The binding surface is generated from these models; without them MSBuild
         would happily reuse a stale WebSharper metadata cache. -->
    <CustomAdditionalCompileInputs Include="..\\tools\\model\\*.json" />
  </ItemGroup>
  <Import Project="..\\.paket\\Paket.Restore.targets" />
</Project>
`);

  writeFileSync(join(dir, "paket.references"), "WebSharper\nWebSharper.FSharp\nFSharp.Core");
  writeFileSync(join(dir, "wsconfig.json"), `{
  "$schema": "https://websharper.com/wsconfig.schema.json",
  "project": "binding"
}
`);

  const template = `type file
id ${id}
authors
    IntelliFactory
owners
    IntelliFactory
repositoryType git
repositoryUrl https://github.com/dotnet-websharper/capacitor.capgo/
licenseUrl https://github.com/dotnet-websharper/capacitor.capgo/blob/main/LICENSE.md
iconUrl https://github.com/dotnet-websharper/core/raw/websharper50/tools/WebSharper.png
projectUrl https://github.com/dotnet-websharper/capacitor.capgo
description
    WebSharper bindings for CapGo Capacitor plugins (${family})
tags
    Web JavaScript F# C#
files
    ../${id}/bin/Release/netstandard2.0/${id}.dll ==> lib/netstandard2.0
    ../${id}/bin/Release/netstandard2.0/${id}.websharper ==> lib/netstandard2.0
    ../${id}/bin/Release/netstandard2.0/${id}.xml ==> lib/netstandard2.0
    ../${id}/${id}.fsproj ==> src/
dependencies
        WebSharper ~> LOCKEDVERSION:[3]
`;
  writeFileSync(join(root, "nuget", `${id}.paket.template`), template);

  console.log(`${family}: ${id} (${models.get(family).length} plugins)`);
}

for (const family of Object.keys(roster.families)) projectFiles(family);

// Remove the stale, misnamed template (its id was already CapGo).
const stale = join(root, "nuget/WebSharper.Capacitor.Community.paket.template");
if (existsSync(stale)) {
  const { unlinkSync } = await import("node:fs");
  unlinkSync(stale);
  console.log("removed stale nuget/WebSharper.Capacitor.Community.paket.template");
}