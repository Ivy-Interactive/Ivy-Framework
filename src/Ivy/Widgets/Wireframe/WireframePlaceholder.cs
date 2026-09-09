// ReSharper disable once CheckNamespace
namespace Ivy;

public record WireframePlaceholder : WidgetBase<WireframePlaceholder>
{
    public WireframePlaceholder(string? text = null, Colors color = Colors.Violet)
    {
        Text = text;
        Color = color;
    }

    internal WireframePlaceholder() { }

    [Prop] public string? Text { get; set; }

    [Prop] public Colors Color { get; set; } = Colors.Violet;

    public static WireframePlaceholder operator |(WireframePlaceholder widget, object child)
    {
        throw new NotSupportedException("WireframePlaceholder does not support children. Use the Text property.");
    }
}

public static class WireframePlaceholderExtensions
{
    public static WireframePlaceholder Text(this WireframePlaceholder placeholder, string text)
        => placeholder with { Text = text };

    public static WireframePlaceholder Color(this WireframePlaceholder placeholder, Colors color)
        => placeholder with { Color = color };
}
