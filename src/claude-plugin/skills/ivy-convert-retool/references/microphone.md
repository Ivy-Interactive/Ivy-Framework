# Microphone

A button to record audio. Recordings are stored in Base64-encoded WebM format, with playback featuring a progress bar and elapsed time display.

## Retool

```toolscript
// Reset the microphone component
microphone.reset();

// Access recorded audio data
const audioData = microphone.data;
const audioUrl = microphone.audioURL;

// Check recording state
if (microphone.recording) {
  console.log("Currently recording...");
}
```

## Ivy

```csharp
public class MicrophoneDemo : ViewBase
{
    public override object? Build()
    {
        var audioFile = UseState<FileUpload<byte[]>?>();
        var upload = UseUpload(
            MemoryStreamUploadHandler.Create(audioFile),
            defaultContentType: "audio/webm"
        );

        return Layout.Vertical()
               | new AudioInput(upload.Value, "Start recording", "Recording...")
               | (audioFile.Value != null
                   ? Text.P($"Recorded: {audioFile.Value.FileName}")
                   : null);
    }
}
```

> **Note:** Ivy's equivalent is the `AudioInput` widget. It supports chunked streaming uploads and configurable audio formats.

## Parameters

| Parameter       | Documentation                                  | Ivy                                         |
|-----------------|-------------------------------------------------|---------------------------------------------|
| `audioFile`     | Details of the recorded audio (read-only)       | State value `FileUpload<byte[]>`            |
| `data`          | Base64 encoded audio data (read-only)           | State value contains raw byte data          |
| `audioURL`      | Base64-encoded data URL (read-only)             | Not directly exposed (use state bytes)      |
| `recording`     | Whether audio is currently recording            | Not directly exposed                        |
| `disabled`      | Disables interaction                            | `disabled` constructor parameter            |
| `label`         | Text label to display                           | `label` constructor parameter               |
| `stopLabel`     | Label for stop button                           | `recordingLabel` constructor parameter      |
| `hidden`        | Hides from view                                 | `Visible` property                          |
| `margin`        | Outside margin spacing                          | Not supported (use layout)                  |
| N/A             | N/A                                             | `MimeType` — audio format (default `audio/webm`) |
| N/A             | N/A                                             | `ChunkInterval` — ms between chunk uploads  |
| N/A             | N/A                                             | `Height` / `Width` — dimensions             |
