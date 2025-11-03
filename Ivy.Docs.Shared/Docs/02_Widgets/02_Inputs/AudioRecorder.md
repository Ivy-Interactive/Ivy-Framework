---
searchHints:
  - microphone
  - recording
  - voice
  - audio
  - capture
  - sound
imports:
  - Ivy.Services
---

# Audio Recorder

<Ingress>
Enable audio recording with a flexible interface for capturing user audio input with automatic upload support.
</Ingress>

The `AudioRecorder` widget allows users to record audio using their microphone. It provides an audio recording interface with options for audio formats, automatic uploads, and chunked streaming. This widget is for recording audio, not playing it.

## Basic Usage

Here's a simple example of an `AudioRecorder` that uploads audio to the server:

```csharp demo-below
public class BasicAudioRecorderDemo : ViewBase
{
    public override object? Build()
    {
        // Create an upload handler for audio chunks
        var upload = this.UseUpload(
            (fileUpload, stream, cancellationToken) => {
                // Process uploaded audio chunk
                Console.WriteLine($"Received {fileUpload.Length} bytes of audio");
                return Task.CompletedTask;
            },
            defaultContentType: "audio/webm" // Match the recorder's mime type
        );

        return new AudioRecorder(upload.Value, "Start recording", "Recording audio...")
                   .ChunkInterval(3000); // Upload every 3 seconds
   }
}
```

## Upload Modes

The audio recorder supports two upload modes:

### Chunked Upload (Streaming)

Upload audio in chunks while recording:

```csharp demo-below
public class ChunkedUploadDemo : ViewBase
{
    public override object? Build()
    {
        var upload = this.UseUpload(
            (fileUpload, stream, cancellationToken) => {
                // Each chunk arrives as recording continues
                Console.WriteLine($"Chunk received: {fileUpload.Length} bytes");
                return Task.CompletedTask;
            },
            defaultContentType: "audio/webm"
        );

        return new AudioRecorder(upload.Value, "Record with streaming", "Streaming...")
                   .ChunkInterval(1000); // Upload every 1 second
    }
}
```

### Single Upload

Upload the complete recording when stopped:

```csharp demo-below
public class SingleUploadDemo : ViewBase
{
    public override object? Build()
    {
        var upload = this.UseUpload(
            (fileUpload, stream, cancellationToken) => {
                // Complete recording arrives when user stops
                Console.WriteLine($"Recording complete: {fileUpload.Length} bytes");
                return Task.CompletedTask;
            },
            defaultContentType: "audio/webm"
        );

        // No ChunkInterval = upload when recording stops
        return new AudioRecorder(upload.Value, "Record", "Recording...");
    }
}
```

## Audio Format

Specify the audio format using MIME type:

```csharp demo-below
public class AudioFormatDemo : ViewBase
{
    public override object? Build()
    {
        // Use webm format (most compatible)
        var webmUpload = this.UseUpload(
            (fileUpload, stream, cancellationToken) => Task.CompletedTask,
            defaultContentType: "audio/webm"
        );

        return new AudioRecorder(webmUpload.Value, "Record WebM", "Recording WebM...")
                   .MimeType("audio/webm");
    }
}
```

<Callout Type="tip">
Use `audio/webm` for best browser compatibility. Other formats like `audio/mp4` or `audio/wav` may work depending on the browser.
</Callout>

## Styling

### Size Variants

Control the size of the audio recorder:

```csharp demo-below
public class SizeVariantsDemo : ViewBase
{
    public override object? Build()
    {
        var upload = this.UseUpload(
            (fileUpload, stream, cancellationToken) => Task.CompletedTask,
            defaultContentType: "audio/webm"
        );

        return Layout.Vertical()
                | Text.H2("Small")
                | new AudioRecorder(upload.Value, "Record", "Recording...").Small()
                | Text.H2("Medium (Default)")
                | new AudioRecorder(upload.Value, "Record", "Recording...")
                | Text.H2("Large")
                | new AudioRecorder(upload.Value, "Record", "Recording...").Large();
    }
}
```

### Custom Labels

Customize the labels shown when idle and recording:

```csharp demo-below
public class CustomLabelsDemo : ViewBase
{
    public override object? Build()
    {
        var upload = this.UseUpload(
            (fileUpload, stream, cancellationToken) => Task.CompletedTask,
            defaultContentType: "audio/webm"
        );

        return new AudioRecorder(upload.Value)
                   .Label("Click to start voice memo")
                   .RecordingLabel("Recording your voice...");
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
        var upload = this.UseUpload(
            (fileUpload, stream, cancellationToken) => Task.CompletedTask,
            defaultContentType: "audio/webm"
        );

        return new AudioRecorder(upload.Value, "Recording disabled", disabled: true);
    }
}
```

<WidgetDocs Type="Ivy.AudioRecorder" ExtensionTypes="Ivy.AudioRecorderExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/AudioRecorder.cs"/>
