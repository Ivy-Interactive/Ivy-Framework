# Icon

Displays a vector icon within a content area. Used for visual communication, decoration, and interactive icon buttons.

## Retool

```toolscript
icon.setValue("/icon:bold/shopping-gift");
icon.setHidden(false);
```

## Ivy

```csharp
new Icon(Icons.ShoppingCart, Colors.Green)
new Icon(Icons.Gift).Large()
Icons.Settings.ToIcon().Color(Colors.Blue).WithAnimation(AnimationType.Pulse)
```

## Parameters

| Parameter              | Documentation                                              | Ivy                                    |
|------------------------|------------------------------------------------------------|-----------------------------------------|
| `icon`                 | The icon key to display (e.g. `/icon:bold/shopping-gift`)  | `Icons` enum (Lucide Icons)             |
| `altText`              | Accessible description for screen readers                  | Not supported                           |
| `clickable`            | Whether a Click event handler is enabled                   | Not supported (use `Button` with icon)  |
| `disabled`             | Disables interaction and triggering                        | Not supported                           |
| `hidden`               | Toggles visibility of the component                        | `Visible` property                      |
| `horizontalAlign`      | Alignment: left, center, or right                          | Not supported (use layout containers)   |
| `id`                   | Unique identifier for the component                        | Not supported (implicit)                |
| `isHiddenOnDesktop`    | Controls visibility in desktop layout                      | Not supported                           |
| `isHiddenOnMobile`     | Controls visibility in mobile layout                       | Not supported                           |
| `loading`              | Displays a loading indicator                               | Not supported                           |
| `maintainSpaceWhenHidden` | Reserves space when component is hidden                 | Not supported                           |
| `margin`               | External spacing around component (`4px 8px` default)      | Not supported (use layout containers)   |
| `showInEditor`         | Remains visible in editor when hidden                      | Not supported                           |
| `style`                | Custom styling options                                     | `.Color()`, `.Scale()`                  |
| `styleVariant`         | Style option: solid or outline                             | Not supported                           |
| `events` (Click)       | Triggered when the icon is clicked                         | Not supported (use `Button` with icon)  |
| `setHidden()`          | Method to toggle visibility                                | `Visible` property                      |
| N/A                    | N/A                                                        | `.Small()` / `.Large()` size variants   |
| N/A                    | N/A                                                        | `.WithAnimation()` (Pulse, Rotate, etc) |
