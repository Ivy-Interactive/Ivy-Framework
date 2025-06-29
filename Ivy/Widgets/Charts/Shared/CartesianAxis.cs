// ReSharper disable once CheckNamespace

using Ivy.Shared;

namespace Ivy.Charts;

public record CartesianAxis
{
    public double? X { get; set; } = null;
    public double? Y { get; set; } = null;
    public double? Width { get; set; } = null;
    public double? Height { get; set; } = null;
    public bool Horizontal { get; set; } = true;
    public bool Vertical { get; set; } = true;
    public Colors? Fill { get; set; } = null;
    public double? FillOpacity { get; set; } = null;
    public string? StrokeDashArray { get; set; }
}

public static class CartesianAxisExtensions
{
    public static CartesianAxis X(this CartesianAxis cartesianGrid, double x)
    {
        return cartesianGrid with { X = x };
    }

    public static CartesianAxis Y(this CartesianAxis cartesianGrid, double y)
    {
        return cartesianGrid with { Y = y };
    }

    public static CartesianAxis Width(this CartesianAxis cartesianGrid, double width)
    {
        return cartesianGrid with { Width = width };
    }

    public static CartesianAxis Height(this CartesianAxis cartesianGrid, double height)
    {
        return cartesianGrid with { Height = height };
    }

    public static CartesianAxis Horizontal(this CartesianAxis cartesianGrid, bool horizontal = true)
    {
        return cartesianGrid with { Horizontal = horizontal };
    }

    public static CartesianAxis Vertical(this CartesianAxis cartesianGrid, bool vertical = true)
    {
        return cartesianGrid with { Vertical = vertical };
    }

    public static CartesianAxis Fill(this CartesianAxis cartesianGrid, Colors fill)
    {
        return cartesianGrid with { Fill = fill };
    }

    public static CartesianAxis FillOpacity(this CartesianAxis cartesianGrid, double fillOpacity)
    {
        return cartesianGrid with { FillOpacity = fillOpacity };
    }

    public static CartesianAxis StrokeDashArray(this CartesianAxis cartesianGrid, string strokeDashArray)
    {
        return cartesianGrid with { StrokeDashArray = strokeDashArray };
    }
}