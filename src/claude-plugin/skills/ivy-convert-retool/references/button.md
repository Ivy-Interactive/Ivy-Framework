# Button

A button to trigger actions when clicked. Supports customizable text, icons, loading states, variants, and event handlers.

## Retool

```toolscript
button1.text = "Submit"
button1.disabled = false
button1.loading = false
button1.styleVariant = "solid"
button1.iconBefore = "bold/interface-arrows-right"
button1.iconAfter = null
button1.hidden = false
button1.horizontalAlign = "center"
button1.allowWrap = true

// Event handler
button1.events = [{ type: "click", action: "triggerQuery", query: "submitForm" }]

// Methods
button1.setDisabled(true)
button1.setHidden(false)
```

## Ivy

```csharp
new Button("Submit", onClick: e => client.Toast("Submitted!"))
    .Icon(Icons.ArrowRight, Align.Left)
    .Loading(false)
    .Disabled(false)

// Variants
new Button("Solid Style")                         // Primary (default, similar to solid)
new Button("Outline Style").Outline()              // Similar to styleVariant: "outline"
new Button("Ghost Style").Ghost()

// Icon-only button
new Button().Icon(Icons.Settings).Ghost()

// Link/navigation button
new Button("Visit Docs")
    .Url("https://example.com")
    .OpenInNewTab()
    .Icon(Icons.ExternalLink, Align.Right)
```

## Parameters

| Parameter              | Retool Documentation                                          | Ivy                                                        |
|------------------------|---------------------------------------------------------------|------------------------------------------------------------|
| `text` / `Title`       | Display text content. Default: `"Button"`                     | `Title` (string, set via constructor or property)          |
| `disabled`             | Disables interaction. Default: `false`                        | `Disabled` (bool) or `.Disabled()` method                  |
| `loading`              | Shows loading indicator. Default: `false`                     | `Loading` (bool) or `.Loading()` method                    |
| `hidden` / `Visible`   | Hides component from view. Default: `false`                   | `Visible` (bool, inverted logic)                           |
| `styleVariant`         | Visual style: `solid` or `outline`                            | `Variant` (ButtonVariant enum: Primary, Secondary, Outline, Ghost, Link, Destructive) |
| `iconBefore`           | Prefix icon. Default: `null`                                  | `.Icon(Icons icon, Align.Left)`                            |
| `iconAfter`            | Suffix icon. Default: `null`                                  | `.Icon(Icons icon, Align.Right)` via `IconPosition`        |
| `events` (Click)       | Triggered when button is clicked                              | `OnClick` (constructor parameter: `Action` or `Func<Event<Button>, ValueTask>`) |
| `horizontalAlign`      | Alignment: `left`, `center`, `right`, `stretch`               | Not supported (handled via layout containers)              |
| `allowWrap`            | Permits content to wrap across multiple lines. Default: `true`| Not supported                                              |
| `ariaLabel`            | Screen reader accessible label. Default: `null`               | Not supported                                              |
| `heightType`           | Height behavior: `fixed` or `auto`. Default: `fixed`          | `Height` (Size)                                            |
| `margin`               | External spacing. Default: `4px 8px`                          | Not supported (handled via layout containers)              |
| `loaderPosition`       | Loader placement: `auto`, `replace`, `left`, `right`          | Not supported                                              |
| `submit`               | Submits form when clicked. Default: `false`                   | Not supported                                              |
| `submitTargetId`       | Form to submit. Default: `null`                               | Not supported                                              |
| `isHiddenOnDesktop`    | Desktop layout visibility. Default: `false`                   | Not supported                                              |
| `isHiddenOnMobile`     | Mobile layout visibility. Default: `true`                     | Not supported                                              |
| `maintainSpaceWhenHidden` | Preserves space when hidden. Default: `false`              | Not supported                                              |
| `showInEditor`         | Visible in editor when hidden. Default: `false`               | Not applicable                                             |
| `id`                   | Unique identifier. Default: `button1`                         | Not applicable (C# object reference)                       |
| `style`                | Custom style options                                          | `.Foreground()`, `.BorderRadius()`, `.Scale()` and variant methods |
| N/A                    | N/A                                                           | `Url` (string) - navigation URL                            |
| N/A                    | N/A                                                           | `Target` (LinkTarget) / `.OpenInNewTab()`                  |
| N/A                    | N/A                                                           | `Tooltip` (string) - hover tooltip text                    |
| N/A                    | N/A                                                           | `Width` (Size)                                             |
| N/A                    | N/A                                                           | `Scale` (Scale?) - size scaling                            |
| N/A                    | N/A                                                           | `Foreground` (Colors?) - text/icon color                   |
| N/A                    | N/A                                                           | `BorderRadius` (None, Rounded, Full)                       |

### Methods

| Method                     | Retool                           | Ivy                              |
|----------------------------|----------------------------------|----------------------------------|
| Toggle disabled             | `button.setDisabled(disabled)`  | Set `Disabled` property          |
| Toggle visibility           | `button.setHidden(hidden)`      | Set `Visible` property           |
