using Ivy.Core;
using Ivy.Widgets.ScreenshotFeedback;

// ReSharper disable once CheckNamespace
namespace Ivy;

public static class ScreenshotFeedbackExtensions
{
    public static ScreenshotFeedback UploadUrl(this ScreenshotFeedback w, string? url) =>
        w with { UploadUrl = url };

    public static ScreenshotFeedback Open(this ScreenshotFeedback w, bool isOpen) =>
        w with { IsOpen = isOpen };

    public static ScreenshotFeedback HandleSave(this ScreenshotFeedback w,
        Func<Event<ScreenshotFeedback>, ValueTask> handler) =>
        w with { OnSave = handler };

    public static ScreenshotFeedback HandleSave(this ScreenshotFeedback w, Action handler) =>
        w with { OnSave = _ => { handler(); return ValueTask.CompletedTask; } };

    public static ScreenshotFeedback HandleCancel(this ScreenshotFeedback w,
        Func<Event<ScreenshotFeedback>, ValueTask> handler) =>
        w with { OnCancel = handler };

    public static ScreenshotFeedback HandleCancel(this ScreenshotFeedback w, Action handler) =>
        w with { OnCancel = _ => { handler(); return ValueTask.CompletedTask; } };
}
