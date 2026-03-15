using Ivy;
using Ivy.Widgets.ScreenshotFeedback;

var server = new Server();
server.AddApp<ScreenshotFeedbackApp>();
await server.RunAsync();

[App(title: "Screenshot Feedback", icon: Icons.Camera)]
class ScreenshotFeedbackApp : ViewBase
{
    public override object? Build()
    {
        var screenshot = UseState<FileUpload<byte[]>?>();
        var uploadCtx = UseUpload(MemoryStreamUploadHandler.Create(screenshot));
        var isOpen = UseState(false);

        return Layout.Vertical().Gap(4)
               | Text.H1("Screenshot Feedback Demo")
               | Text.P("Click the button below to capture a screenshot, annotate it, and upload.")
               | new Button("Take Screenshot", () => isOpen.Set(true), icon: Icons.Camera)
               | new ScreenshotFeedback()
                   .UploadUrl(uploadCtx.Value.UploadUrl)
                   .Open(isOpen.Value)
                   .HandleSave(() => isOpen.Set(false))
                   .HandleCancel(() => isOpen.Set(false))
               | (screenshot.Value?.Status == FileUploadStatus.Finished && screenshot.Value.Content != null
                   ? (object)(Layout.Vertical().Gap(2)
                       | Text.H2("Captured Screenshot")
                       | new Image("data:image/png;base64," + Convert.ToBase64String(screenshot.Value.Content))
                           .Width(Size.Full()))
                   : Text.Muted("No screenshot captured yet."));
    }
}
