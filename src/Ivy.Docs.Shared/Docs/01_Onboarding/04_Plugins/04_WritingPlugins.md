---
searchHints:
  - writing-plugins
  - iivyplugin
  - ivyplugin-attribute
  - pluginmanifest
  - schemabuilder
  - plugin-configuration
  - buildconfigurationview
  - useendpoints
  - plugin-shutdown
---

# Writing Plugins

<Ingress>
A plugin is a plain class library with one attribute and one class. This page is the reference you can hand to plugin authors, or fold into your own host's plugin guide.
</Ingress>

## The Project

A plugin project is a `Microsoft.NET.Sdk` class library with a single reference to the host's abstractions package:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
    <PackageId>Acme.Plugin.Exporter</PackageId>
    <Title>CSV Exporter</Title>
    <Description>Exports records as CSV</Description>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Acme.Plugin.Abstractions" Version="1.4.0" />
  </ItemGroup>

</Project>
```

`CopyLocalLockFileAssemblies` matters, particularly for in-development plugins automatically built from source: the plugin's dependencies must sit next to its DLL for the load context to resolve them.

Reference the extended abstractions package instead if the plugin registers apps, widgets, or HTTP endpoints.

<Callout Type="warning">
A plugin that calls `UseEndpoints` also needs a `FrameworkReference` to `Microsoft.AspNetCore.App`. Without it the endpoint types won't resolve at compile time.
</Callout>

## The Entry Point

One assembly-level attribute names the plugin type.

```csharp
using Ivy.Plugins;

[assembly: IvyPlugin(typeof(Acme.Plugin.Exporter.ExporterPlugin))]
```

## The Plugin Class

Implement `IIvyPlugin<TContext>` rather than the non-generic `IIvyPlugin`, where `TContext` is the context your plugin needs:

```csharp
using Acme.Plugins;
using Ivy.Plugins;

namespace Acme.Plugin.Exporter;

public class ExporterPlugin : IIvyPlugin<IAcmePluginContext>
{
    public PluginManifest Manifest { get; } = new()
    {
        Id = "Acme.Plugin.Exporter",
        Title = "CSV Exporter",
        Icon = PluginIcon.Named("FileSpreadsheet"),
    };

    public PluginConfigurationSchema? ConfigurationSchema { get; } = new SchemaBuilder()
        .AddString("Delimiter", defaultValue: ",", description: "Field separator")
        .AddBoolean("IncludeHeader", defaultValue: true, description: "Write a header row")
        .Build();

    public void Configure(IAcmePluginContext context)
    {
        var delimiter = context.Config.GetValue("Delimiter") ?? ",";
        var includeHeader = context.Config.GetBool("IncludeHeader") ?? true;

        context.RegisterExporter(new CsvExporter(delimiter, includeHeader));
    }
}
```

The generic interface exists for one reason: it type-tests the context for you. If the plugin is loaded by a host that supplies a different context, the failure is a clear message naming both the required and the supplied type, at load time, instead of an `InvalidCastException` somewhere in your `Configure` body.

Constructor injection works — the plugin type is created with `ActivatorUtilities`, so a constructor parameter for a service the host registered before `UsePlugins` is resolved.

## The Manifest

```csharp
public record PluginManifest
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public Version? MinimumHostVersion { get; init; }
    public PluginIcon? Icon { get; init; }
}
```

| Property | Notes |
|----------|-------|
| `Id` | Unique across loaded plugins. Make it match your `PackageId` |
| `Title` | Shown in plugin management UI |
| `MinimumHostVersion` | Optional floor. The plugin is refused, with a reason, on older hosts |
| `Icon` | `PluginIcon.Named("Star")` for an icon from the `Icons` enum, or `PluginIcon.Url("https://…")` for an image |

<Callout Type="info">
Matching `Id` to `PackageId` is a convention worth keeping. Hosts that install plugins from NuGet typically use the package id as the directory name, and then identify the installed plugin by that directory — a mismatch quietly breaks update detection.
</Callout>

## Configuration

Declare the fields the plugin needs with `SchemaBuilder`:

```csharp
public PluginConfigurationSchema? ConfigurationSchema { get; } = new SchemaBuilder()
    .AddString("Endpoint", defaultValue: "https://api.example.com", description: "API base URL", isRequired: true)
    .AddSecret("ApiKey", description: "API key", isRequired: true)
    .AddInteger("MaxRetries", defaultValue: 3, description: "Retry attempts")
    .AddBoolean("Verbose", defaultValue: false)
    .Build();
```

| Method | Field type |
|--------|-----------|
| `AddString` | `String` |
| `AddSecret` | `Secret` — the host is told to treat the value as sensitive |
| `AddInteger` | `Integer` |
| `AddBoolean` | `Boolean` |

Return `null` from `ConfigurationSchema` if the plugin needs no configuration.

Read values through `context.Config`. `GetValue` returns the raw string; `GetInt`, `GetBool`, and `Get<T>` parse it and return `null` when the value is missing or malformed:

```csharp
var endpoint = context.Config.GetValue("Endpoint")!;
var maxRetries = context.Config.GetInt("MaxRetries") ?? 3;
```

The `!` is safe on a required field. The framework validates configuration **before** calling `Configure`: a plugin with a missing or unparseable required field is held in `PluginStatus.Unconfigured` and `Configure` is never called. Declared defaults are supplied automatically. So a plugin's own code never has to defend against half-configured state.

### A Custom Configuration View

By default the host renders a form generated from the schema. Override it by returning a [view](../02_Concepts/02_Views.md) from `BuildConfigurationView` — this requires the extended abstractions package, since it returns Ivy widgets:

```csharp
public object? BuildConfigurationView(IIvyPluginConfig config) =>
    new ExporterConfigView(config);

public class ExporterConfigView(IIvyPluginConfig config) : ViewBase
{
    public override object? Build()
    {
        var endpoint = UseState(config.GetValue("Endpoint") ?? "");
        var apiKey = UseState(config.GetValue("ApiKey") ?? "");
        var saved = UseState(false);

        return Layout.Vertical().Gap(4)
            | new Field(endpoint.ToTextInput(placeholder: "https://api.example.com"),
                        label: "Endpoint", required: true)
            | new Field(apiKey.ToTextInput(variant: TextInputVariant.Password),
                        label: "API Key", required: true)
            | new Button("Save", onClick: _ =>
            {
                config.SetValue("Endpoint", endpoint.Value);
                config.SetValue("ApiKey", apiKey.Value);
                config.Save();
                saved.Set(true);
                return ValueTask.CompletedTask;
            }, icon: Icons.Check)
            | (saved.Value ? Callout.Success("Saved") : null);
    }
}
```

`config.Save()` is what re-activates the plugin with the new values. Calling `SetValue` without `Save` leaves the plugin running on its old configuration.

## Shutdown

`ShutdownAsync` is optional — it has a default no-op implementation. Implement it if the plugin holds connections, background loops, or file handles:

```csharp
public async Task ShutdownAsync(PluginShutdownContext context)
{
    context.Logger.LogInformation("Exporter shutting down: {Reason}", context.Reason);
    await _client.DisposeAsync();
}
```

`Reason` tells you why:

| Reason | Meaning |
|--------|---------|
| `Unload` | The plugin is being unloaded |
| `Reload` | The plugin is being replaced with a new build |
| `Reconfigure` | Configuration changed and the plugin is being re-configured |
| `HostExit` | The host process is stopping |

Respect `context.CancellationToken` — the host allows roughly five seconds before moving on.

## Contributing to an Ivy Host

With the extended context, a plugin can add apps and endpoints.

### Apps

```csharp
public class DashboardPlugin : IIvyPlugin<IAcmeExtendedPluginContext>
{
    public PluginManifest Manifest { get; } = new()
    {
        Id = "Acme.Plugin.Dashboard",
        Title = "Dashboard",
    };

    public PluginConfigurationSchema? ConfigurationSchema => null;

    public void Configure(IAcmeExtendedPluginContext context)
    {
        context.AddApp(new AppDescriptor
        {
            Id = "acme-dashboard",
            Title = "Dashboard",
            Icon = Icons.ChartLine,
            Group = ["Reports"],
            IsVisible = true,
            ViewFactory = () => new DashboardView(),
        });
    }
}
```

Use `AddAppsFromAssembly(typeof(DashboardPlugin).Assembly)` instead to pick up every class in the plugin marked with `[App]`.

The apps are removed again when the plugin is unloaded.

### HTTP Endpoints

`UseEndpoints` mounts a route group at `/ivy/plugins/{slug}`. The slug must be lowercase alphanumeric with dashes, and only one plugin may claim a given slug:

```csharp
context.UseEndpoints("exporter", endpoints =>
{
    endpoints.MapGet("formats", () => Results.Ok(new[] { "csv", "tsv" }));
    endpoints.MapStaticAssets("assets");
});
```

`MapStaticAssets(subPath)` serves files from the plugin's own installed directory — useful for shipping a widget bundle or images with the plugin. It only works on the builder handed to `UseEndpoints`, which is what knows where the plugin lives.

Endpoints are unmapped when the plugin is unloaded.

### Widgets

A plugin can ship an [external widget](../../02_Widgets/07_Advanced/05_ExternalWidgets.md). The framework registers the plugin assembly's widgets on load and unregisters them on unload, so no extra call is needed — build the frontend bundle into the plugin as usual and use the widget from a view the plugin registers.

## Local Development

To work on a plugin against a running host without packaging it, add its directory to `plugin-references.yaml` in the host's plugins directory:

```text
- /Users/me/code/acme-plugin-exporter
```

With `buildSourcePlugins: true` on the host, saving a `.cs` file rebuilds and reloads the plugin. Removing the line unloads it. See [Distributing Plugins](./05_DistributingPlugins.md).

## Troubleshooting

| Issue | What to check |
|-------|---------------|
| "No `[IvyPlugin]` attribute found" | The attribute is missing, or is on a type rather than the assembly |
| "Multiple `[IvyPlugin]` attributes found" | Two attributes in one assembly — only one plugin per assembly |
| "requires context type X but the host provided Y" | The plugin's `TContext` isn't implemented by this host's context, or the abstractions assembly isn't shared |
| "Incompatible plugin type" | Built against a different version of the abstractions than the host provides — see [Version Compatibility](./06_Compatibility.md) |
| Plugin listed as unconfigured | A required field is missing or unparseable; the host's settings form shows the validation errors |
| Config saved but nothing changed | `config.Save()` was not called, or the host's `IIvyPluginConfig.Save` doesn't call `ReconfigurePlugin` |
| A dependency fails to load at runtime | `CopyLocalLockFileAssemblies` is not set, so the dependency isn't next to the plugin DLL |

## See Also

- [Host Abstractions](./03_HostAbstractions.md)
- [Distributing Plugins](./05_DistributingPlugins.md)
- [Version Compatibility](./06_Compatibility.md)
- [Apps](../02_Concepts/10_Apps.md)
- [External Widgets](../../02_Widgets/07_Advanced/05_ExternalWidgets.md)
