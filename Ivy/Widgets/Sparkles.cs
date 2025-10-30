using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Decorative sparkles indicator with optional text, color and size.
/// Useful for accenting headings, badges or status labels.
/// </summary>
public record Sparkles : WidgetBase<Sparkles>
{
    /// <summary>Optional text displayed next to the sparkles icon.</summary>
    [Prop] public string? Text { get; set; }

    /// <summary>Optional foreground color token.</summary>
    [Prop] public Colors? Color { get; set; }

    /// <summary>Visual size of the sparkles icon.</summary>
    [Prop] public Sizes Size { get; set; } = Sizes.Medium;
}

/// <summary>
/// Fluent helpers for configuring <see cref="Sparkles"/>.
/// </summary>
public static class SparklesExtensions
{
    /// <summary>Sets the text.</summary>
    public static Sparkles Text(this Sparkles w, string? text) => w with { Text = text };

    /// <summary>Sets the color.</summary>
    public static Sparkles Color(this Sparkles w, Colors color) => w with { Color = color };

    /// <summary>Sets the size.</summary>
    public static Sparkles Size(this Sparkles w, Sizes size) => w with { Size = size };
}


