# Circular Image

A content area to display a circular image. Circular Image is a preset version of Retool's Image component, preconfigured with a 50% border radius. It is typically used for profile pictures and user avatars.

## Retool

```toolscript
// Display a circular image from a URL
circularImage1.setImageUrl("https://example.com/photo.jpg");

// Hide/show the component
circularImage1.setHidden(false);

// Flip the image horizontally
circularImage1.setFlipHorizontal(true);
```

## Ivy

The closest equivalent in Ivy is the **Avatar** widget. It displays a circular image and falls back to initials when no image is provided.

```csharp
// Display a circular avatar with an image
new Avatar("Niels Bosma", "https://example.com/photo.jpg")

// Avatar without an image falls back to initials
new Avatar("Niels Bosma")

// Multiple avatars in a horizontal layout
Layout.Horizontal()
    | new Avatar("Alice", "https://example.com/alice.jpg")
    | new Avatar("Bob", "https://example.com/bob.jpg")
    | new Avatar("Charlie")
```

## Parameters

| Parameter              | Documentation                                                                 | Ivy                                                    |
|------------------------|-------------------------------------------------------------------------------|--------------------------------------------------------|
| `src`                  | The image source URL.                                                         | `Image` (constructor parameter)                        |
| `altText`              | Accessible description for screen readers.                                    | Not supported                                          |
| `fallback (initials)`  | Not supported (no built-in fallback).                                         | `Fallback` (constructor parameter, generates initials) |
| `fit`                  | Whether the image is cropped (`cover`) or scaled (`contain`).                 | `Scale` property                                       |
| `aspectRatio`          | The width-to-height ratio (e.g., `1.3` for 4:3).                             | Not supported                                          |
| `flipHorizontal`       | Whether the image is flipped horizontally.                                    | Not supported                                          |
| `flipVertical`         | Whether the image is flipped vertically.                                      | Not supported                                          |
| `hidden`               | Whether the component is hidden from view.                                    | `Visible` property                                     |
| `horizontalAlign`      | Horizontal alignment of the image (`left`, `center`, `right`).               | Not supported (use layout containers)                  |
| `heightType`           | Whether height auto-adjusts or is fixed.                                      | `Height` property                                      |
| `margin`               | Amount of margin rendered outside the component.                              | Not supported (use layout containers)                  |
| `style`                | Custom style options.                                                         | Not supported                                          |
| `srcType`              | Source type (`src`, `retoolStorageFileId`, `retoolFileObject`, `dbBlobId`).   | Not supported (URL/data URI/local file only)           |
| `width`                | The width of the image.                                                       | `Width` property                                       |
| `height`               | The height of the image.                                                      | `Height` property                                      |
| `clickable`            | Whether there is a click event handler.                                       | Not supported                                          |
| `events (Click)`       | Event triggered when the image is clicked.                                    | Not supported                                          |
| `isHiddenOnMobile`     | Whether hidden in mobile layout.                                              | Not supported                                          |
| `isHiddenOnDesktop`    | Whether hidden in desktop layout.                                             | Not supported                                          |
| `maintainSpaceWhenHidden` | Whether to reserve space when hidden.                                      | Not supported                                          |
