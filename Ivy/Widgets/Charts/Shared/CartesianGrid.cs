// ReSharper disable once CheckNamespace

using Ivy.Shared;

namespace Ivy.Charts;

public record CartesianGrid
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

public static class CartesianGridExtensions
{
    public static CartesianGrid X(this CartesianGrid cartesianGrid, double x)
    {
        return cartesianGrid with { X = x };
    }

    public static CartesianGrid Y(this CartesianGrid cartesianGrid, double y)
    {
        return cartesianGrid with { Y = y };
    }

    public static CartesianGrid Width(this CartesianGrid cartesianGrid, double width)
    {
        return cartesianGrid with { Width = width };
    }

    public static CartesianGrid Height(this CartesianGrid cartesianGrid, double height)
    {
        return cartesianGrid with { Height = height };
    }

    public static CartesianGrid Horizontal(this CartesianGrid cartesianGrid)
    {
        return cartesianGrid with { Horizontal = true, Vertical = false };
    }

    public static CartesianGrid Vertical(this CartesianGrid cartesianGrid)
    {
        return cartesianGrid with { Vertical = true, Horizontal = false };
    }

    public static CartesianGrid Fill(this CartesianGrid cartesianGrid, Colors fill)
    {
        return cartesianGrid with { Fill = fill };
    }

    public static CartesianGrid FillOpacity(this CartesianGrid cartesianGrid, double fillOpacity)
    {
        return cartesianGrid with { FillOpacity = fillOpacity };
    }

    public static CartesianGrid StrokeDashArray(this CartesianGrid cartesianGrid, string strokeDashArray)
    {
        return cartesianGrid with { StrokeDashArray = strokeDashArray };
    }
}