// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents keyboard input.
/// </summary>
public record Kbd : WidgetBase<Kbd>
{
    public Kbd(string content)
    {
        Content = content;
    }

    public Kbd(object content) : base(content)
    {
    }

    internal Kbd() { }

    [Prop] public string Content { get; set; } = string.Empty;

    [Prop] public bool Ghost { get; set; }
}

public static class KbdExtensions
{
    public static Kbd Ghost(this Kbd kbd, bool ghost = true)
    {
        return kbd with { Ghost = ghost };
    }
}
