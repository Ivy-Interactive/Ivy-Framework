---
searchHints:
  - plugin-discovery
  - plugin-install
  - plugin-update
  - plugin-uninstall
  - plugin-references
  - plugin-marketplace
  - nuget-plugin
---

# Distributing Plugins

<Ingress>
The framework finds plugins on disk. Getting them onto disk is the host's job.
</Ingress>

## What the Framework Discovers

`UsePlugins` takes a plugins directory and looks in two places inside it.

**Subdirectories.** Every immediate subdirectory of the plugins directory is a candidate. The loader scans its DLLs for the `[IvyPlugin]` attribute:

```text
plugins/
├── plugin-config.yaml
├── plugin-references.yaml
├── Acme.Plugin.Exporter/
│   ├── Acme.Plugin.Exporter.dll
│   └── lib/net10.0/SomeDependency.dll
└── Acme.Plugin.Dashboard/
    └── Acme.Plugin.Dashboard.dll
```

**`plugin-references.yaml`.** A YAML list of paths, each absolute or relative to the plugins directory. These are loaded first and take priority over subdirectories:

```text
- /Users/me/code/acme-plugin-exporter
- ../shared-plugins/Acme.Plugin.Dashboard
```

The file is watched. Adding a line loads that plugin; removing a line unloads it. This is how you point a running host at a plugin you are developing, without packaging or copying anything.

Both mechanisms are watched, which means the entire installation contract the framework asks of you is: **put the plugin's files in a directory, and it appears.**

<Callout Type="info">
With `enableHotReload` on, a plugin is reloaded when its DLLs change. Write plugin files to a temporary directory first and move the finished directory into place — otherwise the watcher may pick up a half-written plugin and fail to load it.
</Callout>

## What the Host Provides

Everything else:

- a catalog of available plugins
- downloading and unpacking
- resolving the plugin's dependencies
- knowing which version is installed
- checking for and applying updates
- verifying integrity, and any approval or trust policy
- uninstalling

None of this is in the framework, because none of it has one right answer. A host distributing internal plugins to a handful of teams needs almost none of it; a host with a public plugin marketplace needs all of it plus review workflow.

## A Scheme That Works

The rest of this page describes one concrete approach — NuGet as the package format, a small catalog service in front of it, and installation into the plugins directory. It is the shape [Ivy Tendril](https://github.com/Ivy-Interactive/Ivy-Tendril) uses in production and it maps cleanly onto what the framework expects.

### Catalog

Publish plugins as NuGet packages. Keep a service that returns the plugins you have approved, with their current versions — a table of package id, version, title, icon, and content hash is enough. Then provide endpoints to fetch data from that table.

Consider also offering an escape hatch: install any NuGet package id, or reference a local folder.

### Install

```csharp
var nupkgUrl = $"https://api.nuget.org/v3-flatcontainer/{packageId}/{version}/{packageId}.{version}.nupkg";
```

1. Download the `.nupkg`.
2. Extract it into a **temporary** directory, not the plugins directory — the watcher would otherwise try to load it mid-extraction.
3. Resolve dependencies (below).
4. `Directory.Move` the finished directory to `plugins/{packageId}`.
5. Call `IPluginManager.LoadPlugin(pluginDirectory)`.

Naming the directory after the package id means you can later find the installed plugin from a catalog entry without a separate index. Keep the `.nuspec` in the installed directory too — reading it back is the simplest way to answer "which version is installed?".

<Callout Type="warning">
Guard against path traversal when extracting. Resolve each entry's destination and skip anything that does not sit under your temporary directory.
</Callout>

### Dependencies

A plugin's transitive dependencies need to be next to its DLL, but **not** the ones the host already provides. Walk the `.nuspec` dependency graph and skip:

- every name in your `sharedAssemblyNames`, plus `Ivy`, `Ivy.Plugin.Abstractions`, and the `Microsoft.Extensions.*` assemblies the framework always shares
- anything already present in `AppContext.BaseDirectory` or the runtime directory

This is the install-time counterpart to the load-context rule in [Host-Provided Packages](./02_HostingPlugins.md#host-provided-packages). The load context would ignore a duplicate copy of your abstractions anyway; skipping it at install time means the bytes are never downloaded and a plugin directory stays small.

Pick the best target framework available in each package (`net10.0` down through `netstandard2.0`) and drop the assemblies into the plugin's `lib/<tfm>/` folder.

### Update

1. Ask the catalog what the current version is for each installed package id.
2. Download the new `.nupkg` and **verify its hash** against the one the catalog reported before touching anything on disk.
3. `UnloadPlugin(pluginId)`, replace the directory, `LoadPlugin(pluginDirectory)`.

Serialize updates behind a lock — two concurrent updates to the same directory will not end well. Note the plugin's `ShutdownAsync` runs with `PluginShutdownReason.Unload` during step 3, so a plugin can close its connections cleanly.

### Uninstall

How a plugin was installed determines how it comes out, and you can infer it from where it lives:

| Installed as | Uninstall by |
|--------------|--------------|
| A subdirectory of the plugins directory | Unload, then delete the directory |
| A path in `plugin-references.yaml` | Unload, then remove the line — leave the files alone, they belong to the developer |

Optionally remove the plugin's section from your configuration store at the same time.

### Catalog Metadata

A plugin's `PluginManifest` lives in C# and is only readable after the plugin is loaded — which is too late to render a catalog entry for something not yet installed. If your catalog needs an icon or a blurb up front, pack a small metadata file into the `.nupkg` root and read it out of the archive at submission time (for instance, Ivy Tendril calls its metadata files `tendril.json`):

```json
{
  "icon": {
    "kind": "named",
    "value": "FileSpreadsheet"
  }
}
```

<Callout Type="tip">
If you do this, treat the packed file and `PluginManifest` as one fact that must agree, and validate it when a plugin is submitted. Two sources for the same icon could drift.
</Callout>

## See Also

- [Hosting Plugins](./02_HostingPlugins.md)
- [Writing Plugins](./04_WritingPlugins.md)
- [Version Compatibility](./06_Compatibility.md)
