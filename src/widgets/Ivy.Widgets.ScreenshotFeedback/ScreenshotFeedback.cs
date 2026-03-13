using Ivy;
using Ivy.Core;

namespace Ivy.Widgets.ScreenshotFeedback;

[ExternalWidget("frontend/dist/Ivy_Widgets_ScreenshotFeedback.js",
    ExportName = "ScreenshotFeedback",
    GlobalName = "Ivy_Widgets_ScreenshotFeedback",
    StylePath = "frontend/dist/ivy-widgets-screenshot-feedback.css")]
public record ScreenshotFeedback : WidgetBase<ScreenshotFeedback>
{
    public ScreenshotFeedback() { }

    [Prop] public string? UploadUrl { get; init; }

    [Prop] public bool IsOpen { get; init; }

    [Event] public Func<Event<ScreenshotFeedback>, ValueTask>? OnSave { get; init; }

    [Event] public Func<Event<ScreenshotFeedback>, ValueTask>? OnCancel { get; init; }
}
