namespace Ivy.Widgets.ClaudeJsonRenderer;

[ExternalWidget("frontend/dist/Ivy_Widgets_ClaudeJsonRenderer.js",
                StylePath = "frontend/dist/ivy-widgets-claudejsonrenderer.css",
                ExportName = "ClaudeJsonRenderer")]
public record ClaudeJsonRenderer : WidgetBase<ClaudeJsonRenderer>
{
    public ClaudeJsonRenderer()
    {
        Width = Size.Full();
    }

    /// <summary>Stream of newline-delimited JSON events from Claude Code</summary>
    [Prop] public string? JsonStream { get; init; }

    /// <summary>Auto-scroll to bottom as new events arrive</summary>
    [Prop] public bool AutoScroll { get; init; } = true;

    /// <summary>Show thinking blocks (assistant reasoning)</summary>
    [Prop] public bool ShowThinking { get; init; } = false;

    /// <summary>Show system events (init, tool results)</summary>
    [Prop] public bool ShowSystemEvents { get; init; } = false;

    [Event] public Func<Event<ClaudeJsonRenderer, string>, ValueTask>? OnComplete { get; init; }
}

public static class ClaudeJsonRendererExtensions
{
    public static ClaudeJsonRenderer JsonStream(this ClaudeJsonRenderer w, string jsonStream) =>
        w with { JsonStream = jsonStream };

    public static ClaudeJsonRenderer AutoScroll(this ClaudeJsonRenderer w, bool autoScroll = true) =>
        w with { AutoScroll = autoScroll };

    public static ClaudeJsonRenderer ShowThinking(this ClaudeJsonRenderer w, bool showThinking = true) =>
        w with { ShowThinking = showThinking };

    public static ClaudeJsonRenderer ShowSystemEvents(this ClaudeJsonRenderer w, bool showSystemEvents = true) =>
        w with { ShowSystemEvents = showSystemEvents };

    public static ClaudeJsonRenderer OnComplete(
        this ClaudeJsonRenderer w,
        Func<Event<ClaudeJsonRenderer, string>, ValueTask> handler
    ) => w with { OnComplete = handler };

    public static ClaudeJsonRenderer OnComplete(this ClaudeJsonRenderer w, Action<string> handler) =>
        w with
        {
            OnComplete = e =>
            {
                handler(e.Value);
                return ValueTask.CompletedTask;
            },
        };
}
