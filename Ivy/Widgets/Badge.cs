using Ivy.Core;
using Ivy.Core.Docs;
using Ivy.Shared;

namespace Ivy;

/// <summary>Visual style variants for badge widgets controlling appearance, color scheme, and emphasis.</summary>
public enum BadgeVariant
{
    /// <summary>Primary badge style with prominent emphasis for main actions or important information.</summary>
    Primary,
    /// <summary>Destructive badge style for critical actions or negative status indicators.</summary>
    Destructive,
    /// <summary>Outline badge style with borders for secondary actions or subtle status indicators.</summary>
    Outline,
    /// <summary>Secondary badge style with reduced emphasis for supporting information.</summary>
    Secondary,
    /// <summary>Success badge style for successful operations or positive status indicators.</summary>
    Success,
    /// <summary>Warning badge style for alerts or important notices requiring attention.</summary>
    Warning,
    /// <summary>Info badge style for general information or help indicators.</summary>
    Info
}

/// <summary>Badge widget displaying small pieces of information like counts, statuses, or labels with multiple variants, sizes, and icon integration.</summary>
public record Badge : WidgetBase<Badge>
{
    /// <summary>Initializes a Badge with specified title, variant, and icon.</summary>
    /// <param name="title">Text content to display. When null, creates icon-only badge.</param>
    /// <param name="variant">Visual style variant. Default is <see cref="BadgeVariant.Primary"/>.</param>
    /// <param name="icon">Icon to display. Default is <see cref="Icons.None"/>.</param>
    public Badge(string? title = null, BadgeVariant variant = BadgeVariant.Primary, Icons icon = Icons.None)
    {
        Title = title;
        Variant = variant;
        Icon = icon;
    }

    /// <summary>Text content displayed in the badge. When null, creates icon-only badge. Default is null.</summary>
    [Prop] public string? Title { get; set; }

    /// <summary>Visual style variant controlling appearance and emphasis. Default is <see cref="BadgeVariant.Primary"/>.</summary>
    [Prop] public BadgeVariant Variant { get; set; }

    /// <summary>Icon displayed in the badge. Can be positioned on either side of text. Default is <see cref="Icons.None"/>.</summary>
    [Prop] public Icons? Icon { get; set; }

    /// <summary>Size of the badge controlling dimensions and visual prominence. Default is <see cref="Sizes.Medium"/>.</summary>
    [Prop] public Sizes Size { get; set; } = Sizes.Medium;

    /// <summary>Position of icon relative to title text. Default is <see cref="Align.Left"/>.</summary>
    [Prop] public Align IconPosition { get; set; } = Align.Left;

    /// <summary>Prevents adding children to Badge using pipe operator.</summary>
    /// <param name="badge">The Badge widget.</param>
    /// <param name="child">The child content to add (not supported).</param>
    /// <returns>Always throws NotSupportedException.</returns>
    /// <exception cref="NotSupportedException">Badge does not support children.</exception>
    public static Badge operator |(Badge badge, object child)
    {
        throw new NotSupportedException("Badge does not support children.");
    }
}

/// <summary>Extension methods for Badge widget providing fluent API for configuring appearance and behavior.</summary>
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
