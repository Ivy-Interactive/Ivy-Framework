using Ivy.Core;
using System.Runtime.CompilerServices;

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
    [Event] public EventHandler<Event<PlanAdjuster, string>>? OnUpdate { get; init; }

    /// <summary>Fires when a link is clicked in the markdown. Value is the URL.</summary>
    [Event] public EventHandler<Event<PlanAdjuster, string>>? OnLinkClick { get; init; }
}

public static class PlanAdjusterExtensions
{
    public static PlanAdjuster Content(this PlanAdjuster w, string content) =>
        w with { Content = content };

    public static PlanAdjuster DangerouslyAllowLocalFiles(this PlanAdjuster w, bool allow = true) =>
        w with { DangerouslyAllowLocalFiles = allow };

    [OverloadResolutionPriority(1)]
    public static PlanAdjuster OnUpdate(this PlanAdjuster w,
        Func<Event<PlanAdjuster, string>, ValueTask> handler) =>
        w with { OnUpdate = new(handler) };

    public static PlanAdjuster OnUpdate(this PlanAdjuster w, Action<Event<PlanAdjuster, string>> handler) =>
        w with { OnUpdate = new(handler.ToValueTask()) };

    public static PlanAdjuster OnUpdate(this PlanAdjuster w, Action<string> handler) =>
        w with { OnUpdate = new(@event => { handler(@event.Value); return ValueTask.CompletedTask; }) };

    [OverloadResolutionPriority(1)]
    public static PlanAdjuster OnLinkClick(this PlanAdjuster w,
        Func<Event<PlanAdjuster, string>, ValueTask> handler) =>
        w with { OnLinkClick = new(handler) };

    public static PlanAdjuster OnLinkClick(this PlanAdjuster w, Action<Event<PlanAdjuster, string>> handler) =>
        w with { OnLinkClick = new(handler.ToValueTask()) };

    public static PlanAdjuster OnLinkClick(this PlanAdjuster w, Action<string> handler) =>
        w with { OnLinkClick = new(@event => { handler(@event.Value); return ValueTask.CompletedTask; }) };
}
