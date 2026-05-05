# PDF

Embeds and displays PDF documents within an application. Supports loading from a URL, file storage, or file object, with optional toolbar, zoom controls, rotation, and page-snapping behavior.

## Retool

```toolscript
pdf.setFileUrl('https://example.com/file.pdf');
pdf.setHidden(false);
```

## Ivy

Ivy does not have a dedicated PDF viewer widget. The closest equivalent is `Iframe`, which can render a PDF from a URL inside an embedded browsing context.

```csharp
new Iframe("https://example.com/file.pdf")
```

## Parameters

| Parameter                  | Documentation                                                         | Ivy                                                                                   |
|----------------------------|-----------------------------------------------------------------------|---------------------------------------------------------------------------------------|
| `src`                      | Data source URL for the PDF                                           | `Iframe.Src` serves the same purpose                                                  |
| `srcType`                  | Source type (`src`, `retoolStorageFileId`, `retoolFileObject`, etc.)   | Not supported                                                                         |
| `retoolFileObject`         | File data for non-Retool Storage files                                | Not supported (file bytes can be held via `FileInput` but not displayed)               |
| `retoolStorageFileId`      | UUID reference to a Retool Storage file                               | Not supported                                                                         |
| `scaleMode`                | PDF scaling mode (`width` or `fill`)                                  | Not supported                                                                         |
| `scrollSnap`               | Snap scrolling to page boundaries                                     | Not supported                                                                         |
| `showTopBar`               | Shows a toolbar above the PDF                                         | Not supported                                                                         |
| `showZoomControls`         | Displays zoom in/out buttons                                          | Not supported                                                                         |
| `showRotateButton`         | Displays a rotation control                                           | Not supported                                                                         |
| `title`                    | Display title text                                                    | Not supported                                                                         |
| `hidden`                   | Controls component visibility                                         | `Iframe.Visible` (inverted logic)                                                     |
| `isHiddenOnMobile`         | Hides on mobile layout (default `true`)                               | Not supported                                                                         |
| `isHiddenOnDesktop`        | Hides on desktop layout                                               | Not supported                                                                         |
| `maintainSpaceWhenHidden`  | Reserves canvas space when hidden                                     | Not supported                                                                         |
| `margin`                   | External spacing                                                      | Controlled via layout widgets (`Stack`, `Box`, etc.)                                  |
| `style`                    | Custom styling options                                                | Not supported                                                                         |
| `id`                       | Unique identifier / component name                                    | Not applicable (widgets are referenced by variable)                                   |
| `showInEditor`             | Visibility in editor when hidden                                      | Not supported                                                                         |
| `setFileUrl(src)`          | Method to dynamically set the PDF URL                                 | Bind `Iframe.Src` to a reactive state                                                 |
| `setHidden(hidden)`        | Method to toggle visibility                                           | Bind `Iframe.Visible` to a reactive state                                             |
| `scrollIntoView(options)`  | Scrolls container to display component                                | Not supported                                                                         |
