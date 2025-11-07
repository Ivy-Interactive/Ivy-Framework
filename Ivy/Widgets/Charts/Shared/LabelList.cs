using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

/// <summary>
/// Represents a label list configuration for chart elements.
/// </summary>
public record LabelList
{
    public LabelList(string dataKey)
    {
        DataKey = dataKey;
    }

    /// <summary>
    /// Gets or sets the key that identifies the data property to display in the labels.
    /// </summary>
    public string DataKey { get; set; }

    /// <summary>
    /// Gets or sets the offset distance of the labels from their associated chart element in pixels.
    /// Default is 5 pixels.
    /// </summary>
    public double Offset { get; set; } = 5;

    /// <summary>
    /// Gets or sets the rotation angle of the label text in degrees. Positive values rotate clockwise.
    /// Default is null (no rotation).
    /// </summary>
    public double? Angle { get; set; } = null;

    /// <summary>
    /// Gets or sets the position of the labels relative to their associated chart element.
    /// </summary>
    public Positions? Position { get; set; } = Positions.Outside;

    /// <summary>
    /// Gets or sets the fill color of the label text.
    /// </summary>
    public Colors? Fill { get; set; } = Colors.Black;

    /// <summary>
    /// Gets or sets the opacity of the fill color for the label text. Value ranges from 0.0 to 1.0.
    /// </summary>
    public double? FillOpacity { get; set; } = null;

    /// <summary>
    /// Gets or sets the font size of the label text in pixels.
    /// </summary>
    public int? FontSize { get; set; } = null;

    /// <summary>
    /// Gets or sets the font family for the label text.
    /// </summary>
    public string? FontFamily { get; set; } = null;

    /// <summary>
    /// Gets or sets the number format string for the label values.
    /// </summary>
    public string? NumberFormat { get; set; } = null;
}

/// <summary>
/// Extension methods for the LabelList class.
/// </summary>
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