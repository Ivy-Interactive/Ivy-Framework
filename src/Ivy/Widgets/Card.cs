using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

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
        Width = Ivy.Size.Full();
    }

    internal Card() { }

    internal object? Title { get; set; }
    internal object? Description { get; set; }
    internal object? Icon { get; set; }

    [Prop] public HoverEffect HoverVariant { get; set; } = HoverEffect.None;

    [Prop] public bool Disabled { get; set; }

    [Event] public EventHandler<Event<Card>>? OnClick { get; set; }

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

    private static object[] WithSlot(Card card, string slotName, object? value)
    {
        var others = card.Children.OfType<Slot>().Where(s => s.Name != slotName);
        var result = value != null ? others.Append(new Slot(slotName, value)) : others;
        return result.Cast<object>().ToArray();
    }

    public static Card Header(this Card card, object? title = null, object? description = null, object? icon = null)
    {
        object? header = Layout.Vertical().Gap(0)
                         | (Layout.Horizontal().AlignContent(Align.Center)
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

    public static Card Content(this Card card, object? content) =>
        card with { Children = WithSlot(card, "Content", content) };

    public static Card Footer(this Card card, object? footer) =>
        card with { Children = WithSlot(card, "Footer", footer) };

    public static Card Hover(this Card card, HoverEffect variant) => card with { HoverVariant = variant };

    public static Card Disabled(this Card card, bool disabled = true) => card with { Disabled = disabled };

    private static HoverEffect HoverVariantWithClick(this Card card) => card.HoverVariant == HoverEffect.None ? HoverEffect.PointerAndTranslate : card.HoverVariant;

    public static Card OnClick(this Card card, Func<Event<Card>, ValueTask> onClick)
    {
        return card with
        {
            HoverVariant = card.HoverVariantWithClick(),
            OnClick = new(onClick)
        };
    }

    public static Card OnClick(this Card card, Action<Event<Card>> onClick)
    {
        return card with
        {
            HoverVariant = card.HoverVariantWithClick(),
            OnClick = new(onClick.ToValueTask())
        };
    }

    public static Card OnClick(this Card card, Action onClick)
    {
        return card with
        {
            HoverVariant = card.HoverVariantWithClick(),
            OnClick = new(_ => { onClick(); return ValueTask.CompletedTask; })
        };
    }

    public static Card OnClick(this Card card, Func<ValueTask> onClick)
    {
        return card with
        {
            HoverVariant = card.HoverVariantWithClick(),
            OnClick = new(_ => onClick())
        };
    }
}