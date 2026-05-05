# Image Grid

A content area to display images in a grid layout. Retool provides this as a single dedicated component, while Ivy achieves the same result by composing a `GridLayout` (or `WrapLayout`) with `Image` widgets.

## Retool

```toolscript
imageGrid1.srcByIndex = [
  "https://example.com/photo1.jpg",
  "https://example.com/photo2.jpg",
  "https://example.com/photo3.jpg",
  "https://example.com/photo4.jpg"
];
imageGrid1.captionByIndex = ["Photo 1", "Photo 2", "Photo 3", "Photo 4"];
imageGrid1.columnCount = 3;
imageGrid1.aspectRatio = 1.33;
imageGrid1.clickable = true;
```

## Ivy

```csharp
Layout.Grid()
    .Columns(3)
    .Gap(4)
    | new Image("https://example.com/photo1.jpg")
    | new Image("https://example.com/photo2.jpg")
    | new Image("https://example.com/photo3.jpg")
    | new Image("https://example.com/photo4.jpg")
```

## Parameters

| Parameter | Retool Documentation | Ivy |
|---|---|---|
| `srcByIndex` | Array of image URLs indexed by position | Individual `new Image(src)` widgets composed inside a layout |
| `captionByIndex` | Text descriptions paired with each image by position | Not supported (use a `Text` widget below each image manually) |
| `clickable` | Indicates if click event handler is active on all images | Not supported |
| `clickableByIndex` | Per-image clickability toggle | Not supported |
| `columnCount` | Number of columns to render (fixed mode) | `GridLayout.Columns(int)` |
| `columnMinWidth` | Minimum column width for responsive layouts | Not supported (use `WrapLayout` for automatic wrapping) |
| `columnType` | Layout mode: `"fixed"` or `"responsive"` | `GridLayout` for fixed, `WrapLayout` for responsive |
| `aspectRatio` | Width-to-height ratio for images (e.g. 1.33 for 4:3) | Set `Image.Width` and `Image.Height` manually |
| `tooltipByIndex` | Hover text for individual images | Not supported |
| `tooltipText` | General helper text (supports Markdown) | Not supported |
| `hiddenByIndex` | Visibility state for individual images | `Image.Visible` per widget |
| `margin` | External spacing around the component | `WrapLayout(margin:)` or layout padding |
| `isHiddenOnDesktop` | Desktop visibility toggle | Not supported |
| `isHiddenOnMobile` | Mobile visibility toggle | Not supported |
| `maintainSpaceWhenHidden` | Reserve canvas space when hidden | Not supported |
| `showInEditor` | Visibility during editing mode | Not supported |
| `style` | Custom styling configuration object | Not supported |
| `setHidden(hidden)` | Method to control component visibility | `GridLayout.Visible` / `WrapLayout.Visible` |
| Click event | Triggered when a user clicks an image | Not supported |
