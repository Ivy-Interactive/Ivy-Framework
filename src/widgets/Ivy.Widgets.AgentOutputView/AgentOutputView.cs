namespace Ivy.Widgets.AgentOutputView;

[ExternalWidget("frontend/dist/Ivy_Widgets_AgentOutputView.js",
                StylePath = "frontend/dist/ivy-widgets-agentoutputview.css",
                ExportName = "AgentOutputView")]
public record AgentOutputView : WidgetBase<AgentOutputView>
{
    /// <summary>Agent provider id. One of: "claude", "codex", "gemini", "opencode", "copilot".</summary>
    [Prop] public string Provider { get; init; } = "claude";

    /// <summary>Pre-buffered newline-delimited JSON events from the provider.</summary>
    [Prop] public string? JsonStream { get; init; }

    /// <summary>Live streaming input.</summary>
    [Prop] public IWriteStream<string>? Stream { get; init; }

    /// <summary>Auto-scroll to bottom as new events arrive.</summary>
    [Prop] public bool AutoScroll { get; init; } = true;

    /// <summary>Show thinking/reasoning blocks.</summary>
    [Prop] public bool ShowThinking { get; init; } = false;

    /// <summary>Show system init events.</summary>
    [Prop] public bool ShowSystemEvents { get; init; } = false;

    /// <summary>Show the animated status label at the bottom of the view.</summary>
    [Prop] public bool ShowStatusLabel { get; init; } = true;

    /// <summary>Override the auto-derived status label text. Null = derive from latest event.</summary>
    [Prop] public string? StatusLabelOverride { get; init; }

    /// <summary>Changing this value clears all accumulated stream output.</summary>
    [Prop] public int ResetToken { get; init; } = 0;

    [Event] public Func<Event<AgentOutputView, string>, ValueTask>? OnComplete { get; init; }
}

public static class AgentOutputViewExtensions
{
    public static AgentOutputView Provider(this AgentOutputView w, string provider) =>
        w with { Provider = provider };

    public static AgentOutputView JsonStream(this AgentOutputView w, string jsonStream) =>
        w with { JsonStream = jsonStream };

    public static AgentOutputView Stream(this AgentOutputView w, IWriteStream<string> stream) =>
        w with { Stream = stream };

    public static AgentOutputView AutoScroll(this AgentOutputView w, bool autoScroll = true) =>
        w with { AutoScroll = autoScroll };

    public static AgentOutputView ShowThinking(this AgentOutputView w, bool showThinking = true) =>
        w with { ShowThinking = showThinking };

    public static AgentOutputView ShowSystemEvents(this AgentOutputView w, bool showSystemEvents = true) =>
        w with { ShowSystemEvents = showSystemEvents };

    public static AgentOutputView ShowStatusLabel(this AgentOutputView w, bool showStatusLabel = true) =>
        w with { ShowStatusLabel = showStatusLabel };

    public static AgentOutputView StatusLabel(this AgentOutputView w, string? statusLabel) =>
        w with { StatusLabelOverride = statusLabel };

    public static AgentOutputView ResetToken(this AgentOutputView w, int resetToken) =>
        w with { ResetToken = resetToken };

    public static AgentOutputView OnComplete(
        this AgentOutputView w,
        Func<Event<AgentOutputView, string>, ValueTask> handler
    ) => w with { OnComplete = handler };

    public static AgentOutputView OnComplete(this AgentOutputView w, Action<string> handler) =>
        w with
        {
            OnComplete = e =>
            {
                handler(e.Value);
                return ValueTask.CompletedTask;
            },
        };
}
