
namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;


[App(icon: Icons.Mic, path: ["Widgets", "Inputs"], searchHints: ["microphone", "recording", "voice", "audio", "capture", "sound"])]

public class AudioInputApp() : SampleBase
{
    protected override object? BuildSample()
    {
        // Create a dummy upload for display-only examples
        var dummyUpload = UseUpload(
            (fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask,
            defaultContentType: "audio/webm"
        );

        return Layout.Vertical()
               | Text.H1("Audio Input Widget Examples")
               | Text.P("Demonstrates the AudioInput widget for capturing audio input. This widget is for recording audio, not playing it. The recorder interface is theme-aware and adapts to light/dark themes.")
               | Text.H2("Upload Examples")
               | (Layout.Horizontal().Gap(4)
                    | new Card(new AudioInputBasic()).Title("Basic")
                    | new Card(new AudioInputChunkedUpload()).Title("Chunked Upload")
                    | new Card(new AudioInputDisabledState()).Title("Disabled State"))
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

               | Text.Monospaced("Audio Input")
               | new AudioInput(upload, "Start recording", "Recording audio...").Small()
               | new AudioInput(upload, "Start recording", "Recording audio...")
               | new AudioInput(upload, "Start recording", "Recording audio...").Large()

               | Text.Monospaced("Disabled State")
               | new AudioInput(upload, "Start recording", "Recording audio...", disabled: true).Small()
               | new AudioInput(upload, "Start recording", "Recording audio...", disabled: true)
               | new AudioInput(upload, "Start recording", "Recording audio...", disabled: true).Large();
    }
}

public class AudioInputBasic : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var audioFile = UseState<FileUpload<byte[]>?>();

        // Use MemoryStreamUploadHandler for basic upload
        var upload = UseUpload(
            MemoryStreamUploadHandler.Create(audioFile),
            defaultContentType: "audio/webm"
        );

        // Show toast when upload completes
        UseEffect(() =>
        {
            if (audioFile.Value?.Status == FileUploadStatus.Finished)
            {
                client.Toast($"Recording uploaded: {StringHelper.FormatBytes(audioFile.Value.Length)}", "Upload Complete");
            }
        }, audioFile);

        return Layout.Vertical().Gap(4)
               | Text.P("Basic AudioInput example. Records audio and uploads the complete recording when you stop.")
               | new AudioInput(upload.Value, "Start recording", "Recording audio...")
               | (audioFile.Value != null
                   ? Text.P($"Last upload: {StringHelper.FormatBytes(audioFile.Value.Length)}").Small()
                   : Text.P("No recordings uploaded yet").Small());
    }
}

public class AudioInputChunkedUpload : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var audioFile = UseState<FileUpload<byte[]>?>();
        var chunkCount = UseState(0);

        // Use ChunkedMemoryStreamUploadHandler to accumulate chunks into a single file
        var upload = UseUpload(
            ChunkedMemoryStreamUploadHandler.Create(audioFile),
            defaultContentType: "audio/webm"
        );

        // Track when chunks arrive
        UseEffect(() =>
        {
            if (audioFile.Value?.Length > 0)
            {
                var newCount = chunkCount.Value + 1;
                chunkCount.Set(newCount);
                client.Toast($"Chunk {newCount}: Total size {StringHelper.FormatBytes(audioFile.Value.Length)}", "Audio Chunk Received");
            }
        }, audioFile);

        return Layout.Vertical().Gap(4)
               | Text.P("Records audio and uploads in 2-second chunks while recording. Each chunk is accumulated into a single file.")
               | new AudioInput(upload.Value, "Start chunked recording", "Recording (uploading every 2s)...")
                   .ChunkInterval(2000)
               | Text.P($"Chunks received: {chunkCount.Value}").Small()
               | (audioFile.Value != null
                   ? Text.P($"Total accumulated: {StringHelper.FormatBytes(audioFile.Value.Length)}").Small()
                   : null);
    }
}

public class AudioInputDisabledState : ViewBase
{
    public override object? Build()
    {
        // Create a dummy upload for display-only example
        var dummyUpload = UseUpload(
            (fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask,
            defaultContentType: "audio/webm"
        );

        return Layout.Vertical().Gap(4)
               | Text.P("Demonstrates the AudioInput widget in a disabled state. The recorder is non-interactive and cannot be used for recording.")
               | new AudioInput(dummyUpload.Value, "Start recording", "Recording audio...", disabled: true);
    }
}
