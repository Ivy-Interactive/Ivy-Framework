using Ivy.Core;
using Ivy.Shared;

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
    public Card(object? content = null, object? footer = null) : base([new Slot("Content", content), new Slot("Footer", footer!)])
    {
        Width = Ivy.Shared.Size.Full();
    }

    /// <summary>Gets or sets the title text displayed at the top of the card.</summary>
    [Prop] public string? Title { get; set; }

    /// <summary>Gets or sets the description text displayed below the title.</summary>
    [Prop] public string? Description { get; set; }

    /// <summary>Gets or sets the icon displayed alongside the title and description.</summary>
    [Prop] public Icons? Icon { get; set; }

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

        return widget with { Children = [new Slot("Content", child), new Slot("Footer", null!)] };
    }
}

/// <summary>Extension methods for configuring Card widget properties. </summary>
public static class CardExtensions
{
    /// <summary>Sets the title text for the card.</summary>
    /// <param name="card">The Card to configure.</param>
    /// <param name="title">The title text to display at the top of the card.</param>
    public static Card Title(this Card card, string title)
    {
        return card with { Title = title };
    }

    /// <summary>Sets the description text for the card.</summary>
    /// <param name="card">The Card to configure.</param>
    /// <param name="description">The description text to display below the title.</param>
    public static Card Description(this Card card, string description)
    {
        return card with { Description = description };
    }

    /// <summary>Sets the icon for the card.</summary>
    /// <param name="card">The Card to configure.</param>
    /// <param name="icon">The icon to display alongside the title and description.</param>
    public static Card Icon(this Card card, Icons? icon)
    {
        return card with { Icon = icon };
    }

    /// <summary>Sets the border thickness for the card using an integer value.</summary>
    /// <param name="card">The Card to configure.</param>
    /// <param name="thickness">The uniform border thickness to apply to all sides.</param>
    public static Card BorderThickness(this Card card, int thickness) => card with { BorderThickness = new(thickness) };

    /// <summary>Sets the border thickness for the card using a Thickness object.</summary>
    /// <param name="card">The Card to configure.</param>
    /// <param name="thickness">The Thickness object defining border thickness for each side.</param>
    public static Card BorderThickness(this Card card, Thickness thickness) => card with { BorderThickness = thickness };

    /// <summary>Sets the border radius for the card's corners.</summary>
    /// <param name="card">The Card to configure.</param>
    /// <param name="radius">The border radius to apply to the card's corners.</param>
    public static Card BorderRadius(this Card card, BorderRadius radius) => card with { BorderRadius = radius };

    /// <summary>Sets the visual style of the card's border.</summary>
    /// <param name="card">The Card to configure.</param>
    /// <param name="style">The border style to apply to the card.</param>
    public static Card BorderStyle(this Card card, BorderStyle style) => card with { BorderStyle = style };

    /// <summary>Sets the color of the card's border.</summary>
    /// <param name="card">The Card to configure.</param>
    /// <param name="color">The color to apply to the card's border.</param>
    public static Card BorderColor(this Card card, Colors color) => card with { BorderColor = color };

    /// <summary>Sets the size variant for the card.</summary>
    /// <param name="card">The Card to configure.</param>
    /// <param name="size">The size variant to apply to the card.</param>
    public static Card Size(this Card card, Sizes size) => card with { Size = size };

    /// <summary>Sets the card size to Small.</summary>
    /// <param name="card">The Card to configure.</param>
    public static Card Small(this Card card) => card with { Size = Sizes.Small };

    /// <summary>Sets the card size to Medium.</summary>
    /// <param name="card">The Card to configure.</param>
    public static Card Medium(this Card card) => card with { Size = Sizes.Medium };

    /// <summary>Sets the card size to Large.</summary>
    /// <param name="card">The Card to configure.</param>
    public static Card Large(this Card card) => card with { Size = Sizes.Large };

    /// <summary>Sets the style variant to apply on cursor hover.</summary>
    /// <param name="card">Card to configure.</param>
    /// <param name="variant">Style variants to apply on cursor hover.</param>
    /// <returns>New Card instance with updated hover variant.</returns>
    public static Card Hover(this Card card, CardHoverVariant variant)
    {
        return card with { HoverVariant = variant };
    }

    private static CardHoverVariant HoverVariantWithClick(this Card card)
    {
        return card.HoverVariant == CardHoverVariant.None ? CardHoverVariant.PointerAndTranslate : card.HoverVariant;
    }

    /// <summary>Sets the click event handler for the card.</summary>
    /// <param name="card">The Card to configure.</param>
    /// <param name="onClick">Event handler to call when clicked.</param>
    /// <returns>New Card instance with updated click handler.</returns>
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

    /// <summary>Sets a simple click event handler for the card.</summary>
    /// <param name="card">The Card to configure.</param>
    /// <param name="onClick">Simple action to perform when clicked.</param>
    /// <returns>New Card instance with updated click handler.</returns>
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