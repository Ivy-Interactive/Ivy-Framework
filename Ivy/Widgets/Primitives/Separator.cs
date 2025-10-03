using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Visual separator line (horizontal/vertical) with optional text label.</summary>
public record Separator : WidgetBase<Separator>
{
    /// <summary>Initializes separator.</summary>
    /// <param name="text">Optional text label. Default: null.</param>
    /// <param name="orientation">Orientation. Default: Horizontal.</param>
    public Separator(string? text = null, Orientation orientation = Orientation.Horizontal)
    {
        Text = text;
        Orientation = orientation;
    }

    /// <summary>Separator orientation (horizontal or vertical).</summary>
    [Prop] public Orientation Orientation { get; set; }

    /// <summary>Optional text centered on separator line.</summary>
    [Prop] public string? Text { get; set; }
}

/// <summary>Extension methods for Separator widget configuration.</summary>
public static class SeparatorExtensions
{
    /// <summary>Sets separator orientation.</summary>
    public static Separator Orientation(this Separator separator, Orientation orientation) => separator with { Orientation = orientation };

    /// <summary>Sets separator text.</summary>
    public static Separator Text(this Separator separator, string? text) => separator with { Text = text };
}