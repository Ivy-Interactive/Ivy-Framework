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

/// <summary>
/// A flexible container with a border and shadow for grouping related content.
/// </summary>
public record Card : WidgetBase<Card>
{
    public Card(object? content = null, object? footer = null, object? header = null) : base(
        new List<object?>
        {
            content != null ? new Slot("Content", content) : null,
            footer != null ? new Slot("Footer", footer) : null,
            header != null ? new Slot("Header", header) : null
        }.Where(x => x != null).Cast<object>().ToArray())
    {
        Width = Ivy.Shared.Size.Full();
    }

    internal Card() { }

    internal object? Title { get; set; }
    internal object? Description { get; set; }
    internal object? Icon { get; set; }



    [Prop] public CardHoverVariant HoverVariant { get; set; } = CardHoverVariant.None;

    [Event] public Func<Event<Card>, ValueTask>? OnClick { get; set; }

    public static Card operator |(Card widget, object child)
    {
        if (child is IEnumerable<object> _)
        {
            throw new NotSupportedException("Cards does not support multiple children.");
        }

        var slots = new List<object?>
        {
            new Slot("Content", child),
            widget.GetSlot("Footer"),
            widget.GetSlot("Header")
        };

        return widget with { Children = slots.Where(x => x != null).Cast<object>().ToArray() };
    }
}

public static class CardExtensions
{
    internal static Slot? GetSlot(this Card card, string name) => card.Children.FirstOrDefault(e => e is Slot slot && slot.Name == name) as Slot;

    public static Card Header(this Card card, object? title = null, object? description = null, object? icon = null)
    {
        object? header = Layout.Vertical().Gap(0)
                         | (Layout.Horizontal().Align(Align.Center)
                            | title?.WithLayout().Grow()
                            | icon)
                         | description;

        var slots = new List<object?>
        {
            card.GetSlot("Content"),
            card.GetSlot("Footer"),
            header != null ? new Slot("Header", header) : null
        };

        return card with
        {
            Children = slots.Where(x => x != null).Cast<object>().ToArray(),
            Title = title,
            Description = description,
            Icon = icon
        };
    }

    public static Card Title(this Card card, object? title) => card.Header(title, card.Description, card.Icon);

    public static Card Description(this Card card, object? description) => card.Header(card.Title, description, card.Icon);

    public static Card Icon(this Card card, object? icon)
    {
        if (icon is Icons iconsValue)
        {
            icon = iconsValue.ToIcon().Color(Colors.Neutral);
        }
        return card.Header(card.Title, card.Description, icon);
    }


    public static Card Hover(this Card card, CardHoverVariant variant) => card with { HoverVariant = variant };

    private static CardHoverVariant HoverVariantWithClick(this Card card) => card.HoverVariant == CardHoverVariant.None ? CardHoverVariant.PointerAndTranslate : card.HoverVariant;

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