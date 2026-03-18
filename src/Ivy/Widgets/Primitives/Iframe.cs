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

    [Prop] public string? OutboundMessageType { get; set; }

    [Prop] public string? OutboundMessageToken { get; set; }

    [Event] public Func<Event<Iframe, (string type, JsonNode payload)>, ValueTask>? OnMessageReceived { get; set; }
}

public static class IframeExtensions
{
    public static Iframe OnMessageReceived(
        this Iframe widget,
        Func<Event<Iframe, (string type, JsonNode payload)>, ValueTask> callback)
    {
        widget.OnMessageReceived = callback;
        return widget;
    }
}