// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

public record Legend
{
    public enum Layouts { Horizontal, Vertical }
    public enum Alignments { Left, Center, Right }
    public enum VerticalAlignments { Top, Middle, Bottom }
    public enum IconTypes { Line, PlainLine, Square, Rect, Circle, Cross, Diamond, Star, Triangle, Wye }

    public Legend()
    {

    }

    public Layouts Layout { get; set; } = Layouts.Horizontal;
    public Alignments Align { get; set; } = Alignments.Center;
    public VerticalAlignments VerticalAlign { get; set; } = VerticalAlignments.Bottom;
    public int IconSize { get; set; } = 14;
    public IconTypes? IconType { get; set; } = null;
}

public static class LegendExtensions
{
    public static Legend Layout(this Legend legend, Legend.Layouts layout)
    {
        return legend with { Layout = layout };
    }

    public static Legend Horizontal(this Legend legend)
    {
        return legend with { Layout = Legend.Layouts.Horizontal };
    }

    public static Legend Vertical(this Legend legend)
    {
        return legend with { Layout = Legend.Layouts.Vertical };
    }

    public static Legend Align(this Legend legend, Legend.Alignments align)
    {
        return legend with { Align = align };
    }

    public static Legend Left(this Legend legend)
    {
        return legend with { Align = Legend.Alignments.Left };
    }

    public static Legend Center(this Legend legend)
    {
        return legend with { Align = Legend.Alignments.Center };
    }

    public static Legend Right(this Legend legend)
    {
        return legend with { Align = Legend.Alignments.Right };
    }

    public static Legend VerticalAlign(this Legend legend, Legend.VerticalAlignments verticalAlign)
    {
        return legend with { VerticalAlign = verticalAlign };
    }

    public static Legend Top(this Legend legend)
    {
        return legend with { VerticalAlign = Legend.VerticalAlignments.Top };
    }

    public static Legend Middle(this Legend legend)
    {
        return legend with { VerticalAlign = Legend.VerticalAlignments.Middle };
    }

    public static Legend Bottom(this Legend legend)
    {
        return legend with { VerticalAlign = Legend.VerticalAlignments.Bottom };
    }

    public static Legend IconSize(this Legend legend, int iconSize)
    {
        return legend with { IconSize = iconSize };
    }

    public static Legend IconType(this Legend legend, Legend.IconTypes iconType)
    {
        return legend with { IconType = iconType };
    }
}