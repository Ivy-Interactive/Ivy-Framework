using Ivy.Core;
using Ivy.Core.Docs;
using Ivy.Shared;

namespace Ivy;

public enum BadgeVariant
{
    Primary,
    Destructive,
    Outline,
    Secondary
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

    [Prop] public Sizes Size { get; set; } = Sizes.Medium;

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

    public static Badge Size(this Badge badge, Sizes size)
    {
        return badge with { Size = size };
    }

    [RelatedTo(nameof(Badge.Size))]
    public static Badge Large(this Badge badge)
    {
        return badge.Size(Sizes.Large);
    }

    [RelatedTo(nameof(Badge.Size))]
    public static Badge Small(this Badge badge)
    {
        return badge.Size(Sizes.Small);
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
}
