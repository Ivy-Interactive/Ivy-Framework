using Ivy.Hooks;
using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;


[App(icon: Icons.Mic, path: ["Widgets", "Inputs"], searchHints: ["microphone", "recording", "voice", "audio", "capture", "sound"])]

public class AudioRecorderApp() : SampleBase
{
    protected override object? BuildSample()
    {
        // Create a dummy upload for display-only examples
        var dummyUpload = this.UseUpload(
            (fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask,
            defaultContentType: "audio/webm"
        );

        return Layout.Vertical()
               | Text.H1("Audio Recorder Widget Examples")
               | Text.P("Demonstrates the AudioRecorder widget for capturing audio input. This widget is for recording audio, not playing it. The recorder interface is theme-aware and adapts to light/dark themes.")
               | Text.H2("Upload Examples")
               | (Layout.Horizontal().Gap(4)
                    | new Card(new AudioRecorderBasic()).Title("Basic")
                    | new Card(new AudioRecorderChunkedUpload()).Title("Chunked Upload")
                    | new Card(new AudioRecorderDisabledState()).Title("Disabled State"))
               | Text.H2("Sizes")
               | CreateSizesSection(dummyUpload.Value);
    }

    private object CreateSizesSection(UploadContext upload)
    {
        return Layout.Grid().Columns(4)
               | Text.InlineCode("Description")
               | Text.InlineCode("Small")
               | Text.InlineCode("Medium")
               | Text.InlineCode("Large")

               | Text.InlineCode("Audio Recorder")
               | new AudioRecorder(upload, "Start recording", "Recording audio...").Small()
               | new AudioRecorder(upload, "Start recording", "Recording audio...")
               | new AudioRecorder(upload, "Start recording", "Recording audio...").Large()

               | Text.InlineCode("Disabled State")
               | new AudioRecorder(upload, "Start recording", "Recording audio...", disabled: true).Small()
               | new AudioRecorder(upload, "Start recording", "Recording audio...", disabled: true)
               | new AudioRecorder(upload, "Start recording", "Recording audio...", disabled: true).Large();
    }
}

public class AudioRecorderBasic : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var audioFile = UseState<FileUpload<byte[]>?>();

        // Use MemoryStreamUploadHandler for basic upload
        var upload = this.UseUpload(
            MemoryStreamUploadHandler.Create(audioFile),
            defaultContentType: "audio/webm"
        );

        // Show toast when upload completes
        UseEffect(() =>
        {
            if (audioFile.Value?.Status == FileUploadStatus.Finished)
            {
                client.Toast($"Recording uploaded: {Utils.FormatBytes(audioFile.Value.Length)}", "Upload Complete");
            }
        }, audioFile);

        return Layout.Vertical().Gap(4)
               | Text.P("Basic AudioRecorder example. Records audio and uploads the complete recording when you stop.")
               | new AudioRecorder(upload.Value, "Start recording", "Recording audio...")
               | (audioFile.Value != null
                   ? Text.Small($"Last upload: {Utils.FormatBytes(audioFile.Value.Length)}")
                   : Text.Small("No recordings uploaded yet"));
    }
}

public class AudioRecorderChunkedUpload : ViewBase
{
    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var audioFile = UseState<FileUpload<byte[]>?>();
        var chunkCount = UseState(0);

        // Use ChunkedMemoryStreamUploadHandler to accumulate chunks into a single file
        var upload = this.UseUpload(
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
                client.Toast($"Chunk {newCount}: Total size {Utils.FormatBytes(audioFile.Value.Length)}", "Audio Chunk Received");
            }
        }, audioFile);

        return Layout.Vertical().Gap(4)
               | Text.P("Records audio and uploads in 2-second chunks while recording. Each chunk is accumulated into a single file.")
               | new AudioRecorder(upload.Value, "Start chunked recording", "Recording (uploading every 2s)...")
                   .ChunkInterval(2000)
               | Text.Small($"Chunks received: {chunkCount.Value}")
               | (audioFile.Value != null
                   ? Text.Small($"Total accumulated: {Utils.FormatBytes(audioFile.Value.Length)}")
                   : null);
    }
}

public class AudioRecorderDisabledState : ViewBase
{
    public override object? Build()
    {
        // Create a dummy upload for display-only example
        var dummyUpload = this.UseUpload(
            (fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask,
            defaultContentType: "audio/webm"
        );

        return Layout.Vertical().Gap(4)
               | Text.P("Demonstrates the AudioRecorder widget in a disabled state. The recorder is non-interactive and cannot be used for recording.")
               | new AudioRecorder(dummyUpload.Value, "Start recording", "Recording audio...", disabled: true);
    }
}
