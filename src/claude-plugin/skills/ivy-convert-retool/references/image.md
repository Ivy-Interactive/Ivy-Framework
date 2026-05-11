# Image

A content area to display an image. Supports URLs, file objects, base64 data, and storage references with options for sizing, alignment, and interaction.

## Retool

```toolscript
// Display an image from a URL
image1.setImageUrl("https://example.com/photo.jpg");

// Hide/show the image
image1.setHidden(false);

// Flip the image
image1.setFlipHorizontal(true);

// Scroll the image into view
image1.scrollIntoView({ behavior: "smooth", block: "center" });
```

## Ivy

```csharp
new Image("https://example.com/photo.jpg")
```

## Parameters

| Parameter           | Documentation                                                                 | Ivy                           |
|---------------------|-------------------------------------------------------------------------------|-------------------------------|
| `src`               | The image source URL or data URI.                                             | `Src`                         |
| `altText`           | Accessible description for screen readers.                                    | Not supported                 |
| `aspectRatio`       | Width-to-height ratio (e.g., `1.3` for 4:3).                                 | Not supported                 |
| `fit`               | Whether image is cropped (`cover`) or scaled (`contain`) when height is fixed.| `Scale`                       |
| `heightType`        | Fixed height or auto-expand to fit content.                                   | `Height`                      |
| `srcWidth`          | The width of the image.                                                       | `Width`                       |
| `srcHeight`         | The height of the image.                                                      | `Height`                      |
| `flipHorizontal`    | Whether the image is flipped horizontally.                                    | Not supported                 |
| `flipVertical`      | Whether the image is flipped vertically.                                      | Not supported                 |
| `horizontalAlign`   | Horizontal alignment (`left`, `center`, `right`).                             | Not supported                 |
| `hidden`            | Whether the component is hidden from view.                                    | `Visible`                     |
| `clickable`         | Whether a click event handler is enabled.                                     | Not supported                 |
| `margin`            | Margin rendered outside the component.                                        | Not supported                 |
| `style`             | Custom style options.                                                         | Not supported                 |
| `srcType`           | Source type (`src`, `retoolStorageFileId`, `retoolFileObject`, `dbBlobId`).    | Not supported                 |
| `retoolStorageFileId`| Unique identifier for a file in Retool Storage.                              | Not supported                 |
| `retoolFileObject`  | File data when not using Retool Storage.                                      | Not supported                 |
| `isHiddenOnMobile`  | Whether hidden in mobile layout.                                              | Not supported                 |
| `isHiddenOnDesktop` | Whether hidden in desktop layout.                                             | Not supported                 |
| `maintainSpaceWhenHidden` | Whether to reserve space when hidden.                                   | Not supported                 |
