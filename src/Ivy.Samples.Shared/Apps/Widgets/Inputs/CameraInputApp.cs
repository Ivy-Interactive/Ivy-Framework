
namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.Camera, group: ["Widgets", "Inputs"], searchHints: ["camera", "webcam", "photo", "capture", "snapshot", "picture"])]

public class CameraInputApp() : SampleBase
{
    protected override object? BuildSample()
    {
        var dummyUpload = UseUpload(
            (fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask,
            defaultContentType: "image/png"
        );

        return Layout.Vertical()
               | Text.H1("Camera Input Widget Examples")
               | Text.P("Demonstrates the CameraInput widget for capturing photos from the user's webcam/camera.")
               | Text.H2("Examples")
               | (Layout.Horizontal().Gap(4)
                    | new Card(new CameraInputBasic()).Title("Basic")
                    | new Card(new CameraInputDisabledState()).Title("Disabled State"))
               | Text.H2("Sizes")
               | CreateSizesSection(dummyUpload.Value);
    }

    private object CreateSizesSection(UploadContext upload)
    {
        return Layout.Grid().Columns(4)
               | Text.Monospaced("Description")
               | Text.Monospaced("Small")
               | Text.Monospaced("Medium")
               | Text.Monospaced("Large")

               | Text.Monospaced("Camera Input")
               | new CameraInput(upload, "Take a photo").Small()
               | new CameraInput(upload, "Take a photo")
               | new CameraInput(upload, "Take a photo").Large()

               | Text.Monospaced("Disabled State")
               | new CameraInput(upload, "Take a photo", disabled: true).Small()
               | new CameraInput(upload, "Take a photo", disabled: true)
               | new CameraInput(upload, "Take a photo", disabled: true).Large();
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

public class CameraInputDisabledState : ViewBase
{
    public override object? Build()
    {
        var dummyUpload = UseUpload(
            (fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask,
            defaultContentType: "image/png"
        );

        return Layout.Vertical().Gap(4)
               | Text.P("Demonstrates the CameraInput widget in a disabled state. The camera cannot be activated.")
               | new CameraInput(dummyUpload.Value, "Take a photo", disabled: true);
    }
}

