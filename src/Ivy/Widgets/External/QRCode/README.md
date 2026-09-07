# Ivy.Widgets.QRCode

A QR code widget for the Ivy Framework, powered by [`qrcode.react`](https://github.com/zpao/qrcode.react).

## Props

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `Value` | `string` | `""` | The data to encode as a QR code |
| `PixelSize` | `int?` | *(client default 256)* | Size of the QR code in pixels |
| `ErrorCorrectionLevel` | `QrErrorCorrectionLevel` | `Low` | Error correction: `Low` (L), `Medium` (M), `Quartile` (Q), `High` (H) |
| `Background` | `Colors?` | `null` | Module background (resolved from the active Ivy theme) |
| `Foreground` | `Colors?` | `null` | Module foreground (resolved from the active Ivy theme) |

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
    .ErrorCorrectionLevel(QrErrorCorrectionLevel.High)
    .Foreground(Colors.Primary)
    .Background(Colors.White)
```
