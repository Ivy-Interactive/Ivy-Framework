---
searchHints:
  - host-abstractions
  - abstractions-package
  - plugin-contract
  - extended-abstractions
  - two-tier
  - plugin-context
  - iivyplugincontext
---

# Host Abstractions

<Ingress>
The framework does not define what a plugin does — you do. That contract lives in its own NuGet package, which plugin authors reference and your host declares as shared.
</Ingress>

## Why Your Own Package

`IIvyPluginContext` gives a plugin a service collection and a configuration bag. Nothing in it says what your app can be extended with. That is the contract you write:

```csharp
namespace Acme.Plugins;

public interface IAcmePluginContext : IIvyPluginContext
{
    string DataRoot { get; }
    IAcmeHooks Hooks { get; }

    void RegisterExporter(IExporter exporter)
    {
        Services.AddSingleton(exporter);
    }
}
```

Three things to notice:

- **It derives from `IIvyPluginContext`**, so a plugin still gets `Services` and `Config`.
- **It is safe to grow.** Plugins *receive* the context; they never implement it. Adding members does not break existing plugins. (Contrast with interfaces plugins *do* implement — see [Version Compatibility](./06_Compatibility.md).)
- **Default interface methods make good registration helpers.** `RegisterExporter` above is a one-liner over `Services`, but it gives plugin authors an obvious entry point and gives you somewhere to add validation later.

Ship this in a package — `Acme.Plugin.Abstractions` — separate from your host application. Plugin authors reference the package; they never reference your host.

<Callout Type="tip">
Prefer extension methods over interface members where the behaviour is just a shortcut over `Services` or `Config`. Extension methods have no binary coupling to the interface, so they are the safest thing to add later.
</Callout>

```csharp
namespace Acme.Plugins;

public static class AcmePluginContextExtensions
{
    public static void RegisterExporters(this IAcmePluginContext context, params IExporter[] exporters)
    {
        foreach (var exporter in exporters)
            context.RegisterExporter(exporter);
    }
}
```

## Base and Extended Packages

Not every plugin needs UI. A plugin that registers an exporter, handles a lifecycle hook, or talks to an external API has no reason to take the entire Ivy framework as a dependency. So split your abstractions in two:

| Package | References | For plugins that |
|---------|------------|------------------|
| `Acme.Plugin.Abstractions` | `Ivy.Plugin.Abstractions` | contribute services, handlers, and integrations — no UI |
| `Acme.Plugin.Extended.Abstractions` | `Acme.Plugin.Abstractions` + `Ivy` | also register apps, widgets, or HTTP endpoints |

The base package:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <PackageId>Acme.Plugin.Abstractions</PackageId>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Ivy.Plugin.Abstractions" Version="1.3.16" />
  </ItemGroup>

</Project>
```

The extended package:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <PackageId>Acme.Plugin.Extended.Abstractions</PackageId>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="../Acme.Plugin.Abstractions/Acme.Plugin.Abstractions.csproj" />
    <PackageReference Include="Ivy" Version="1.3.16" />
  </ItemGroup>

</Project>
```

And the extended context inherits from both your base context and Ivy's:

```csharp
namespace Acme.Plugins.Extended;

public interface IAcmeExtendedPluginContext : IIvyExtendedPluginContext, IAcmePluginContext
{
    void TransformSettingsMenuItems(Func<IEnumerable<MenuItem>, IEnumerable<MenuItem>> transformer);
    Action RegisterDialog(string id, Func<IState<bool>, object?> factory);
}
```

That multiple inheritance is the whole point. Your host implements **one** context class, `IAcmeExtendedPluginContext`, and it satisfies both tiers at once — a base-tier plugin declaring `IIvyPlugin<IAcmePluginContext>` and an extended-tier plugin declaring `IIvyPlugin<IAcmeExtendedPluginContext>` both receive the same object and both type-test successfully.

<Callout Type="tip">
[Ivy Tendril](https://tendril.ivy.app) ships exactly this pair. Its base tier carries messaging channels, an inbox, and lifecycle hooks — things a headless integration plugin needs. Its extended tier adds menu transforms, sidebar badges, and dialogs, which need Ivy widgets.
</Callout>

## Wiring Both Tiers into the Host

Your host references both packages and declares both as shared assemblies:

```csharp
server.UsePlugins(pluginsDir, configFactory,
    contextFactory: (s, builder) => new AcmePluginContext(s, builder, dataRoot),
    sharedAssemblyNames: ["Acme.Plugin.Abstractions", "Acme.Plugin.Extended.Abstractions"]);
```

Both names are required even though only one context class exists, because types from both assemblies cross the boundary. See [Host-Provided Packages](./02_HostingPlugins.md#host-provided-packages).

## Namespaces

Put your context and contract types in your own namespace and keep it stable — plugin authors take a `using` on it, and moving a type between namespaces is a breaking change even when the type name is unchanged.

If you split base and extended into different namespaces, an extended-tier plugin needs a `using` for each. Using one namespace for both tiers avoids that and is recommended.

## Versioning the Pair Together

Give both packages a single version number and publish them together. The simplest way is one `Directory.Build.props` above both projects:

```xml
<Project>
  <PropertyGroup>
    <Version>1.4.0</Version>
  </PropertyGroup>
</Project>
```

## Developing Against Local Ivy Sources

While your abstractions package is under active development you often want to build against a local checkout of the framework, but always against published packages in CI and for releases. A conditional reference handles both:

```xml
<PropertyGroup>
  <!-- Use local Ivy sources for development if they exist; NuGet packages in CI and for releases -->
  <IvySource Condition="'$(IvySource)' == '' And Exists('$(MSBuildThisFileDirectory)../../Ivy-Framework/src/Ivy/Ivy.csproj')">true</IvySource>
  <IvySource Condition="'$(IvySource)' == ''">false</IvySource>
  <IvySource Condition="'$(GITHUB_ACTIONS)' == 'true' Or '$(Publishing)' == 'true'">false</IvySource>
</PropertyGroup>
```

```xml
<ItemGroup Condition="'$(IvySource)' == 'true'">
  <ProjectReference Include="../../../Ivy-Framework/src/Ivy.Plugin.Abstractions/Ivy.Plugin.Abstractions.csproj" />
</ItemGroup>

<ItemGroup Condition="'$(IvySource)' != 'true'">
  <PackageReference Include="Ivy.Plugin.Abstractions" Version="1.3.16" />
</ItemGroup>
```

<Callout Type="warning">
A source build without assembly versioning produces version `0.0.0.0`, which the framework's shared-assembly check skips. That is deliberate — it keeps local development working — but it means a compatibility problem you would catch in CI can pass unnoticed on your machine.
</Callout>

## See Also

- [Hosting Plugins](./02_HostingPlugins.md)
- [Writing Plugins](./04_WritingPlugins.md)
- [Version Compatibility](./06_Compatibility.md)
- [Plugins Overview](./01_PluginsOverview.md)
