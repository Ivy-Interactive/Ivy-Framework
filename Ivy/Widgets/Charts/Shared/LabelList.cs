using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

public record LabelList
{
    public LabelList(string dataKey)
    {
        DataKey = dataKey;
    }

    public string DataKey { get; set; }
    public double Offset { get; set; } = 5;
    public double? Angle { get; set; } = null;
    public Positions? Position { get; set; } = Positions.Outside;
    public Colors? Fill { get; set; } = Colors.Black;
    public double? FillOpacity { get; set; } = null;
    public int? FontSize { get; set; } = null;
    public string? FontFamily { get; set; } = null;
    public string? NumberFormat { get; set; } = null;
}

public static class LabelListExtensions
{
    public static LabelList Offset(this LabelList label, double offset)
    {
        return label with { Offset = offset };
    }

    public static LabelList Angle(this LabelList label, double angle)
    {
        return label with { Angle = angle };
    }

    public static LabelList Position(this LabelList label, Positions position)
    {
        return label with { Position = position };
    }

    public static LabelList Fill(this LabelList label, Colors color)
    {
        return label with { Fill = color };
    }

    public static LabelList FillOpacity(this LabelList label, double fillOpacity)
    {
        return label with { FillOpacity = fillOpacity };
    }

    public static LabelList FontSize(this LabelList label, int fontSize)
    {
        return label with { FontSize = fontSize };
    }

    public static LabelList FontFamily(this LabelList label, string fontFamily)
    {
        return label with { FontFamily = fontFamily };
    }

    public static LabelList NumberFormat(this LabelList label, string format)
    {
        return label with { NumberFormat = format };
    }
}