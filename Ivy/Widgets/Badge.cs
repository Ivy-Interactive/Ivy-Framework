using Ivy.Core;
using Ivy.Core.Docs;
using Ivy.Shared;

namespace Ivy;

public enum BadgeVariant
{
    Default,
    Destructive,
    Outline,
    Secondary
}

public record Badge : WidgetBase<Badge>
{
    public Badge(string? title = null, Action<Event<Button>>? onClick = null, BadgeVariant variant = BadgeVariant.Default, Icons icon = Icons.None)
    {
        Title = title;
        Variant = variant;
        Icon = icon;
        OnClick = onClick;
    }

    [Prop] public string? Title { get; set; }

    [Prop] public BadgeVariant Variant { get; set; }

    [Prop] public Icons? Icon { get; set; }

    [Prop] public bool Disabled { get; set; }

    [Event] public Action<Event<Button>>? OnClick { get; set; }

    public static Badge operator |(Badge badge, object child)
    {
        throw new NotSupportedException("Badge does not support children.");
    }
}

public static class BadgeExtensions
{
    public static Badge Disabled(this Badge button, bool disabled = true)
    {
        return button with { Disabled = disabled };
    }

    public static Badge Icon(this Badge button, Icons icon)
    {
        return button with { Icon = icon };
    }

    public static Badge HandleClick(this Badge button, Action<Event<Button>> onClick)
    {
        return button with { OnClick = onClick };
    }

    public static Badge Variant(this Badge button, BadgeVariant variant)
    {
        return button with { Variant = variant };
    }

    [RelatedTo(nameof(Badge.Variant))]
    public static Badge Secondary(this Badge button)
    {
        return button with { Variant = BadgeVariant.Secondary };
    }

    [RelatedTo(nameof(Badge.Variant))]
    public static Badge Destructive(this Badge button)
    {
        return button with { Variant = BadgeVariant.Destructive };
    }

    [RelatedTo(nameof(Badge.Variant))]
    public static Badge Outline(this Badge button)
    {
        return button with { Variant = BadgeVariant.Outline };
    }

    [RelatedTo(nameof(Badge.Variant))]
    public static Badge Default(this Badge button)
    {
        return button with { Variant = BadgeVariant.Default };
    }
}
