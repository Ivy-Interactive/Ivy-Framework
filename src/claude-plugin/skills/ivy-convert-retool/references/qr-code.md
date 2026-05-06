# QR Code

A content area to display a QR code. Encodes URLs, text, or other data as a scannable QR code.

## Retool

```toolscript
qrCode1.value = "https://example.com"
qrCode1.setValue("https://example.com/page")
qrCode1.horizontalAlign = "center"
```

## Ivy

Ivy does not have a dedicated QR Code widget. The closest approach is to use an `Image` widget with an external QR code generation API, or an `Iframe`/`Html` widget with a JavaScript QR library.

```csharp
// Using an external QR code API via Image widget
new Image("https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=https://example.com")
    .Width(Size.Units(50))
    .Height(Size.Units(50));
```

## Parameters

| Parameter                | Documentation                                                       | Ivy                                            |
|--------------------------|---------------------------------------------------------------------|-------------------------------------------------|
| `value`                  | Current QR code content (string, default `null`)                    | Not supported (use external API URL parameter)  |
| `horizontalAlign`        | Horizontal alignment: `left`, `center`, `right` (default `left`)    | Not supported (use layout)                      |
| `heightType`             | Fixed or auto height (default `fixed`)                              | `Image.Height`                                  |
| `hidden`                 | Toggles component visibility (default `false`)                      | `Image.Visible`                                 |
| `margin`                 | Exterior spacing (default `4px 8px`)                                | Not supported (use layout spacing)              |
| `isHiddenOnMobile`       | Hide on mobile layout (default `true`)                              | Not supported                                   |
| `isHiddenOnDesktop`      | Hide on desktop layout (default `false`)                            | Not supported                                   |
| `maintainSpaceWhenHidden`| Reserve space when hidden (default `false`)                         | Not supported                                   |
| `showInEditor`           | Keeps component visible in editor when hidden (default `false`)     | Not supported                                   |
| `style`                  | Custom styling options (default `null`)                             | Not supported                                   |
| `clearValue()`           | Removes current value                                               | Not supported                                   |
| `resetValue()`           | Restores default value                                              | Not supported                                   |
| `setValue(value)`        | Updates QR code content                                             | Not supported                                   |
| `setHidden(hidden)`      | Sets visibility state                                               | `Image.Visible`                                 |
| `scrollIntoView()`       | Scrolls container so component is visible                           | Not supported                                   |
