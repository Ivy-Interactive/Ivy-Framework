namespace Ivy.Widgets.QRCode;

[ExternalWidget("frontend/dist/Ivy_Widgets_QRCode.js", ExportName = "QRCode")]
public record QRCode : WidgetBase<QRCode>
{
    [Prop] public string Value { get; init; } = "";
    [Prop] public int? PixelSize { get; init; }
    [Prop] public QrErrorCorrectionLevel ErrorCorrectionLevel { get; init; } = QrErrorCorrectionLevel.Low;
    [Prop] public Colors? Background { get; init; }
    [Prop] public Colors? Foreground { get; init; }
}

public static class QRCodeExtensions
{
    public static QRCode Value(this QRCode w, string value) =>
        w with { Value = value };

    public static QRCode PixelSize(this QRCode w, int size) =>
        w with { PixelSize = size };

    public static QRCode ErrorCorrectionLevel(this QRCode w, QrErrorCorrectionLevel level) =>
        w with { ErrorCorrectionLevel = level };

    public static QRCode Background(this QRCode w, Colors color) =>
        w with { Background = color };

    public static QRCode Foreground(this QRCode w, Colors color) =>
        w with { Foreground = color };
}

public enum QrErrorCorrectionLevel
{
    Low,
    Medium,
    Quartile,
    High
}