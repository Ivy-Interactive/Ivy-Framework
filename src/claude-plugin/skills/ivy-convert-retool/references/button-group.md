# Button Group

A group of buttons displayed together to trigger actions when clicked. Supports overflow handling, wrapping, and per-button click events.

## Retool

```toolscript
// Button Group with click event handler
buttonGroup1.events = [
  {
    event: "click",
    type: "script",
    method: "handleButtonClick"
  }
];

// Configure overflow after 4 buttons
buttonGroup1.overflowPosition = 4;
buttonGroup1.overflowMode = "scroll"; // or "wrap"

// Programmatic visibility control
buttonGroup1.setHidden(false);
buttonGroup1.scrollIntoView({ behavior: "smooth", block: "center" });
```

## Ivy

Ivy has no dedicated `ButtonGroup` widget. The equivalent is composing multiple `Button` widgets inside a `Layout.Horizontal()`:

```csharp
Layout.Horizontal().Gap(4)
    | new Button("Copy", onClick: _ => client.Toast("Copied!"))
    | new Button("Paste", onClick: _ => client.Toast("Pasted!"))
    | new Button("Cut", onClick: _ => client.Toast("Cut!")).Destructive()
    | new Button("Settings").Ghost().Icon(Icons.Settings)
```

For action menus with overflow-like behavior, `DropDownMenu` can serve a similar purpose:

```csharp
new DropDownMenu(@evt => client.Toast("Selected: " + @evt.Value),
    new Button("Actions"),
    MenuItem.Default("Copy").Tag("copy"),
    MenuItem.Default("Paste").Tag("paste"),
    MenuItem.Default("Cut").Tag("cut"),
    MenuItem.Separator(),
    MenuItem.Default("Settings").Tag("settings"))
```

## Parameters

| Parameter              | Documentation                                              | Ivy                                                          |
|------------------------|------------------------------------------------------------|--------------------------------------------------------------|
| `id`                   | Unique identifier for the component                        | Variable name                                                |
| `events`               | List of event handlers triggered on click                  | `onClick` parameter on each `Button`                         |
| `alignment`            | Text alignment: `left`, `center`, `right`                  | `Layout.Horizontal().Center()` / `.Left()` / `.Right()`      |
| `overflowMode`         | Overflow behavior: `scroll` or `wrap`                      | `Layout.Horizontal()` (no scroll) / `Layout.Wrap()` for wrap |
| `overflowPosition`     | Index after which items go into overflow menu               | Not supported (use `DropDownMenu` for overflow pattern)      |
| `overlayMaxHeight`     | Max height of overflow options list (px)                   | Not supported                                                |
| `overlayMinWidth`      | Min width of overflow options list (px)                    | Not supported                                                |
| `hidden`               | Whether the component is hidden                            | `.Visible` property on layout or buttons                     |
| `heightType`           | Fixed or auto height                                       | `Size` via `.Height()`                                       |
| `margin`               | External spacing (`4px 8px` or `0`)                        | `.Gap()` on layout                                           |
| `style`                | Custom styling object                                      | Variant methods: `.Primary()`, `.Ghost()`, `.Outline()`, etc.|
| `isHiddenOnDesktop`    | Hide on desktop layout                                     | Not supported                                                |
| `isHiddenOnMobile`     | Hide on mobile layout                                      | Not supported                                                |
| `maintainSpaceWhenHidden` | Reserve space when hidden                               | Not supported                                                |
| `showInEditor`         | Show in editor when hidden                                 | Not supported                                                |
| `scrollIntoView()`     | Scroll container to show the component                     | Not supported                                                |
| `setHidden()`          | Toggle component visibility programmatically               | `.Visible` property                                          |
