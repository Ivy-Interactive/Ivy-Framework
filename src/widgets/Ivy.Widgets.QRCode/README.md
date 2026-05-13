# Ivy.Widgets.QRCode

A QR code widget for the Ivy Framework, powered by [`qrcode.react`](https://github.com/zpao/qrcode.react).

## Props

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `Value` | `string` | `""` | The data to encode as a QR code |
| `PixelSize` | `int?` | `256` | Size of the QR code in pixels |
| `Level` | `string` | `"L"` | Error correction level: `"L"`, `"M"`, `"Q"`, or `"H"` |
| `IncludeMargin` | `bool` | `true` | Whether to include a quiet zone margin |
| `BgColor` | `string?` | `null` | Background color (CSS color string) |
| `FgColor` | `string?` | `null` | Foreground color (CSS color string) |

## Usage

```csharp
using Ivy;
using Ivy.Widgets.QRCode;

// Basic usage
new QRCode().Value("https://ivy-interactive.com")

// With options
new QRCode()
    .Value("https://ivy-interactive.com")
    .PixelSize(300)
    .Level("H")
    .FgColor("#1a1a2e")
    .BgColor("#ffffff")
```
