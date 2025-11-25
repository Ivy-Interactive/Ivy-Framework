using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Separator : WidgetBase<Separator>
{
    public Separator(string? text = null, Orientation orientation = Orientation.Horizontal)
    {
        Text = text;
        Orientation = orientation;
    }

    [Prop] public Orientation Orientation { get; set; }

    [Prop] public string? Text { get; set; }
}

public static class SeparatorExtensions
{
    public static Separator Orientation(this Separator separator, Orientation orientation) => separator with { Orientation = orientation };

    public static Separator Text(this Separator separator, string? text) => separator with { Text = text };
}