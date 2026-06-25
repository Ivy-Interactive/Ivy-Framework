// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents keyboard input. Each key in a combination is rendered as its own
/// standalone cap, so a single <see cref="Kbd"/> describes a whole shortcut — for
/// example <c>new Kbd("Ctrl+Enter")</c> renders two caps. Modifier and navigation
/// keys are shown as platform-appropriate icons where available.
/// </summary>
public record Kbd : WidgetBase<Kbd>
{
    /// <summary>
    /// Initializes a new <see cref="Kbd"/> from a shortcut string such as
    /// "Ctrl+Enter". The string is split on "+" and each key is rendered as its
    /// own standalone cap.
    /// </summary>
    /// <param name="content">The shortcut string (e.g. "Shift+Ctrl+C").</param>
    public Kbd(string content)
    {
        Keys = content;
    }

    /// <summary>
    /// Initializes a new <see cref="Kbd"/> from arbitrary content, rendered inside a
    /// single key cap. Prefer the string overload for keyboard shortcuts.
    /// </summary>
    /// <param name="content">The content to render inside a single cap.</param>
    public Kbd(object content) : base(content)
    {
    }

    internal Kbd() { }

    /// <summary>The shortcut string to render as standalone key caps (e.g. "Ctrl+Enter").</summary>
    [Prop] public string? Keys { get; set; }

    /// <summary>When true, renders caps without a background or border.</summary>
    [Prop] public bool Ghost { get; set; }
}

/// <summary>
/// Extension methods for <see cref="Kbd"/>.
/// </summary>
public static class KbdExtensions
{
    /// <summary>
    /// Renders the key caps without a background or border, leaving just the key
    /// glyphs — useful for inline, low-emphasis shortcut hints.
    /// </summary>
    public static Kbd Ghost(this Kbd kbd, bool ghost = true)
    {
        return kbd with { Ghost = ghost };
    }
}
