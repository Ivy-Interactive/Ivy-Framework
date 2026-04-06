// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents keyboard input.
/// </summary>
public record Kbd : WidgetBase<Kbd>
{
    public Kbd(object content) : base(content)
    {
    }

    internal Kbd() { }
}