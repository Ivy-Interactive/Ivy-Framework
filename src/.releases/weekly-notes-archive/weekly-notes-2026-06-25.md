# Ivy Framework Weekly Notes - Week of 2026-06-25

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

This release focuses on a major revamp, hardening, and modernization of the **Ivy Plugin Architecture**. The configuration system has been fully redesigned to be schema-driven and type-safe, compile-time type safety has been improved for plugin contexts, custom configuration wizard hooks have been added, and the plugin loading lifecycle has been hardened against race conditions, resource leaks, and concurrent mutations.

---

## Major Highlights

### Modern Schema-Driven Plugin Configuration
Plugins no longer access raw `IConfiguration` directly from the host. Instead, the host provides a dedicated `IIvyPluginConfig` interface via the new `IIvyPluginConfigFactory`. 
* **Fluent Schema Definition**: Use the new `SchemaBuilder` API to define plugin configuration schemas dynamically. Object-initializer validation is now strictly internal to prevent runtime configuration schema corruption.
* **Typed Accessors**: Default interface methods like `GetInt(key)`, `GetBool(key)`, and `Get<T>(key)` eliminate manual text parsing boilerplate in plugins.
* **Inline Schema UI**: The Plugin Manager UI now automatically renders interactive schema-driven configuration forms inline for active and unconfigured plugins.

#### Example: Fluent Configuration Schema & Config Access
```csharp
using Ivy.Plugins;

public class HelloWorldPlugin : IIvyPlugin
{
    public PluginManifest Manifest => new()
    {
        Id = "ivy.plugin.helloworld",
        Title = "Hello World",
        Icon = PluginIcon.Named("SiLinear")
    };

    // Define schema using the new fluent SchemaBuilder API
    public PluginConfigurationSchema? ConfigurationSchema => new SchemaBuilder()
        .AddString("Greeting", defaultValue: "Hello", description: "Greeting prefix", isRequired: true)
        .AddBoolean("Enthusiastic", defaultValue: true, description: "Whether to append exclamation mark")
        .Build();

    public void Configure(IIvyPluginContext context)
    {
        // Safe typed accessors on IIvyPluginConfig
        string greeting = context.Config.GetValue("Greeting") ?? "Hello";
        bool enthusiastic = context.Config.GetBool("Enthusiastic") ?? true;
        
        context.Logger.LogInformation("Greeting: {Prefix}{Suffix}", greeting, enthusiastic ? "!" : "");
    }
}
```

### Compile-Time Type-Safe Plugins
To avoid error-prone manual casting inside plugin `Configure` methods, developers can now implement the generic `IIvyPlugin<TContext>` interface to enforce the expected plugin context type at compile time.

```csharp
using Ivy.Plugins;

public class CustomExtendedPlugin : IIvyPlugin<IIvyExtendedPluginContext>
{
    public PluginManifest Manifest => new() { Id = "custom.extended", Title = "Extended Plugin" };
    public PluginConfigurationSchema? ConfigurationSchema => null;

    // Receives the specialized context directly without casting
    public void Configure(IIvyExtendedPluginContext context)
    {
        context.RegisterAppActions("ActionName", () => { /* ... */ });
    }
}
```

### Custom Plugin Configuration UIs
In addition to automatically generated schema-driven forms, plugins can now provide custom configuration views (e.g. multi-step wizards using `Stepper`, `Card`, or `Callout` components) by implementing `BuildConfigurationView`.

```csharp
public object? BuildConfigurationView(IIvyPluginConfig config)
{
    return Layout.Vertical(
        Component.Stepper(
            Step.Create("Basic Settings", Layout.Stack(
                Component.TextInput(config, "Greeting", "Default Greeting")
            )),
            Step.Create("Formatting", Layout.Stack(
                Component.BoolInput(config, "Enthusiastic", "Enthusiastic Style")
            ))
        )
    );
}
```

### Graceful Plugin Shutdown Hooks
Plugins can now implement `ShutdownAsync` to perform asynchronous cleanup operations (e.g., terminating open connections or saving local states) before they are unloaded, reconfigured, or when the host process exits.
* **Isolation**: All shutdown calls are bounded by a **5-second timeout**. Exceptions are caught, logged, and isolated to prevent a single faulty plugin from hanging host shutdowns.

```csharp
public async Task ShutdownAsync(PluginShutdownContext context)
{
    context.Logger.LogInformation("Plugin shutting down due to: {Reason}", context.Reason);
    await _myDatabaseConnection.DisposeAsync();
}
```

---

## Bug Fixes and Improvements

### Plugin Watcher & Reload Reliability
* **Directory Leak Prevention**: Tracked and fixed a directory leak under `$TMPDIR/ivy-plugins/` where shadow-copied plugin DLLs were left behind on reload, load failures, or host exits. Shadow folders are now deleted immediately after unloading or via a host `ProcessExit` fallback hook.
* **Thread-Safety Hardening**: Fixed a race condition where collection updates during reload operations on the Plugin Loader thread raced with concurrent reads on the UI rendering thread. Thread-safe locks have been added across `PluginContextBase` collections.
* **Watcher Initialization**: Delayed plugin filesystem watchers from starting until the host application is fully initialized, resolving startup race conditions where early build updates triggered premature reloads.
* **Atomic Save Watchers**: Added `Renamed` file system handlers to correctly capture reload events from IDEs and text editors that save files atomically via write-and-rename operations.
* **Source Build Failures**: Gracefully handle compilation/build errors of local source-plugin targets during reloading, tracking failed builds instead of loading broken or stale assemblies.

### API & Tooling Improvements
* **API Compatibility CI Check**: Integrated `EnablePackageValidation` into package building and CI workflows. Pulled API contract baseline against `v1.2.67` to prevent accidental breaking changes to plugin-facing public APIs on pull requests.
* **Enum Ordinals**: Hardened all plugin enums (`ConfigFieldType`, `PluginIconKind`, etc.) by adding explicit ordinal values to ensure long-term forward binary compatibility when new enums are appended.
* **Framework Cleanup**: Moved `AddBadgeProvider` to the Tendril-specific API context and removed legacy non-functional sidebar transformers (`TransformMenuItems`, `AddFooterMenuItems`) to streamline developer APIs.

---

## Security Enhancements

* **Transitive Vulnerability Upgrades**: Updated `dompurify` and resolved high-severity vulnerability warning audits on local test suites by targeting safe `SQLitePCLRaw.bundle_e_sqlite3` (`v3.0.0`) package structures.
* **Path Traversal Shielding**: Added strict directory path traversal verification inside the plugin loader, neutralizing potential risks when loading local assemblies or serving custom assets under `/ivy/plugins/`.
