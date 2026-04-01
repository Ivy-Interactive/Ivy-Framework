using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A simple container for grouping content.
/// </summary>
public record Box : WidgetBase<Box>
{
    public Box(params IEnumerable<object> content) : base(content.ToArray())
    {
    }

    internal Box()
    {
    }

    [Prop] public Colors? Background { get; set; } = null;

    [Prop] public Colors? BorderColor { get; set; } = null;

    [Prop] public BorderRadius BorderRadius { get; set; } = BorderRadius.Rounded;

    [Prop] public BorderStyle BorderStyle { get; set; } = BorderStyle.Solid;

    [Prop] public Thickness BorderThickness { get; set; } = new(1);

    [Prop] public Thickness Padding { get; set; } = new(2);

    [Prop] public Thickness Margin { get; set; } = new(0);

    [Prop] public Align? ContentAlign { get; set; } = Align.TopLeft;

    [Prop] public float? Opacity { get; set; }

    [Prop] public float? BorderOpacity { get; set; }

    [Prop] public HoverEffect HoverVariant { get; set; } = HoverEffect.None;

    [Event] public EventHandler<Event<Box>>? OnClick { get; set; }
}

public static class BoxExtensions
{
    public static Box Background(this Box box, Colors color) => box with { Background = color };

    public static Box Background(this Box box, Colors color, float opacity) => box with { Background = color, Opacity = (1.0f - opacity) * 100 };

    public static Box Opacity(this Box box, float opacity) => box with { Opacity = opacity };

    public static Box BorderColor(this Box box, Colors color) => box with { BorderColor = color };

    public static Box BorderColor(this Box box, Colors color, float opacity) => box with { BorderColor = color, BorderOpacity = (1.0f - opacity) * 100 };

    public static Box BorderOpacity(this Box box, float opacity) => box with { BorderOpacity = opacity };

    public static Box BorderThickness(this Box box, int thickness) => box with { BorderThickness = new(thickness) };

    public static Box BorderThickness(this Box box, Thickness thickness) => box with { BorderThickness = thickness };

    public static Box BorderRadius(this Box box, BorderRadius radius) => box with { BorderRadius = radius };

    public static Box BorderStyle(this Box box, BorderStyle style) => box with { BorderStyle = style };

    public static Box Padding(this Box box, int padding) => box with { Padding = new(padding) };

    public static Box Padding(this Box box, Thickness padding) => box with { Padding = padding };

    public static Box Margin(this Box box, int margin) => box with { Margin = new(margin) };

    public static Box Margin(this Box box, Thickness margin) => box with { Margin = margin };

    public static Box Content(this Box box, params object[] content) => box with { Children = content };

    public static Box ContentAlign(this Box box, Align? align) => box with { ContentAlign = align };

    public static Box WithBox(this object anything)
    {
        return new Box(anything);
    }

    public static Box WithCell(this object anything)
    {
        return new Box(anything)
        {
            BorderRadius = Ivy.BorderRadius.None,
            BorderStyle = Ivy.BorderStyle.None,
            BorderThickness = new(0),
            Padding = new(0),
            Background = null,
            ContentAlign = Align.Left
        }.Width(Size.Full()).Height(Size.Full());
    }

    public static Box Grow(this Box box) => box.Width(Size.Grow());

    public static Box Hover(this Box box, HoverEffect variant) => box with { HoverVariant = variant };

    private static HoverEffect HoverVariantWithClick(this Box box) => box.HoverVariant == HoverEffect.None ? HoverEffect.PointerAndTranslate : box.HoverVariant;

    public static Box OnClick(this Box box, Func<Event<Box>, ValueTask> onClick)
    {
        return box with
        {
            HoverVariant = box.HoverVariantWithClick(),
            OnClick = new(onClick)
        };
    }

    public static Box OnClick(this Box box, Action<Event<Box>> onClick)
    {
        return box with
        {
            HoverVariant = box.HoverVariantWithClick(),
            OnClick = new(onClick.ToValueTask())
        };
    }

    public static Box OnClick(this Box box, Action onClick)
    {
        return box with
        {
            HoverVariant = box.HoverVariantWithClick(),
            OnClick = new(_ => { onClick(); return ValueTask.CompletedTask; })
        };
    }

    public static Box OnClick(this Box box, Func<ValueTask> onClick)
    {
        return box with
        {
            HoverVariant = box.HoverVariantWithClick(),
            OnClick = new(_ => onClick())
        };
    }
}