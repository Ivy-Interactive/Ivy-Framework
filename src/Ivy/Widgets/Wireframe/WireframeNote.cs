// ReSharper disable once CheckNamespace
namespace Ivy;

public record WireframeNote : WidgetBase<WireframeNote>
{
    public WireframeNote(string? text = null, Colors color = Colors.Yellow)
    {
        Text = text;
        Color = color;
    }

    internal WireframeNote() { }

    [Prop] public string? Text { get; set; }

    [Prop] public Colors Color { get; set; } = Colors.Yellow;

    public static WireframeNote operator |(WireframeNote widget, object child)
    {
        throw new NotSupportedException("WireframeNote does not support children. Use the Text property.");
    }
}

public static class WireframeNoteExtensions
{
    public static WireframeNote Text(this WireframeNote note, string text)
        => note with { Text = text };

    public static WireframeNote Color(this WireframeNote note, Colors color)
        => note with { Color = color };
}
