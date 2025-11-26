using Ivy.Core;
using Ivy.Core.Docs;
using Ivy.Shared;

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

public record Badge : WidgetBase<Badge>
{
    public Badge(string? title = null, BadgeVariant variant = BadgeVariant.Primary, Icons icon = Icons.None)
    {
        Title = title;
        Variant = variant;
        Icon = icon;
    }

    [Prop] public string? Title { get; set; }

    [Prop] public BadgeVariant Variant { get; set; }

    [Prop] public Icons? Icon { get; set; }

    [Prop] public Align IconPosition { get; set; } = Align.Left;

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

    [RelatedTo(nameof(Badge.Variant))]
    public static Badge Secondary(this Badge badge)
    {
        return badge with { Variant = BadgeVariant.Secondary };
    }

    [RelatedTo(nameof(Badge.Variant))]
    public static Badge Destructive(this Badge badge)
    {
        return badge with { Variant = BadgeVariant.Destructive };
    }

    [RelatedTo(nameof(Badge.Variant))]
    public static Badge Outline(this Badge badge)
    {
        return badge with { Variant = BadgeVariant.Outline };
    }

    [RelatedTo(nameof(Badge.Variant))]
    public static Badge Primary(this Badge badge)
    {
        return badge with { Variant = BadgeVariant.Primary };
    }

    [RelatedTo(nameof(Badge.Variant))]
    public static Badge Success(this Badge badge)
    {
        return badge with { Variant = BadgeVariant.Success };
    }

    [RelatedTo(nameof(Badge.Variant))]
    public static Badge Warning(this Badge badge)
    {
        return badge with { Variant = BadgeVariant.Warning };
    }

    [RelatedTo(nameof(Badge.Variant))]
    public static Badge Info(this Badge badge)
    {
        return badge with { Variant = BadgeVariant.Info };
    }
}
