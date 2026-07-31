---
searchHints:
  - useplugins
  - plugin-host
  - shared-assemblies
  - host-provided-packages
  - plugin-config
  - iivypluginconfigfactory
  - ipluginmanager
  - plugincontextbase
  - plugin-hot-reload
---

# Hosting Plugins

<Ingress>
Turn your Ivy app into a plugin host with one call on the Server, then supply the two things the framework leaves to you: where configuration lives, and which assemblies form your contract.
</Ingress>

## Enabling Plugins

`UsePlugins` is the single entry point on [Server](../01_Program.md):

```csharp
public Server UsePlugins(
    string pluginsDirectory,
    IIvyPluginConfigFactory configFactory,
    Version? hostVersion = null,
    Func<Server, WebApplicationBuilder, PluginContextBase>? contextFactory = null,
    IEnumerable<string>? sharedAssemblyNames = null,
    bool enableHotReload = true,
    bool buildSourcePlugins = false,
    bool deferPluginLoads = false)
```

| Parameter | Description |
|-----------|-------------|
| `pluginsDirectory` | Directory scanned for plugins. Create it if it doesn't exist |
| `configFactory` | Your configuration store — see [Storing Configuration](#storing-configuration) |
| `hostVersion` | Version plugins are gated against. Defaults to the entry assembly's version |
| `contextFactory` | Returns your own context type — see [Extending the Context](#extending-the-context) |
| `sharedAssemblyNames` | Your contract assemblies — see [Host-Provided Packages](#host-provided-packages) |
| `enableHotReload` | Reload a plugin when its files change |
| `buildSourcePlugins` | Run `dotnet build` on plugin directories that contain a `.csproj` |
| `deferPluginLoads` | Load plugins in the background instead of blocking startup |

The smallest useful host:

```csharp
var server = new Server();
server.UseAppShell(new AppShellSettings());
server.AddAppsFromAssembly(typeof(Program).Assembly);

var pluginsDir = Path.Combine(AppContext.BaseDirectory, "plugins");
Directory.CreateDirectory(pluginsDir);

server.UsePlugins(pluginsDir, new AcmePluginConfigFactory(pluginsDir));

await server.RunAsync();
```

## Storing Configuration

The framework declares what configuration a plugin needs and validates it, but never decides where it lives. You implement `IIvyPluginConfigFactory` and `IIvyPluginConfig`:

```csharp
public interface IIvyPluginConfigFactory
{
    IIvyPluginConfig Create(string pluginId);
    void SetPluginManager(IPluginManager pluginManager) { }
}

public interface IIvyPluginConfig
{
    string? GetValue(string key);
    void SetValue(string key, string value);
    void RemoveValue(string key);
    void Save();

    // GetInt, GetBool and Get<T> have default implementations
}
```

A file-backed implementation keyed by plugin id:

```csharp
public class AcmePluginConfigFactory(string pluginsDir) : IIvyPluginConfigFactory
{
    private readonly string _configPath = Path.Combine(pluginsDir, "plugin-config.yaml");
    private IPluginManager? _pluginManager;

    public void SetPluginManager(IPluginManager pluginManager) => _pluginManager = pluginManager;

    public IIvyPluginConfig Create(string pluginId) =>
        new AcmePluginConfig(_configPath, pluginId, () => _pluginManager);
}

public class AcmePluginConfig(string configPath, string pluginId, Func<IPluginManager?> getPluginManager)
    : IIvyPluginConfig
{
    public string? GetValue(string key) => Read().GetValueOrDefault(key);

    public void SetValue(string key, string value)
    {
        var values = Read();
        values[key] = value;
        Write(values);
    }

    public void RemoveValue(string key)
    {
        var values = Read();
        if (values.Remove(key)) Write(values);
    }

    public void Save() => getPluginManager()?.ReconfigurePlugin(pluginId);

    private Dictionary<string, string> Read() { /* read the section for pluginId */ }
    private void Write(Dictionary<string, string> values) { /* write the section for pluginId */ }
}
```

Two details matter:

- **`Save()` must call `ReconfigurePlugin`.** A plugin whose required fields are missing is held in `PluginStatus.Unconfigured` and its `Configure` method is never called. `ReconfigurePlugin` re-validates and activates it. Without this, saving configuration in your UI appears to do nothing.
- **`SetPluginManager` is how the factory gets the manager.** It has a default (no-op) implementation, so implementing it is optional — but you need it for the point above.

<Callout Type="tip">
Store secret fields (`ConfigFieldType.Secret`) wherever you keep the rest of your app's secrets. The framework tells you which fields are secret; it does not encrypt them for you.
</Callout>

## Host-Provided Packages

Each plugin is loaded into its own `AssemblyLoadContext`. By default that means the plugin's copy of an assembly is a *different* assembly from the host's copy, even at the same version — so a cast to one of your interfaces fails even though the code compiled.

`sharedAssemblyNames` fixes that. It is a list of simple assembly names that must resolve from the host instead of from the plugin's directory:

```csharp
internal class PluginAssemblyLoadContext(string pluginPath, IReadOnlySet<string> sharedAssemblyNames)
    : AssemblyLoadContext(isCollectible: true)
{
    private readonly AssemblyDependencyResolver _resolver = new(pluginPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name is null) return null;

        // Shared assemblies are loaded from the host so types match across contexts.
        if (sharedAssemblyNames.Contains(assemblyName.Name))
            return Default.LoadFromAssemblyName(new AssemblyName(assemblyName.Name));

        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        return path != null ? LoadFromAssemblyPath(path) : null;
    }
}
```

Note that shared assemblies resolve **by simple name, ignoring version** — a plugin built against version 1.2 of your abstractions gets the host's 1.4. That is what allows old plugins to keep working, and it is why [version compatibility](./06_Compatibility.md) is a discipline rather than an automatic guarantee.

Your list is added to a built-in set, so you only ever name your own packages. The framework always shares:

- `Ivy.Plugin.Abstractions` and `Ivy`
- `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.DependencyInjection.Abstractions`, and `Microsoft.Extensions.Logging.Abstractions` — these appear in the plugin contract surface (`IServiceCollection`, `ILogger`) and must share type identity

So a host that ships a base and an extended abstractions package declares both:

```csharp
server.UsePlugins(pluginsDir, configFactory,
    sharedAssemblyNames: ["Acme.Plugin.Abstractions", "Acme.Plugin.Extended.Abstractions"]);
```

<Callout Type="warning">
Every assembly containing types that cross the host/plugin boundary must be listed. Miss one and the plugin loads a second copy of it — `Configure` then throws about a context type mismatch, or a cast to your interface returns null, with no compile-time warning of any kind.
</Callout>

Assemblies listed here are also skipped when the plugin's own files are loaded, so a plugin that ships a copy of your abstractions DLL does no harm.

## Extending the Context

A plugin receives whatever context you hand it. To offer your own, derive from `PluginContextBase` — which already implements `IIvyExtendedPluginContext` — and return it from `contextFactory`:

```csharp
internal class AcmePluginContext(Server server, WebApplicationBuilder builder, string dataRoot)
    : PluginContextBase(server, builder), IAcmeExtendedPluginContext
{
    public string DataRoot { get; } = dataRoot;
    public AcmeHookRegistry Hooks { get; } = new();

    // ...your own members, implemented against Services and CurrentPluginId
}
```

Wire it up in the same callback that publishes its registries into DI, so the rest of your app can read what plugins contributed:

```csharp
server.UsePlugins(pluginsDir, new AcmePluginConfigFactory(pluginsDir),
    contextFactory: (s, builder) =>
    {
        var pluginContext = new AcmePluginContext(s, builder, dataRoot);
        builder.Services.AddSingleton(pluginContext.Hooks);
        return pluginContext;
    },
    sharedAssemblyNames: ["Acme.Plugin.Abstractions", "Acme.Plugin.Extended.Abstractions"],
    deferPluginLoads: true);
```

Inside `Configure`, `PluginContextBase` tracks which plugin is currently being configured:

- `Services` returns **that plugin's own** service collection, not a shared one.
- `CurrentPluginId` (protected) is the id of that plugin.

Registered services, apps, and endpoints are revoked for you when a plugin unloads. Contributions to registries *you* invented are not, so record `CurrentPluginId` alongside each one:

```csharp
public void AddThing(IThing thing)
{
    _things.Add((PluginId: CurrentPluginId!, Thing: thing));
}
```

Then drop those entries when the plugin goes away, using the `IPluginManager` events below.

## Reacting to Plugin Changes

`IPluginManager` is registered as a singleton and is the host's view of plugin state:

```csharp
public interface IPluginManager
{
    IReadOnlyList<string> GetActivePluginIds();
    PluginManifest? GetPluginManifest(string pluginId);
    PluginConfigurationSchema? GetPluginSchema(string pluginId);
    object? BuildPluginConfigurationView(string pluginId, IIvyPluginConfig config);
    IReadOnlyList<PluginCandidate> GetUnloadedPlugins();
    IReadOnlyList<UnconfiguredPlugin> GetUnconfiguredPlugins();
    bool UnloadPlugin(string pluginId);
    bool LoadPlugin(string pluginPath);
    bool ReloadPlugin(string pluginId);
    bool ReconfigurePlugin(string pluginId);

    event Action<string>? PluginLoaded;
    event Action<string>? PluginLoadFailed;
    event Action<string>? PluginUnloaded;
    event Action<string>? PluginRemoved;
    event Action<string>? PluginReloaded;
    event Action<string>? PluginActivated;
    event Action<string>? PluginDeactivated;
}
```

`GetUnloadedPlugins()` returns `PluginCandidate` records carrying a `FailureReason` and `FailedAt`, which is what you show when a plugin refuses to load. `GetUnconfiguredPlugins()` returns each plugin's schema plus its `ValidationErrors`.

Subscribe to `PluginUnloaded` and `PluginRemoved` to drop that plugin's contributions from your own registries.

## Consuming What Plugins Register

Plugins register services into their own collection, not your app's. Resolve them through `IPluginServiceProvider`, registered as a singleton:

```csharp
public class ThingListView : ViewBase
{
    public override object? Build()
    {
        var plugins = UseService<IPluginServiceProvider>();
        UsePluginState();

        var things = plugins.GetServices<IThing>();

        return Layout.Vertical() | things.Select(t => new Card(content: Text.P(t.Name))).ToArray();
    }
}
```

`GetServices<T>()` aggregates across every active plugin. The `UsePluginState()` [hook](../../../03_Hooks/01_HookIntroduction.md) re-renders the view whenever a plugin is loaded, unloaded, or reconfigured, so the list stays current without a page refresh.

## What You Get for Free

`UsePlugins` registers a **Plugin Manager** app. It lists active, unconfigured, and failed plugins; offers Reload and Unload for each; and renders a settings form generated from each plugin's configuration schema — or the plugin's own custom view when it provides one. It is hidden from the sidebar by default (`isVisible: false`), so link to it from your own settings screen, or build your own management UI on `IPluginManager` and ignore it.

## Development Loop

Three options make iterating on a plugin bearable:

- `enableHotReload: true` (the default) watches the plugins directory and reloads a plugin when its DLLs change.
- `buildSourcePlugins: true` treats any plugin directory containing a `.csproj` as a source plugin and runs `dotnet build` on it when a source file changes. Combined with hot reload, editing a plugin's `.cs` file gets you a rebuilt, reloaded plugin. Requires the .NET SDK on the machine.
- `deferPluginLoads: true` loads plugins in the background after the web application is built, so a slow or failing plugin doesn't hold up startup.

To develop a plugin that lives outside the plugins directory, list its path in `plugin-references.yaml` — see [Distributing Plugins](./05_DistributingPlugins.md).

## See Also

- [Host Abstractions](./03_HostAbstractions.md)
- [Writing Plugins](./04_WritingPlugins.md)
- [Version Compatibility](./06_Compatibility.md)
- [Program](../01_Program.md)
- [Secrets](../14_Secrets.md)
