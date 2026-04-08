// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A graphical representation of a user or entity.
/// </summary>
public record Avatar : WidgetBase<Avatar>
{
    public Avatar(string fallback, string? image = null)
    {
        Fallback = fallback;
        Image = image;
    }

    internal Avatar()
    {
    }

    [Prop] public string Fallback { get; set; } = string.Empty;

    [Prop] public string? Image { get; set; }

    [Prop] public Colors? Color { get; set; }
}

public static class AvatarExtensions
{
    public static Avatar Fallback(this Avatar avatar, string fallback) => avatar with { Fallback = fallback };

    public static Avatar Image(this Avatar avatar, string? image) => avatar with { Image = image };

    public static Avatar Color(this Avatar avatar, Colors color) => avatar with { Color = color };
}