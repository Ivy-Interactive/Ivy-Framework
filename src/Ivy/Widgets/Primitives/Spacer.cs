// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Adds empty space between elements.
/// By default, grows to fill available space in the parent layout's direction.
/// </summary>
public record Spacer : WidgetBase<Spacer>
{
    public Spacer()
    {
        Width = Size.Grow();
    }

    public static Spacer operator |(Spacer widget, object child)
    {
        throw new NotSupportedException("Spacer does not support children.");
    }
}