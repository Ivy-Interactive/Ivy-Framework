namespace Ivy.Widgets.QRCode;

[ExternalWidget("frontend/dist/Ivy_Widgets_QRCode.js", ExportName = "QRCode")]
public record QRCode : WidgetBase<QRCode>
{
    [Prop] public string Value { get; init; } = "";
    [Prop] public int? PixelSize { get; init; }
    [Prop] public string Level { get; init; } = "L";
    [Prop] public bool IncludeMargin { get; init; } = true;
    [Prop] public string? BgColor { get; init; }
    [Prop] public string? FgColor { get; init; }
}

public static class QRCodeExtensions
{
    public static QRCode Value(this QRCode w, string value) =>
        w with { Value = value };

    public static QRCode PixelSize(this QRCode w, int size) =>
        w with { PixelSize = size };

    public static QRCode Level(this QRCode w, string level) =>
        w with { Level = level };

    public static QRCode IncludeMargin(this QRCode w, bool includeMargin = true) =>
        w with { IncludeMargin = includeMargin };

    public static QRCode BgColor(this QRCode w, string bgColor) =>
        w with { BgColor = bgColor };

    public static QRCode FgColor(this QRCode w, string fgColor) =>
        w with { FgColor = fgColor };
}
