using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A visual separator line.
/// </summary>
public record Separator : WidgetBase<Separator>
{
    public Separator(string? text = null, Orientation orientation = Orientation.Horizontal)
    {
        Text = text;
        Orientation = orientation;
    }

    internal Separator()
    {
    }

    [Prop] public Orientation Orientation { get; set; } = Orientation.Horizontal;

    [Prop] public string? Text { get; set; }

    [Prop] public TextAlignment TextAlign { get; set; } = TextAlignment.Center;
}

public static class SeparatorExtensions
{
    public static Separator Orientation(this Separator separator, Orientation orientation) => separator with { Orientation = orientation };

    public static Separator Text(this Separator separator, string? text) => separator with { Text = text };

    public static Separator TextAlign(this Separator separator, TextAlignment textAlign) => separator with { TextAlign = textAlign };
}