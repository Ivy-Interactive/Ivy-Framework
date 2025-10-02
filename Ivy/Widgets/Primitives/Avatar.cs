using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>User avatar widget with profile image and text fallback (typically initials).</summary>
public record Avatar : WidgetBase<Avatar>
{
    /// <summary>Initializes avatar.</summary>
    /// <param name="fallback">Fallback text (1-3 chars, typically initials).</param>
    /// <param name="image">Optional profile image URL/path.</param>
    public Avatar(string fallback, string? image = null)
    {
        Fallback = fallback;
        Image = image;
    }

    /// <summary>Fallback text shown when image unavailable.</summary>
    [Prop] public string Fallback { get; set; }

    /// <summary>Profile image URL/path.</summary>
    [Prop] public string? Image { get; set; }
}

/// <summary>Extension methods for Avatar widget configuration.</summary>
public static class AvatarExtensions
{
    /// <summary>Sets fallback text.</summary>
    public static Avatar Fallback(this Avatar avatar, string fallback) => avatar with { Fallback = fallback };

    /// <summary>Sets profile image.</summary>
    public static Avatar Image(this Avatar avatar, string? image) => avatar with { Image = image };
}