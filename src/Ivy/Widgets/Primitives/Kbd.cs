using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Displays a keyboard shortcut or key combination component.
/// </summary>
public record Kbd : WidgetBase<Kbd>
{
    public Kbd(object content) : base(content)
    {
    }

    internal Kbd() { }
}