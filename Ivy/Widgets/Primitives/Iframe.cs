using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Iframe widget for embedding external web content. Default size: full. Security: isolated context.</summary>
public record Iframe : WidgetBase<Iframe>
{
    /// <summary>Initializes iframe. Default size: full width and height.</summary>
    /// <param name="src">External content URL.</param>
    /// <param name="refreshToken">Optional refresh control token.</param>
    public Iframe(string src, long? refreshToken = null)
    {
        Src = src;
        Width = Size.Full();
        Height = Size.Full();
        RefreshToken = refreshToken;
    }

    /// <summary>External content URL.</summary>
    [Prop] public string Src { get; set; }

    /// <summary>Refresh control token (changing triggers reload).</summary>
    [Prop] public long? RefreshToken { get; }
}