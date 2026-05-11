# Alert (Legacy)

A message box to display important information with optional title, description, and clickable action button. Supports info, success, error, and warning styles.

## Retool

```toolscript
// Alert (Legacy) component configured via the Inspector:
// - Title: "System Update"
// - Description: "A new version is available."
// - Alert Type: "info" | "success" | "error" | "warning"
// - Button Text: "Update Now"
// - Event: Click → runs a query or script

// Programmatic scroll
legacyAlert1.scrollIntoView({ behavior: "smooth", block: "nearest" });
```

## Ivy

```csharp
// Callout widget — closest equivalent to Retool's Alert (Legacy)
new Callout("A new version is available.", "System Update", CalloutVariant.Info)

// Static factory methods for each variant
Callout.Info("Informational message")
Callout.Success("Operation completed successfully")
Callout.Warning("Proceed with caution")
Callout.Error("Something went wrong")

// With title, icon, and rich content
new Callout(title: "System Update", variant: CalloutVariant.Warning, icon: Icons.AlertTriangle)
{
    new Text("A new version is available. Please update at your earliest convenience."),
    new Button("Update Now", OnClick: () => { /* handle action */ })
}
```

## Parameters

| Parameter              | Retool Documentation                                                              | Ivy                                                                 |
|------------------------|-----------------------------------------------------------------------------------|---------------------------------------------------------------------|
| `title`                | Title text to display.                                                            | `Title` property or constructor param                               |
| `description`          | Descriptive text to display to the user.                                          | `description` constructor param (supports rich content as children)  |
| `alertType`            | Alert type: `info`, `success`, `error`, `warning`. Default `info`.                | `Variant` — `CalloutVariant.Info/Success/Warning/Error`             |
| `buttonText`           | Text label for an optional action button.                                         | Not built-in; compose with a `Button` child widget                  |
| `buttonType`           | Type of button (`action`).                                                        | Not supported                                                       |
| `action`               | Event handler that runs when the action button is clicked.                        | Not supported; use `Button.OnClick` inside the callout              |
| `events`               | Configured event handlers (supports `Click`).                                     | Not supported; use child widget events instead                      |
| `hidden`               | Whether the component is hidden. Default `false`.                                 | `Visible` (inverted logic — `true` means shown)                    |
| `id`                   | Unique identifier. Default `legacyAlert1`.                                        | Not applicable; widgets are referenced by variable name             |
| `margin`               | Margin: `4px 8px` (normal) or `0` (none). Default `4px 8px`.                     | Not supported directly; use layout spacing                          |
| `style`                | Custom style options object.                                                      | Not supported; styling via `Variant`, `Scale`, `Height`, `Width`    |
| `isHiddenOnDesktop`    | Hide on desktop layout. Default `false`.                                          | Not supported                                                       |
| `isHiddenOnMobile`     | Hide on mobile layout. Default `true`.                                            | Not supported                                                       |
| `maintainSpaceWhenHidden` | Keep space when hidden. Default `false`.                                       | Not supported                                                       |
| `showInEditor`         | Show in editor when hidden. Default `false`.                                      | Not supported                                                       |
| `scrollIntoView()`     | Method to scroll the component into view with behavior and block options.         | Not supported                                                       |
| —                      | —                                                                                 | `Icon` — custom icon override (`Icons?`)                            |
| —                      | —                                                                                 | `Scale` — size adjustment                                           |
| —                      | —                                                                                 | `Height` / `Width` — dimension properties                           |
