
namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.Camera, group: ["Widgets", "Inputs"], searchHints: ["camera", "webcam", "photo", "capture", "snapshot", "picture"])]

public class CameraInputApp() : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("Camera Input")
               | Layout.Tabs(
                   new Tab("Examples", new CameraInputBasic()),
                   new Tab("States", new CameraInputStates()),
                   new Tab("Validation", new CameraInputValidation()),
                   new Tab("Sizes", new CameraInputSizes()),
                   new Tab("Events", new CameraInputEvents())
               ).Variant(TabsVariant.Content);
    }
}

public class CameraInputBasic : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var photo = UseState<FileUpload<byte[]>?>();

        var upload = UseUpload(
            MemoryStreamUploadHandler.Create(photo),
            defaultContentType: "image/png"
        );

        UseEffect(() =>
        {
            if (photo.Value?.Status == FileUploadStatus.Finished)
            {
                client.Toast($"Photo captured: {StringHelper.FormatBytes(photo.Value.Length)}", "Upload Complete");
            }
        }, photo);

        return Layout.Vertical().Gap(4)
               | Text.P("Basic CameraInput example. Captures a photo from your webcam and uploads it.")
               | new CameraInput(upload.Value, "Take a photo")
               | (photo.Value != null
                   ? Text.P($"Captured: {photo.Value.FileName} ({StringHelper.FormatBytes(photo.Value.Length)})").Small()
                   : Text.P("No photo captured yet").Small());
    }
}

public class CameraInputStates : ViewBase
{
    public override object? Build()
    {
        var dummyUpload = UseUpload(
            (fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask,
            defaultContentType: "image/png"
        );

        return Layout.Grid().Columns(4)
               | Text.Monospaced("Description")
               | Text.Monospaced("Default")
               | Text.Monospaced("Disabled")
               | Text.Monospaced("Invalid")

               | Text.Monospaced("Camera Input")
               | new CameraInput(dummyUpload.Value, "Take a photo")
               | new CameraInput(dummyUpload.Value, "Take a photo", disabled: true)
               | new CameraInput(dummyUpload.Value, "Take a photo").Invalid("Camera input is required");
    }
}

public class CameraInputValidation : ViewBase
{
    public override object? Build()
    {
        var dummyUpload = UseUpload(
            (fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask,
            defaultContentType: "image/png"
        );

        return Layout.Vertical().Gap(4)
               | Text.P("Demonstrates CameraInput with validation error state.")
               | new CameraInput(dummyUpload.Value, "Take a photo")
                   .Invalid("Camera input is invalid");
    }
}

public class CameraInputSizes : ViewBase
{
    public override object? Build()
    {
        var dummyUpload = UseUpload(
            (fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask,
            defaultContentType: "image/png"
        );

        return Layout.Grid().Columns(4)
               | Text.Monospaced("Description")
               | Text.Monospaced("Small")
               | Text.Monospaced("Medium")
               | Text.Monospaced("Large")

               | Text.Monospaced("Camera Input")
               | new CameraInput(dummyUpload.Value, "Take a photo").Small()
               | new CameraInput(dummyUpload.Value, "Take a photo")
               | new CameraInput(dummyUpload.Value, "Take a photo").Large()

               | Text.Monospaced("Disabled State")
               | new CameraInput(dummyUpload.Value, "Take a photo", disabled: true).Small()
               | new CameraInput(dummyUpload.Value, "Take a photo", disabled: true)
               | new CameraInput(dummyUpload.Value, "Take a photo", disabled: true).Large();
    }
}

public class CameraInputEvents : ViewBase
{
    public override object? Build()
    {
        var focused = UseState(false);
        var lastEvent = UseState<string?>(() => null);

        var dummyUpload = UseUpload(
            (fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask,
            defaultContentType: "image/png"
        );

        void onFocus()
        {
            focused.Set(true);
            lastEvent.Set("OnFocus");
        }

        void onBlur()
        {
            focused.Set(false);
            lastEvent.Set("OnBlur");
        }

        return Layout.Vertical()
               | Text.P("Focus the camera input (Tab / click), then blur it to trigger events.")
               | new CameraInput(dummyUpload.Value, "Take a photo")
                   .OnFocus(onFocus)
                   .OnBlur(onBlur)
               | Text.P($"Focused: {focused.Value}").Small()
               | Text.P($"Last event: {lastEvent.Value ?? "—"}").Small();
    }
}
