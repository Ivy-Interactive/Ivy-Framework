# Ivy Framework Weekly Notes - Week of 2026-06-25 (v2)

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

This release introduces major enhancements to Tooltips and Keyboard Shortcuts, adds the server-side responsive viewport branching hook `UseBreakpoint`, and refines helper methods.

---

## New Features

### Interactive, Semantic & Controllable Tooltips
We have significantly enhanced the Tooltip widget:
* **Visual Anchors**: Use the new `.Bubble()` builder method to render a pointing arrow indicator targeting the trigger element.
* **Dismissible Persistent Tooltips**: Introduce `.Persist()` to show a manual close (X) button, keeping the tooltip pinned open until dismissed (disabling automatic hover/click-away dismissals).
* **Controlled Open States**: Wire the `Open` prop and `OnOpen`/`OnClose`/`OnOpenChange` event handlers to open or close tooltips dynamically from application logic (e.g. server-driven validation popups).
* **Semantic Variants**: Set background, text, and arrow colors using predefined theme-aware tokens (`Default`, `Info`, `Success`, `Warning`, `Error`) via the `.Variant(...)` API.

#### Example Usage
```csharp
using Ivy;

public class TooltipDemo : ViewBase
{
    private bool _showError = false;

    public override object Build() =>
        Layout.Vertical(
            Component.Button("Toggle Error")
                .OnClick(() => _showError = !_showError),
            
            Component.Tooltip("A serious system error occurred.")
                .Bubble()
                .Persist()
                .Variant(TooltipVariant.Error)
                .Open(_showError)
                .OnClose(() => _showError = false)
        );
}
```

### Keyboard Shortcuts (Kbd) API Enhancements
Keyboard shortcuts (Kbd) now look cleaner and support rich key combinations:
* **Standalone Key Caps**: Combinations like "Ctrl+Enter" now display as separate styled blocks (e.g., `[Ctrl]` `[↵]`) rather than a single combined capsule, making shortcut indicators much more compact.
* **Platform-Appropriate Icons**: Render navigation and modifier key names (e.g. Shift, Enter, Backspace) as platform-appropriate symbols (with macOS glyphs `⌃`, `⌘`, `⇧`, `⌥`).
* **Ghost Variant**: Added `.Ghost()` (and `<ShortcutKeys ghost />`) to render keycaps without backgrounds and borders.
* **Unified Component rendering**: Integrated shortcut rendering across buttons, input boxes, content selectors, and dropdowns under a shared `ShortcutKeys` React component.

### Responsive viewport branching (`UseBreakpoint` hook)
* **Server-Side Breakpoints**: Added `UseBreakpoint()` hook returning `IState<Breakpoint>` and a listener layout widget to roundtrip active browser media query breakpoints back to server state. This permits dynamic C# layout branching (e.g., mobile bottom Sheet vs desktop Dialog views).

#### Example Usage
```csharp
using Ivy;

public class ResponsiveApp : ViewBase
{
    public override object Build()
    {
        var (breakpoint, listener) = Hook.UseBreakpoint();

        return Layout.Vertical(
            listener, // Renders the listener in the layout
            breakpoint.Value == Breakpoint.Mobile
                ? Component.Sheet("Mobile View")
                : Component.Dialog("Desktop View")
        );
    }
}
```

---

## Bug Fixes and Improvements

### Collection Emptiness in IsEmptyContent
* **IEnumerable Emptiness Check**: Hardened `IsEmptyContent` to treat empty collections (`IEnumerable`) as empty content, allowing `Details.RemoveEmpty` to correctly hide empty list/array rows.

### Single-File Publish Warning
* **IL3000 location warning**: Fixed a runtime location warning in `PluginLoader` by targeting `Assembly.GetName().Name` instead of `Assembly.Location`.
