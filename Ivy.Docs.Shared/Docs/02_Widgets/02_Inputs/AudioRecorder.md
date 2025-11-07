---
searchHints:
  - microphone
  - recording
  - voice
  - audio
  - capture
  - sound
---

# Audio Recorder

<Ingress>
Enable audio recording with a flexible interface for capturing user audio input with automatic upload support.
</Ingress>

The `AudioRecorder` widget allows users to record audio using their microphone. It provides an audio recording interface with options for audio formats, automatic uploads, and chunked streaming. This widget is for recording audio, not playing it.

## Basic Usage

Here's a simple example of an `AudioRecorder` that uploads audio to the server and stores it in state:

```csharp demo-below
public class BasicAudioRecorderDemo : ViewBase
{
    public override object? Build()
    {
        var audioFile = UseState<FileUpload<byte[]>?>();
        var upload = this.UseUpload(
            MemoryStreamUploadHandler.Create(audioFile),
            defaultContentType: "audio/webm"
        );

        return Layout.Vertical()
               | new AudioRecorder(upload.Value, "Start recording", "Recording audio...")
               | (audioFile.Value != null
                   ? Text.P($"Recorded: {audioFile.Value.FileName} ({Utils.FormatBytes(audioFile.Value.Length)})")
                   : null);
   }
}
```

### Chunked Upload (Streaming)

Upload audio in chunks while recording. Use `ChunkedMemoryStreamUploadHandler` to accumulate chunks into a single file:

```csharp demo-below
public class ChunkedUploadDemo : ViewBase
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
```

<Callout Type="tip">
Use `MemoryStreamUploadHandler` for complete file uploads (uploads when recording stops) and `ChunkedMemoryStreamUploadHandler` for streaming uploads (uploads chunks during recording).
</Callout>

## Audio Format

Specify the audio format using MIME type:

```csharp demo-below
public class AudioFormatDemo : ViewBase
{
    public override object? Build()
    {
        var audioFile = UseState<FileUpload<byte[]>?>();

        // Use webm format (most compatible)
        var upload = this.UseUpload(
            MemoryStreamUploadHandler.Create(audioFile),
            defaultContentType: "audio/webm"
        );

        return Layout.Vertical()
               | new AudioRecorder(upload.Value, "Record WebM", "Recording WebM...")
                   .MimeType("audio/webm")
               | (audioFile.Value != null
                   ? Text.Small($"Format: {audioFile.Value.ContentType}, Size: {Utils.FormatBytes(audioFile.Value.Length)}")
                   : null);
    }
}
```

<Callout Type="tip">
Use `audio/webm` for best browser compatibility. Other formats like `audio/mp4` or `audio/wav` may work depending on the browser.
</Callout>

## Styling

### Custom Labels

Customize the labels shown when idle and recording:

```csharp demo-below
public class CustomLabelsDemo : ViewBase
{
    public override object? Build()
    {
        var audioFile = UseState<FileUpload<byte[]>?>();
        var upload = this.UseUpload(
            MemoryStreamUploadHandler.Create(audioFile),
            defaultContentType: "audio/webm"
        );

        return Layout.Vertical()
               | new AudioRecorder(upload.Value)
                   .Label("Click to start voice memo")
                   .RecordingLabel("Recording your voice...")
               | (audioFile.Value != null
                   ? audioFile.Value.ToDetails()
                   : null);
    }
}
```

### Disabled State

Disable the audio recorder:

```csharp demo-below
public class AudioRecorderDisabledDemo : ViewBase
{
    public override object? Build()
    {
        var audioFile = UseState<FileUpload<byte[]>?>();
        var upload = this.UseUpload(
            MemoryStreamUploadHandler.Create(audioFile),
            defaultContentType: "audio/webm"
        );

        return new AudioRecorder(upload.Value, "Recording disabled", disabled: true);
    }
}
```

<WidgetDocs Type="Ivy.AudioRecorder" ExtensionTypes="Ivy.AudioRecorderExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/AudioRecorder.cs"/>
