# Ivy Namespace Flattening

## Summary

~20 sub-namespaces have been collapsed into the root `namespace Ivy;`. This simplifies the API surface and eliminates the need for most `using Ivy.*` directives in connection projects.

Normal Ivy apps only need to `using Ivy;`.

## Breaking Changes

### Removed Namespaces

The following namespaces no longer exist. All types they contained are now in `namespace Ivy;`:

- `Ivy.Apps`
- `Ivy.Auth`
- `Ivy.Chrome`
- `Ivy.Client`
- `Ivy.Helpers`, `Ivy.Helpers.Tui`
- `Ivy.Hooks`
- `Ivy.Shared`
- `Ivy.Services`
- `Ivy.Views`, `Ivy.Views.Alerts`, `Ivy.Views.Blades`, `Ivy.Views.Builders`, `Ivy.Views.Charts`, `Ivy.Views.Dashboards`, `Ivy.Views.Forms`, `Ivy.Views.Tables`, `Ivy.Views.DataTables`, `Ivy.Views.Kanban`
- `Ivy.Charts`
- `Ivy.Connections`
- `Ivy.Widgets.Inputs`

### Internal types moved to `Ivy.Core.*`

Types that are not part of the public API have been moved from `namespace Ivy;` into `Ivy.Core.*` sub-namespaces (with the goal of eventually marking them `internal`):

| Old Location | New Namespace | Types |
|---|---|---|
| `Ivy` (Apps/) | `Ivy.Core.Apps` | `AppSession`, `IAppRepository`, `ScopedAppRepository`, `AppRouter`, `AppRouteResult`, `AppRepository`, `IAppRepositoryNode`, `IAppRepositoryGroup`, `AppRepositoryGroup`, `AppHelpers`, `AppDescriptor`, `AppIds` |
| `Ivy` (Chrome/) | `Ivy.Core.Chrome` | `ChromeUtils` |
| `Ivy.Middleware` | `Ivy.Core.Server.Middleware` | `PathToAppIdMiddleware`, `PathToAppIdMiddlewareExtensions`, `RoutingConstantData` |
| `Ivy` (root) | `Ivy.Core.Server` | `AppHub`, `ClientSender`, `ClientProvider`, `AppSessionStore`, `HotReloadService`, `ServerDescription`, `AppDescription`, `ConnectionDescription`, `ServiceDescription`, `ServerUtils` |


### Unchanged Namespaces

These namespaces remain as-is:

- `Ivy.Core.*`
- `Ivy.Themes`
- `Ivy.Widgets.Internal`

### Type Renames

| Old Name | New Name | Reason |
|---|---|---|
| `Ivy.Charts.Tooltip` (chart config) | `ChartTooltip` | Collision with `Tooltip` (UI widget) |
| `Ivy.Charts.TooltipExtensions` | `ChartTooltipExtensions` | Follows `ChartTooltip` rename |

### Extension Classes Made Partial

These extension classes existed in both `Ivy.Views.*` and `Ivy.Widgets.*` namespaces. Now that they share `namespace Ivy;`, they have been merged via `partial class`:

- `AreaChartExtensions`
- `BarChartExtensions`
- `LineChartExtensions`
- `PieChartExtensions`
- `FormExtensions`
- `TableExtensions`

## Migration Guide

### 1. Remove `using` directives for flattened namespaces

Delete any of these from your code (they are no longer needed):

```csharp
// Remove all of these:
using Ivy.Apps;
using Ivy.Auth;
using Ivy.Chrome;
using Ivy.Client;
using Ivy.Helpers;
using Ivy.Helpers.Tui;
using Ivy.Hooks;
using Ivy.Shared;
using Ivy.Services;
using Ivy.Views;
using Ivy.Views.Alerts;
using Ivy.Views.Blades;
using Ivy.Views.Builders;
using Ivy.Views.Charts;
using Ivy.Views.Dashboards;
using Ivy.Views.Forms;
using Ivy.Views.Tables;
using Ivy.Views.DataTables;
using Ivy.Views.Kanban;
using Ivy.Charts;
```

### 2. Remove `GlobalUsings.cs` entries

If your connection project has a `GlobalUsings.cs` importing these namespaces, those entries can be removed.

### 3. Update `Charts.Tooltip` references

```csharp
// Before:
Charts.Tooltip tooltip = new Charts.Tooltip();

// After:
ChartTooltip tooltip = new ChartTooltip();
```

### 4. Update `Ivy.Middleware` references

```csharp
// Before:
using Ivy.Middleware;

// After:
using Ivy.Core.Server.Middleware;
```

### 5. Update fully-qualified namespace references

If your code used fully-qualified names like `Ivy.Shared.Size`, `Ivy.Apps.AppContext`, etc., update them:

```csharp
// Before:
Ivy.Shared.Size.Full()
Ivy.Apps.AppContext

// After:
Ivy.Size.Full()
Ivy.AppContext
```
