# Ivy Framework Weekly Notes - Week of 2026-06-18

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## New Features

### Localization Support in Activity Heatmap
We have added a new `Localize` property to `ActivityHeatmap`. By default (`Localize = false`), day/month labels and hover tooltips render in English to ensure consistent styling. When enabled (`Localize = true`), the widget resolves and formats all date/time and day/month labels according to the client browser's locale.

#### Example Usage
```csharp
using Ivy;
using Ivy.Widgets.ActivityHeatmap;

public class LocalizationDemo : ViewBase
{
    private IQueryable<ActivityLog> _logs;

    public override object Build() =>
        _logs.ToActivityHeatmap(
            dimension: log => log.Timestamp,
            measure: log => log.Count
        )
        .Localize(true) // Enables localized day/month labels and tooltips
        .ValueLabel("Events");
}
```

### Loading Overlay for Xterm Terminal
The Xterm Terminal widget now supports a built-in loading overlay spinner. 
* **Props**: Use `Loading` (boolean) and `LoadingText` (string) props to show an overlay spinner until the first visible output from the PTY stream is rendered. 
* ANSI escape preambles (such as title/cursor setup sequences) are filtered out so that the overlay only disappears when actual interactive content arrives.

### HideScrollbar API in StackLayout
Introduced `HideScrollbar` in `StackLayout` to support clean layouts without scrollbar noise.
* **Capabilities**: Applies the `.invisible-scrollbar` class to the underlying Radix ScrollArea, permitting touch and mousewheel scrolling without rendering a scrollbar track.
* **Builder**: Fluent `HideScrollbar()` method added on `LayoutView` and `StackLayout` builder definitions.

#### Example Usage
```csharp
using Ivy;

public class CleanListApp : ViewBase
{
    public override object Build() =>
        Layout.Vertical(
            Component.Text("Dynamic Stream"),
            Layout.Stack(
                Component.Text("Item 1"),
                Component.Text("Item 2"),
                Component.Text("Item 3")
            )
            .HideScrollbar() // Scrollable layout without scrollbar indicators
        );
}
```

### LinkTarget in LinkBuilder
The `Link` builder factory method now accepts a `LinkTarget` parameter, allowing developers to target either `Self` or `Blank` (defaulting to `Self` for backwards compatibility).

---

## Bug Fixes and Improvements

### High-Severity MessagePack Vulnerability Patched
Directly referenced `MessagePack 2.5.302` across framework packages to resolve a high-severity transitive vulnerability advisory (GHSA-hv8m-jj95-wg3x, audit warning NU1903) coming from older SignalR dependency packages.

### Sheet Layout Breakpoint Preservation
Fixed an issue in `Sheet.Resizable()` where injecting min/max constraints collapsed multi-breakpoint responsive width (`Responsive<Size>`) definitions down to default widths, causing sizing truncation on larger screens. Constraints are now recursively applied to all populated breakpoints.

### Scaffolding Scoped Fields in DetailsBuilder
Explicitly supplying a builder using `Builder()` on fields (such as collection lists) inside `DetailsBuilder` now correctly resets the internal `IsRemoved` flag to `false`. This prevents collections (like `List<Link>`) from being scaffolded out and ignored.

### SignalR appArgs Reconnection Loop
Resolved a WebSocket connection lost reconnect loop when navigating between applications containing dynamic query arguments (`appArgs`). By utilizing a stabilized `stableAppArgs` reference in SignalR URL mappings, root shell socket connections are no longer recycled mid-handshake.

### Table Sizing and Performance
Fixed column rendering artifacts and optimized resize performance in `Table` and `TableBuilder` widgets.

### Nullable Boolean Cycle-Binding
Fixed serialization and cyclic three-state selection behavior in nullable boolean toggle controls (`BoolInput`).

### App ID Folder Collapsing
When organizing applications under hierarchical folder structures where the folder matches the type name (e.g., `Apps/Jobs/JobsApp`), the generated app ID is now cleanly flattened to `"jobs"` instead of `"jobs/jobs"`.

### Xterm Box-Drawing & Alignment
Switched to the Canvas renderer (`@xterm/addon-canvas`) in the Xterm widget to guarantee seamless tiling for box-drawing and block characters (e.g., Claude Code logos), resolving font loading races and last-row clipping.

### External Widget Frontend Build Optimization
Speed up external widget builds by caching steps and neutralizing `CI=true` overrides locally to prevent pnpm no-TTY abort errors (`ERR_PNPM_ABORTED_REMOVE_MODULES_DIR_NO_TTY`).

### Input Design & Styling Consistency
Standardized visuals across all input components (`Select`, `Number`, `TextInput`, `TextArea`, and `Password` variants):
- Removed gray background colors from all default input elements and `SearchInput` affixes.
- Standardized text borders, density gap spacing, and suffix/affix sizes across various input densities.
