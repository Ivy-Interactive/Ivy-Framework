
namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;


[App(icon: Icons.Mic, group: ["Widgets", "Inputs"], searchHints: ["microphone", "recording", "voice", "audio", "capture", "sound"])]

public class AudioInputApp() : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Tabs(
            new Tab("Upload", new AudioInputUploadSample()),
            new Tab("Validation", new AudioInputValidationSample()),
            new Tab("Sizes", new AudioInputSizesSample()),
            new Tab("Events", new AudioInputEventsSample())
        ).Variant(TabsVariant.Content);
    }
}

public class AudioInputUploadSample : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            | Text.P("Demonstrates the AudioInput widget for capturing audio input. This widget is for recording audio, not playing it. The recorder interface is theme-aware and adapts to light/dark themes.")
            | (Layout.Horizontal()
                | new Card(new AudioInputBasic()).Title("Basic")
                | new Card(new AudioInputChunkedUpload()).Title("Chunked Upload")
                | new Card(new AudioInputDisabledState()).Title("Disabled State")
                | new Card(new AudioInputSampleRate(24000)).Title("24 kHz (speech)"));
    }
}

public class AudioInputValidationSample : ViewBase
{
    public override object? Build()
    {
        var dummyUpload = UseUpload(
            (fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask,
            defaultContentType: "audio/webm"
        );

        return Layout.Vertical()
            | Text.P("Demonstrates validation states on the AudioInput widget.")
            | new AudioInput(dummyUpload.Value, "Invalid Audio Input", "Recording...")
                .Invalid("Audio input is invalid");
    }
}

public class AudioInputSizesSample : ViewBase
{
    public override object? Build()
    {
        var dummyUpload = UseUpload(
            (fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask,
            defaultContentType: "audio/webm"
        );

        return Layout.Grid().Columns(4)
            | Text.Monospaced("Description")
            | Text.Monospaced("Small")
            | Text.Monospaced("Medium")
            | Text.Monospaced("Large")
            | Text.Monospaced("Audio Input")
            | new AudioInput(dummyUpload.Value, "Start recording", "Recording audio...").Small()
            | new AudioInput(dummyUpload.Value, "Start recording", "Recording audio...")
            | new AudioInput(dummyUpload.Value, "Start recording", "Recording audio...").Large()
            | Text.Monospaced("Disabled State")
            | new AudioInput(dummyUpload.Value, "Start recording", "Recording audio...", disabled: true).Small()
            | new AudioInput(dummyUpload.Value, "Start recording", "Recording audio...", disabled: true)
            | new AudioInput(dummyUpload.Value, "Start recording", "Recording audio...", disabled: true).Large();
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

        return Layout.Vertical()
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

        return Layout.Vertical()
               | Text.P("Records audio and uploads in 2-second chunks while recording. Each chunk is accumulated into a single file.")
               | new AudioInput(upload.Value, "Start chunked recording", "Recording (uploading every 2s)...")
                   .ChunkInterval(2000)
               | Text.P($"Chunks received: {chunkCount.Value}").Small()
               | (audioFile.Value != null
                   ? Text.P($"Total accumulated: {StringHelper.FormatBytes(audioFile.Value.Length)}").Small()
                   : null);
    }
}

public class AudioInputSampleRate : ViewBase
{
    private readonly int? _sampleRate;

    public AudioInputSampleRate(int? sampleRate)
    {
        _sampleRate = sampleRate;
    }

    public override object? Build()
    {
        var audioFile = UseState<FileUpload<byte[]>?>();
        var upload = UseUpload(
            MemoryStreamUploadHandler.Create(audioFile),
            defaultContentType: "audio/webm"
        );

        var label = _sampleRate.HasValue ? $"Record at {_sampleRate} Hz" : "Record (browser default)";
        var input = new AudioInput(upload.Value, label, "Recording...");
        if (_sampleRate.HasValue)
            input = input.SampleRate(_sampleRate.Value);

        return Layout.Vertical()
               | Text.P(_sampleRate.HasValue
                   ? $"Records at {_sampleRate} Hz (e.g. for speech or high-fidelity)."
                   : "Uses the browser's default sample rate (typically 48 kHz).")
               | input
               | (audioFile.Value != null
                   ? Text.P($"Uploaded: {StringHelper.FormatBytes(audioFile.Value.Length)}").Small()
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

        return Layout.Vertical()
               | Text.P("Demonstrates the AudioInput widget in a disabled state. The recorder is non-interactive and cannot be used for recording.")
               | new AudioInput(dummyUpload.Value, "Start recording", "Recording audio...", disabled: true);
    }
}
public class AudioInputEventsSample : ViewBase
{
    public override object? Build()
    {
        var upload = UseUpload(
            (fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask,
            defaultContentType: "audio/webm"
        );

        var onBlurLabel = UseState("");
        var onFocusLabel = UseState("");

        return Layout.Vertical()
            | new Card(
                Layout.Vertical().Gap(2)
                    | Text.P("The blur event fires when the audio input loses focus.").Small()
                    | new AudioInput(upload.Value, "OnBlur Test", "Recording...").OnBlur(e => onBlurLabel.Set("Blur Event Triggered"))
                    | (onBlurLabel.Value != ""
                        ? Callout.Success(onBlurLabel.Value)
                        : Callout.Info("Interact then click away to see blur events"))
            ).Title("OnBlur Handler")
            | new Card(
                Layout.Vertical().Gap(2)
                    | Text.P("The focus event fires when you click on or tab into the audio input.").Small()
                    | new AudioInput(upload.Value, "OnFocus Test", "Recording...").OnFocus(e => onFocusLabel.Set("Focus Event Triggered"))
                    | (onFocusLabel.Value != ""
                        ? Callout.Success(onFocusLabel.Value)
                        : Callout.Info("Click or tab into the input to see focus events"))
            ).Title("OnFocus Handler");
    }
}
