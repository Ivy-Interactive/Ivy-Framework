using System.Text.Json.Nodes;
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Embeds an iframe.
/// </summary>
public record Iframe : WidgetBase<Iframe>
{
    public Iframe(string src, long? refreshToken = null) : this()
    {
        Src = src;
        RefreshToken = refreshToken;
    }

    internal Iframe()
    {
        Width = Size.Full();
        Height = Size.Full();
    }

    [Prop] public string Src { get; set; } = null!;

    [Prop] public long? RefreshToken { get; }
}