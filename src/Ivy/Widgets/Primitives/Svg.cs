using System.Runtime.CompilerServices;
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Renders SVG content.
/// </summary>
public record Svg : WidgetBase<Svg>
{
    public Svg(string content)
    {
        Content = content;
    }

    internal Svg()
    {
        Width = Size.Auto();
        Height = Size.Auto();
    }

    [Prop] public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Event fired when a link within the SVG is clicked.
    /// The event data contains the href URL of the clicked link.
    /// </summary>
    [Event] public EventHandler<Event<Svg, string>>? OnLinkClick { get; set; }
}

public static class SvgExtensions
{
    [OverloadResolutionPriority(1)]
    public static Svg OnLinkClick(this Svg svg, Func<Event<Svg, string>, ValueTask> onLinkClick)
    {
        return svg with { OnLinkClick = new(onLinkClick) };
    }

    // Overload for Action<Event<Svg, string>>
    public static Svg OnLinkClick(this Svg svg, Action<Event<Svg, string>> onLinkClick)
    {
        return svg with { OnLinkClick = new(onLinkClick.ToValueTask()) };
    }

    public static Svg OnLinkClick(this Svg svg, Action<string> onLinkClick)
    {
        return svg with { OnLinkClick = new(@event => { onLinkClick(@event.Value); return ValueTask.CompletedTask; }) };
    }
}