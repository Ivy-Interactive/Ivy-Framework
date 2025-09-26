using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Widget for displaying keyboard keys and key combinations with distinctive styling and semantic representation.</summary>
public record Kbd : WidgetBase<Kbd>
{
    /// <summary>Initializes keyboard input widget with specified key or key combination content.</summary>
    /// <param name="content">Keyboard key, key combination, or input instruction to display.</param>
    public Kbd(object content) : base(content)
    {
    }
}