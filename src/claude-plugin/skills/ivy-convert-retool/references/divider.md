# Divider

Display a dividing line with an optional label to organize content sections and improve readability.

## Retool

```toolscript
divider1.text = "Section Title"
divider1.horizontalAlign = "center"
divider1.setHidden(false)
```

## Ivy

```csharp
new Separator()
new Separator("Section Title")
new Separator("Section Title", Orientation.Vertical)
```

## Parameters

| Parameter                | Documentation                                          | Ivy                              |
|--------------------------|--------------------------------------------------------|----------------------------------|
| `text`                   | Optional label displayed on the divider                | `Text`                           |
| `horizontalAlign`        | Label alignment: `left`, `center`, `right`             | Not supported                    |
| `hidden`                 | Controls component visibility                          | `Visible` (read-only)            |
| `id`                     | Unique identifier for the component                    | Not supported                    |
| `isHiddenOnDesktop`      | Toggle visibility on desktop layouts                   | Not supported                    |
| `isHiddenOnMobile`       | Toggle visibility on mobile layouts                    | Not supported                    |
| `maintainSpaceWhenHidden`| Reserves space when the component is hidden            | Not supported                    |
| `margin`                 | External spacing (e.g. `4px 8px`)                      | Not supported                    |
| `showInEditor`           | Remains visible in editor when hidden                  | Not supported                    |
| `style`                  | Custom styling options                                 | Not supported                    |
| N/A                      | N/A                                                    | `Orientation` (Horizontal/Vertical) |
