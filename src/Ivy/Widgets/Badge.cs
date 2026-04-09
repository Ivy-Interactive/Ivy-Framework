using Ivy.Core;
using Ivy.Core.Docs;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum BadgeVariant
{
    Primary,
    Destructive,
    Outline,
    Secondary,
    Success,
    Warning,
    Info
}

/// <summary>
/// A small visual indicator for status, counts, or labeling.
/// </summary>
public record Badge : WidgetBase<Badge>
{
    public Badge(string? title = null, BadgeVariant variant = BadgeVariant.Primary, Icons? icon = null, Colors? color = null)
    {
        Title = title;
        Variant = variant;
        Icon = icon;
        Color = color;
    }

    internal Badge() { }

    [Prop] public string? Title { get; set; }

    [Prop] public BadgeVariant Variant { get; set; } = BadgeVariant.Primary;

    [Prop] public Icons? Icon { get; set; }

    [Prop] public Colors? Color { get; set; }

    [Prop] public Align IconPosition { get; set; } = Align.Left;

    [Event] public EventHandler<Event<Badge>>? OnClick { get; set; }

    public static Badge operator |(Badge badge, object child)
    {
        throw new NotSupportedException("Badge does not support children.");
    }
}

public static class BadgeExtensions
{
    public static Badge Icon(this Badge badge, Icons? icon, Align position = Align.Left)
    {
        return badge with { Icon = icon, IconPosition = position };
    }

    public static Badge Variant(this Badge badge, BadgeVariant variant)
    {
        return badge with { Variant = variant };
    }

    [RelatedTo(nameof(Badge.Color))]
    public static Badge Color(this Badge badge, Colors color)
    {
        return badge with { Color = color };
    }

    [RelatedTo(nameof(Badge.Color))]
    public static Badge Secondary(this Badge badge) => badge with { Color = Colors.Secondary };

    [RelatedTo(nameof(Badge.Color))]
    public static Badge Destructive(this Badge badge) => badge with { Color = Colors.Destructive };

    [RelatedTo(nameof(Badge.Color))]
    public static Badge Success(this Badge badge) => badge with { Color = Colors.Success };

    [RelatedTo(nameof(Badge.Color))]
    public static Badge Warning(this Badge badge) => badge with { Color = Colors.Warning };

    [RelatedTo(nameof(Badge.Color))]
    public static Badge Info(this Badge badge) => badge with { Color = Colors.Info };

    [RelatedTo(nameof(Badge.Color))]
    public static Badge Muted(this Badge badge) => badge with { Color = Colors.Muted };

    [RelatedTo(nameof(Badge.Color))]
    public static Badge Primary(this Badge badge) => badge with { Color = Colors.Primary };

    [RelatedTo(nameof(Badge.Variant))]
    public static Badge Outline(this Badge badge)
    {
        return badge with { Variant = BadgeVariant.Outline };
    }

    public static Badge OnClick(this Badge badge, Func<Event<Badge>, ValueTask> onClick) => badge with { OnClick = new(onClick) };

    public static Badge OnClick(this Badge badge, Action<Event<Badge>> onClick) => badge with { OnClick = new(onClick.ToValueTask()) };

    public static Badge OnClick(this Badge badge, Action onClick) => badge with { OnClick = new(_ => { onClick(); return ValueTask.CompletedTask; }) };

    public static Badge OnClick(this Badge badge, Func<ValueTask> onClick) => badge with { OnClick = new(_ => onClick()) };
}
