# Avatar

A content area to display user information and profile image. Displays an image if available or falls back to initials or a placeholder when no image is provided.

## Retool

```toolscript
// Avatar with image source
avatar1.src = "https://example.com/user-photo.jpg"
avatar1.fallback = "JD"
avatar1.imageSize = 48
avatar1.icon = "/icon:bold/shopping-gift"
avatar1.labelPosition = "left"
avatar1.tooltipText = "John Doe"
avatar1.horizontalAlign = "center"
```

## Ivy

```csharp
Layout.Horizontal()
    | new Avatar("Niels Bosma", "https://api.images.cat/150/150?1")
    | new Avatar("Niels Bosma")
```

## Parameters

| Parameter              | Documentation                                                                 | Ivy                   |
|------------------------|-------------------------------------------------------------------------------|-----------------------|
| src                    | The image URL data source (string)                                            | `image` constructor parameter |
| fallback               | Text to display if there is no image or icon (string)                         | `fallback` constructor parameter |
| imageSize              | Size of the avatar: 16, 24, 32, 48, or 64 px                                 | `.Height()` / `.Width()` via `Size` |
| icon                   | Icon to display (icon key string)                                             | Not supported         |
| horizontalAlign        | Horizontal alignment: left, center, right                                     | Not supported (use Layout) |
| labelPosition          | Label position relative to the avatar: top or left                            | Not supported         |
| tooltipText            | Tooltip text on hover (markdown string)                                       | Not supported         |
| clickable              | Whether click event handler is enabled (boolean)                              | Not supported (wrap in Button) |
| isHiddenOnDesktop      | Hide component on desktop layout (boolean)                                    | Not supported         |
| isHiddenOnMobile       | Hide component on mobile layout (boolean)                                     | Not supported         |
| maintainSpaceWhenHidden| Keep space when hidden (boolean)                                              | Not supported         |
| showInEditor           | Show in editor even if hidden (boolean)                                       | Not supported         |
| margin                 | Margin around the component: "4px 8px" or "0"                                 | Not supported         |
| style                  | Custom style options (object)                                                 | Not supported         |
| id                     | Unique identifier/name (string)                                               | Not supported         |
| hidden                 | Whether the component is visible (via `setHidden()`)                          | `Visible` property    |
| scrollIntoView()       | Scroll canvas so avatar is visible (method)                                   | Not supported         |
| setHidden()            | Toggle component visibility (method)                                          | `Visible` property    |
