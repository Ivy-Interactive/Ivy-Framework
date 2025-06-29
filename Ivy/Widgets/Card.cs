using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Card : WidgetBase<Card>
{
    public Card(object? content = null, object? footer = null) : base([new Slot("Content", content), new Slot("Footer", footer!)])
    {
        Width = Size.Full();
    }

    [Prop] public string? Title { get; set; }
    [Prop] public string? Description { get; set; }
    [Prop] public Icons? Icon { get; set; }

    public static Card operator |(Card widget, object child)
    {
        if (child is IEnumerable<object> _)
        {
            throw new NotSupportedException("Cards does not support multiple children.");
        }

        return widget with { Children = [new Slot("Content", child), new Slot("Footer", null!)] };
    }
}

public static class CardExtensions
{
    public static Card Title(this Card card, string title)
    {
        return card with { Title = title };
    }

    public static Card Description(this Card card, string description)
    {
        return card with { Description = description };
    }

    public static Card Icon(this Card card, Icons? icon)
    {
        return card with { Icon = icon };
    }
}