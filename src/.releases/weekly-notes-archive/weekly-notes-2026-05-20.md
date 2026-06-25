# Ivy Framework Weekly Notes - Week of 2026-05-20

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## Plugin System

### Simplified Plugin Interface & Configuration States

The `IIvyPlugin` interface has been streamlined — `ConfigureServices` has been removed. Plugins now only need to implement `Manifest`, `ConfigurationSchema`, and `Configure`:

```csharp
public class MyPlugin : IIvyPlugin
{
    public PluginManifest Manifest => new("my-plugin", "My Plugin", "1.0.0");
    public PluginConfigurationSchema? ConfigurationSchema => null;

    public void Configure(IIvyPluginContext context)
    {
        // Register your services here
    }
}
```

The plugin manager now supports configuration states. Plugins can be **Active** or **Unconfigured**, and the new `IPluginManager` API reflects this:

- `GetActivePluginIds()` (renamed from `GetLoadedPluginIds`)
- `GetUnconfiguredPlugins()` — returns plugins that need configuration before activation
- `ReconfigurePlugin(pluginId)` — trigger reconfiguration of a plugin
- New events: `PluginActivated` and `PluginDeactivated`

---

## New Features

### New UI Widgets (ActivityHeatmap, AgentOutputView, QRCode, AnimatedStatusLabel)

We have introduced four new premium widgets:
- **`ActivityHeatmap`**: A contribution-graph style daily activity grid. Supports custom `ColorScheme` tokens, customizable start/end date boundaries, and responsive localization.
- **`AgentOutputView`**: Designed specifically for displaying AI agent execution steps. Groups sequential tool calls, features stream animations, handles inner body/descendant auto-scroll pinning, and adapts to framework-specific CSS variables.
- **`QRCode`**: A QR code generator widget fully integrated with framework APIs and backed by automated unit tests.
- **`AnimatedStatusLabel`**: A reusable status label cell renderer with shimmer, timer, and badge modes for dynamic cell status updates.

### DropDownMenu StayOpen Support

Added the `.StayOpen()` modifier to the `DropDownMenu` widget. When enabled, selecting an item in the menu will not automatically close the dropdown list. This is particularly useful for menus containing checkbox items, multi-select values, or submenus where users make multiple selections at once.

```csharp
new DropDownMenu()
    .StayOpen()
    .Items(new[]
    {
        new MenuItem("Enable Feature A").Checked(true),
        new MenuItem("Enable Feature B").Checked(false),
        new MenuItem("Enable Feature C").Checked(false)
    });
```

### Markdown Article Grade Typography

Introduced an opt-in `Article` typography mode for the `Markdown` widget via the `Markdown.Article()` builder method. This allows standalone markdown content to automatically render with the same premium typography styling (such as relaxed paragraph line-heights, heading top margins, and elegant H2 dividers) as the full `Ivy.Article` widget, without needing its surrounding table of contents or page footer chrome.

```csharp
new Markdown("# Getting Started\nThis is a paragraph of text...")
    .Article();
```

### MultipleSelector Customization (rightSlot)

The `MultipleSelector` component (used for select-many fields) now supports a `rightSlot` property to place custom action buttons or indicators (e.g. clear button, custom dropdown indicators, loaders, or validation feedback icon) aligned to the right inside the selector input field. 

We also refactored `SelectMultiVariant` to move the loading spinner, invalid icon, and "Clear All" (`X`) button inside this new `rightSlot`, maintaining a consistent visual layout across all inputs.

---

## Improvements & Bug Fixes

### DataTable empty states and filters

- **Interactive Empty Tables:** Previously, when a filter matched zero rows, the table component would collapse and replace the whole container with the empty view (which also removed the headers and filter input, preventing the user from clearing their search). Now, the full table (header, border, filters) remains visible, and the empty view is displayed as a centered overlay above the empty grid body.
- **Single-Line Filter Editor Newline Fix:** Fixed an issue in CodeMirror single-line filter inputs where pressing Enter to submit the filter would insert a trailing space. Enter is now correctly intercepted and consumed by the editor.
- **Empty Rows & Sizing:** Fixed height calculation for filtered tables. The grid maintains its border height and headers, displaying "No rows to display" when empty instead of collapsing down to 0 height.
- **Grid Cell Enhancements:** Column headers now show a dedicated style when filtering is active, cells truncate overflown text with ellipses (`...`), and we added a column resize handle icon on active headers for mobile and touch devices.
- **Double-Tap & Long-Press Support:** Enhanced cell click handling in `DataTable` to support double-tap interactions, and refactored links in `DataTableEditor` to use double-tap/long-press for touch devices.

### Charts & Visualization

- **Tooltip Axis Formatters:** Fixed Chart tooltip formatting. Tooltips now format their headers using the active axis tick formatter instead of the raw data value (e.g., currency `C0` or percentage `P0` formatters).

### SelectInput & Dropdown Enhancements

- **Dropdown Search Input:** Added a `header` prop to `SelectContent` for search input integration, and ensured the search input retains focus during dropdown interactions.
- **Sizing Alignment:** Aligned text input with select input by height.
- **Scroll Button Visibility:** Ensured scroll button state updates correctly in `SelectSingleVariant` component by programmatically dispatching scroll events on Radix viewport changes.

### Input Widgets & Window Sizing

- **Affix Button Alignment:** Consistently integrated `affixEmbeddedButtonClasses` and `affixIconOnlyCellPaddingClasses` into `DateRangeInputWidget` and `DateTimeInputWidget` to align buttons with prefix/suffix elements correctly.
- **Desktop Window Bounds:** Added a default minimum window size constraint to the `DesktopWindow` container to prevent applications from being resized down to an unusable size.
- **Dialogs & Swatches:** Adjusted dialog overlays to stack from top-to-bottom for natural visual hierarchy, and fixed the popover picker to automatically close when selecting a color swatch.

---

## Builds & Internal Maintenance

- **Scriban Dependency Upgrade:** Upgraded `Scriban` template engine to `7.2.0` in Ivy build tools to resolve security vulnerabilities (NU1903).
- **React 19 Build Restoration:** Fixed Vite frontend configuration to resolve TypeScript compilation errors, React 19 merge compilation issues, and conditional `useState` hooks.
- **Frontend Diagnostics:** Integrated `react-doctor.config.json` to focus on React correctness and performance optimizations.
- **Test Relocation:** Moved `ActivityHeatmap` and `QRCode` tests into separate widget-local test projects and excluded test files from the main widget compilation step to speed up production builds.
- **CI / Build Pipeline:** Added checkout and build automation for the `Ivy-Examples` repository during automated release package builds.
