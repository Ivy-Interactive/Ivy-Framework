using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

public record Label
{
    public Label()
    {
    }

    public double Offset { get; set; } = 5;
    public double? Angle { get; set; } = null;
    public Positions? Position { get; set; } = null;
    public Colors? Color { get; set; } = null;
}

public static class LabelExtensions
{
    public static Label Offset(this Label label, double offset)
    {
        return label with { Offset = offset };
    }

    public static Label Angle(this Label label, double angle)
    {
        return label with { Angle = angle };
    }

    public static Label Position(this Label label, Positions position)
    {
        return label with { Position = position };
    }

    public static Label Color(this Label label, Colors color)
    {
        return label with { Color = color };
    }
}

