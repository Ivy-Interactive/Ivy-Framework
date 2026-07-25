# Ivy Framework Weekly Notes - Week of 2026-07-18

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

This release brings major enhancements to the plugin system with scoped HTTP endpoint routing (`UseEndpoints`), deferred background plugin loading, lifecycle event hooks (`PluginLoadFailed`, `PluginRemoved`), full-height `DataTable` layout fixes, and keyboard shortcut event handling fixes.

## New Features

### Plugins
- **Plugin HTTP Endpoints (`UseEndpoints`)**: Plugins can now register scoped, hot-reloadable HTTP routes under `/ivy/plugins/{slug}/` via `context.UseEndpoints()`.

  ```csharp
  public void Configure(IIvyExtendedPluginContext context)
  {
      context.UseEndpoints(endpoints =>
      {
          endpoints.MapGet("/status", () => Results.Ok(new { Status = "Healthy" }));
      });
  }
  ```

- **Deferred Background Loading**: Pass `deferPluginLoads: true` to `UsePlugins` to load source and binary plugins asynchronously after the server starts up, improving cold startup times.

  ```csharp
  app.UsePlugins(deferPluginLoads: true);
  ```

- **Lifecycle Events**: Added `PluginLoadFailed` and `PluginRemoved` events to `IPluginManager` to notify UI observers whenever a plugin fails to load or has its directory removed.
- **Pure `.csproj` Source Plugins**: Supported source plugins containing only a `.csproj` file with package references (no `.cs` files required).

## Bug Fixes & Improvements

### Layout & UI
- **DataTable Container Sizing**: Fixed full-height `DataTable` widgets collapsing when placed inside flex or scrolling parent containers.
- **Keyboard Shortcuts in Hidden Tabs**: Resolved issue where global shortcut handlers fired inside hidden tab panels.
- **Backspace Shortcut Listener Cleanup**: Fixed listener cleanup logic so unregistering all keyboard shortcuts does not accidentally strip the global backspace listener.
