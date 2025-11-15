using Ivy.Core;
using Ivy.Shared;
using Ivy.Views;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Card style variants applied on cursor hover.</summary>
public enum CardHoverVariant
{
    /// <summary>No styling applied on hover.</summary>
    None,
    /// <summary>Applies "cursor: pointer".</summary>
    Pointer,
    /// <summary>Applies "cursor: pointer" and adds a minor translate effect on hover.</summary>
    PointerAndTranslate,
}

/// <summary>A structured container for organizing related content with optional title, description, and icon.</summary>
public record Card : WidgetBase<Card>
{
    /// <summary>
    /// Initializes a new Card with the specified content and optional footer.
    /// </summary>
    /// <param name="content">The main content to display in the card body.</param>
    /// <param name="footer">Optional footer content displayed at the bottom.</param>
    public Card(object? content = null, object? footer = null, object? header = null) : base([new Slot("Content", content), new Slot("Footer", footer!), new Slot("Header", header!)])
    {
        Width = Ivy.Shared.Size.Full();
    }

    internal object? Title { get; set; }
    internal object? Description { get; set; }
    internal object? Icon { get; set; }

    /// <summary>Gets or sets the thickness of the card's border.</summary>
    [Prop] public Thickness? BorderThickness { get; set; }

    /// <summary>Gets or sets the border radius for the card's corners.</summary>
    [Prop] public BorderRadius? BorderRadius { get; set; }

    /// <summary>Gets or sets the visual style of the card's border.</summary>
    [Prop] public BorderStyle? BorderStyle { get; set; }

    /// <summary>Gets or sets the color of the card's border.</summary>
    [Prop] public Colors? BorderColor { get; set; }

    /// <summary>Style variant to apply on cursor hover. Default is <see cref="CardHoverVariant.None"/> when no click listener is applied, and <see cref="CardHoverVariant.PointerAndTranslate"/> when a click listener is applied.</summary>
    [Prop] public CardHoverVariant HoverVariant { get; set; }

    /// <summary>Event handler called when card is clicked. Default is null.</summary>
    [Event] public Func<Event<Card>, ValueTask>? OnClick { get; set; }

    /// <summary>Gets or sets the size variant of the card. Default is Medium.</summary>
    [Prop] public Sizes Size { get; set; } = Sizes.Medium;

    /// <summary>
    /// Adds content to the card's main content area using the pipe operator.
    /// </summary>
    /// <param name="widget">The Card widget to add content to.</param>
    /// <param name="child">The child content to add to the card's main content area.</param>
    /// <returns>A new Card instance with the updated content.</returns>
    /// <exception cref="NotSupportedException">Thrown when attempting to add multiple children at once.</exception>
    public static Card operator |(Card widget, object child)
    {
        if (child is IEnumerable<object> _)
        {
            throw new NotSupportedException("Cards does not support multiple children.");
        }
        return widget with { Children = [new Slot("Content", child), widget.GetSlot("Footer"), widget.GetSlot("Header")] };
    }
}

/// <summary>Extension methods for configuring Card widget properties. </summary>
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

    public static Card Size(this Card card, Sizes size) => card with { Size = size };

    public static Card Small(this Card card) => card with { Size = Sizes.Small };

    public static Card Medium(this Card card) => card with { Size = Sizes.Medium };

    public static Card Large(this Card card) => card with { Size = Sizes.Large };

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