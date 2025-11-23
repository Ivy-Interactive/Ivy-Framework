using Ivy.Core;
using Ivy.Shared;
using Ivy.Views;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum CardHoverVariant
{
    None,
    Pointer,
    PointerAndTranslate,
}

public record Card : WidgetBase<Card>
{
    public Card(object? content = null, object? footer = null, object? header = null) : base([new Slot("Content", content), new Slot("Footer", footer!), new Slot("Header", header!)])
    {
        Width = Ivy.Shared.Size.Full();
    }

    internal object? Title { get; set; }
    internal object? Description { get; set; }
    internal object? Icon { get; set; }

    [Prop] public Thickness? BorderThickness { get; set; }

    [Prop] public BorderRadius? BorderRadius { get; set; }

    [Prop] public BorderStyle? BorderStyle { get; set; }

    [Prop] public Colors? BorderColor { get; set; }

    [Prop] public CardHoverVariant HoverVariant { get; set; }

    [Event] public Func<Event<Card>, ValueTask>? OnClick { get; set; }

    public static Card operator |(Card widget, object child)
    {
        if (child is IEnumerable<object> _)
        {
            throw new NotSupportedException("Cards does not support multiple children.");
        }
        return widget with { Children = [new Slot("Content", child), widget.GetSlot("Footer"), widget.GetSlot("Header")] };
    }
}

public static class CardExtensions
{
    internal static Slot GetSlot(this Card card, string name) => card.Children.FirstOrDefault(e => e is Slot slot && slot.Name == name) as Slot ?? new Slot(name, null!);

    public static Card Header(this Card card, object? title = null, object? description = null, object? icon = null)
    {
        object? header = Layout.Horizontal()
                         | (Layout.Vertical().Gap(0) | title | description)
                         | icon!;
        return card with
        {
            Children = [card.GetSlot("Content"), card.GetSlot("Footer"), new Slot("Header", header)],
            Title = title,
            Description = description,
            Icon = icon
        };
    }

    public static Card Title(this Card card, string title)
    {
        return card.Header(Text.Block(title), card.Description, card.Icon);
    }

    public static Card Description(this Card card, string description)
    {
        return card.Header(card.Title, Text.Muted(description), card.Icon);
    }

    public static Card Icon(this Card card, object? icon)
    {
        if (icon is Icons iconsValue)
        {
            icon = iconsValue.ToIcon().Color(Colors.Black);
        }
        return card.Header(card.Title, card.Description, icon);
    }

    public static Card BorderThickness(this Card card, int thickness) => card with { BorderThickness = new(thickness) };

    public static Card BorderThickness(this Card card, Thickness thickness) => card with { BorderThickness = thickness };

    public static Card BorderRadius(this Card card, BorderRadius radius) => card with { BorderRadius = radius };

    public static Card BorderStyle(this Card card, BorderStyle style) => card with { BorderStyle = style };

    public static Card BorderColor(this Card card, Colors color) => card with { BorderColor = color };

    public static Card Hover(this Card card, CardHoverVariant variant)
    {
        return card with { HoverVariant = variant };
    }

    private static CardHoverVariant HoverVariantWithClick(this Card card)
    {
        return card.HoverVariant == CardHoverVariant.None ? CardHoverVariant.PointerAndTranslate : card.HoverVariant;
    }

    public static Card HandleClick(this Card card, Func<Event<Card>, ValueTask> onClick)
    {
        return card with
        {
            HoverVariant = card.HoverVariantWithClick(),
            OnClick = onClick
        };
    }

    public static Card HandleClick(this Card card, Action<Event<Card>> onClick)
    {
        return card with
        {
            HoverVariant = card.HoverVariantWithClick(),
            OnClick = onClick.ToValueTask()
        };
    }

    public static Card HandleClick(this Card card, Action onClick)
    {
        return card with
        {
            HoverVariant = card.HoverVariantWithClick(),
            OnClick = _ => { onClick(); return ValueTask.CompletedTask; }
        };
    }

    public static Card HandleClick(this Card card, Func<ValueTask> onClick)
    {
        return card with
        {
            HoverVariant = card.HoverVariantWithClick(),
            OnClick = _ => onClick()
        };
    }
}