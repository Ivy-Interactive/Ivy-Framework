// ReSharper disable once CheckNamespace
namespace Ivy.Charts;

public record Toolbox
{
    public enum Orientations
    {
        Horizontal,
        Vertical
    }

    public enum Alignments
    {
        Left,
        Center,
        Right
    }

    public enum VerticalAlignments
    {
        Top,
        Middle,
        Bottom
    }

    public Toolbox()
    {

    }

    public Orientations Orientation { get; set; } = Orientations.Horizontal;

    public Alignments Align { get; set; } = Alignments.Right;

    public VerticalAlignments VerticalAlign { get; set; } = VerticalAlignments.Top;

    public bool SaveAsImage { get; set; } = true;

    public bool Restore { get; set; } = true;

    public bool DataView { get; set; } = true;

    public bool MagicType { get; set; } = true;
}

public static class ToolboxExtensions
{

    public static Toolbox Orientation(this Toolbox toolbox, Toolbox.Orientations orientation)
    {
        return toolbox with { Orientation = orientation };
    }

    public static Toolbox Horizontal(this Toolbox toolbox)
    {
        return toolbox with { Orientation = Toolbox.Orientations.Horizontal };
    }

    public static Toolbox Vertical(this Toolbox toolbox)
    {
        return toolbox with { Orientation = Toolbox.Orientations.Vertical };
    }

    public static Toolbox Align(this Toolbox toolbox, Toolbox.Alignments align)
    {
        return toolbox with { Align = align };
    }

    public static Toolbox Left(this Toolbox toolbox)
    {
        return toolbox with { Align = Toolbox.Alignments.Left };
    }

    public static Toolbox Center(this Toolbox toolbox)
    {
        return toolbox with { Align = Toolbox.Alignments.Center };
    }

    public static Toolbox Right(this Toolbox toolbox)
    {
        return toolbox with { Align = Toolbox.Alignments.Right };
    }

    public static Toolbox VerticalAlign(this Toolbox toolbox, Toolbox.VerticalAlignments verticalAlign)
    {
        return toolbox with { VerticalAlign = verticalAlign };
    }

    public static Toolbox Top(this Toolbox toolbox)
    {
        return toolbox with { VerticalAlign = Toolbox.VerticalAlignments.Top };
    }

    public static Toolbox Middle(this Toolbox toolbox)
    {
        return toolbox with { VerticalAlign = Toolbox.VerticalAlignments.Middle };
    }

    public static Toolbox Bottom(this Toolbox toolbox)
    {
        return toolbox with { VerticalAlign = Toolbox.VerticalAlignments.Bottom };
    }

    public static Toolbox SaveAsImage(this Toolbox toolbox, bool enabled = true)
    {
        return toolbox with { SaveAsImage = enabled };
    }

    public static Toolbox Restore(this Toolbox toolbox, bool enabled = true)
    {
        return toolbox with { Restore = enabled };
    }

    public static Toolbox DataView(this Toolbox toolbox, bool enabled = true)
    {
        return toolbox with { DataView = enabled };
    }

    public static Toolbox MagicType(this Toolbox toolbox, bool enabled = true)
    {
        return toolbox with { MagicType = enabled };
    }
}
