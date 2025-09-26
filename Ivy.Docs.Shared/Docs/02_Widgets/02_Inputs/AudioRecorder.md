# Audio Recorder

<Ingress>
Enable audio recording with a flexible interface for capturing user audio input.
</Ingress>

The `AudioRecorder` widget allows users to record audio using their microphone. It provides an audio recording interface with options for audio formats, audio upload endpoint, and upload chunking. This widget is for recording audio, not playing it.

## Basic Usage

Here's a simple example of a `AudioRecorder` that allows users to record audio:

```csharp demo-below
public class BasicAudioRecorderDemo : ViewBase
{
    public override object? Build()
    {
        var uploadUrl = this.UseUpload(
            fileBytes => {
                // Process uploaded file bytes
                Console.WriteLine($"Received {fileBytes.Length} bytes");
            },
            "application/octet-stream",
            "uploaded-audio"
        );

        return new AudioRecorder("Start recording", "Recording audio..").UploadUrl(uploadUrl.Value).ChunkInterval(3000);
   }     
}    
```

## Styling

`AudioRecorder` can be customized with various styling options:

### Disabled

To render a disabled `AudioRecorder` control, the `Disabled` function should be used.

```csharp demo-below
public class AudioRecorderDisabledDemo : ViewBase
{
    public override object? Build()
    {
        return new AudioRecorder("Disabled audio recorder", disabled: true);
    }
}    
```

<WidgetDocs Type="Ivy.AudioRecorder" ExtensionTypes="Ivy.AudioRecorderExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/AudioRecorder.cs"/>