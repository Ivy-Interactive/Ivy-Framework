namespace Ivy.Widgets.PlanAdjuster;

[ExternalWidget("frontend/dist/Ivy_Widgets_PlanAdjuster.js",
                StylePath = "frontend/dist/ivy-widgets-planadjuster.css",
                ExportName = "PlanAdjuster")]
public record PlanAdjuster : WidgetBase<PlanAdjuster>
{
    /// <summary>The markdown content to render</summary>
    [Prop] public string Content { get; init; } = string.Empty;

    /// <summary>Allow local file:// links in the markdown</summary>
    [Prop] public bool DangerouslyAllowLocalFiles { get; init; }

    /// <summary>
    /// Fires when the user clicks "Update" with all adjustments.
    /// Value is a JSON string: { "adjustments": [{ "paragraphIndex": 0, "text": "..." }, ...] }
    /// </summary>
    [Event] public Func<Event<PlanAdjuster, string>, ValueTask>? OnUpdate { get; init; }

    /// <summary>Fires when a link is clicked in the markdown. Value is the URL.</summary>
    [Event] public Func<Event<PlanAdjuster, string>, ValueTask>? OnLinkClick { get; init; }
}

public static class PlanAdjusterExtensions
{
    public static PlanAdjuster Content(this PlanAdjuster w, string content) =>
        w with { Content = content };

    public static PlanAdjuster DangerouslyAllowLocalFiles(this PlanAdjuster w, bool allow = true) =>
        w with { DangerouslyAllowLocalFiles = allow };

    public static PlanAdjuster OnUpdate(this PlanAdjuster w,
        Func<Event<PlanAdjuster, string>, ValueTask> handler) =>
        w with { OnUpdate = handler };

    public static PlanAdjuster OnUpdate(this PlanAdjuster w, Action<string> handler) =>
        w with
        {
            OnUpdate = e =>
            {
                handler(e.Value);
                return ValueTask.CompletedTask;
            },
        };

    public static PlanAdjuster OnLinkClick(this PlanAdjuster w,
        Func<Event<PlanAdjuster, string>, ValueTask> handler) =>
        w with { OnLinkClick = handler };

    public static PlanAdjuster OnLinkClick(this PlanAdjuster w, Action<string> handler) =>
        w with
        {
            OnLinkClick = e =>
            {
                handler(e.Value);
                return ValueTask.CompletedTask;
            },
        };
}
