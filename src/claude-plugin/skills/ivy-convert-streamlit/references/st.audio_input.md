# st.audio_input

Display a widget that records audio from the user's microphone and returns the recording.

## Streamlit

```python
import streamlit as st

audio_value = st.audio_input("Record a voice message")

if audio_value:
    st.audio(audio_value)
```

## Ivy

```csharp
public class BasicAudioInputDemo : ViewBase
{
    public override object? Build()
    {
        var audioFile = UseState<FileUpload<byte[]>?>();
        var upload = UseUpload(
            MemoryStreamUploadHandler.Create(audioFile),
            defaultContentType: "audio/webm"
        );

        return Layout.Vertical()
               | new AudioInput(upload.Value, "Start recording", "Recording audio...")
               | (audioFile.Value != null
                   ? Text.P($"Recorded: {audioFile.Value.FileName} ({Utils.FormatBytes(audioFile.Value.Length)})")
                   : null);
    }
}
```

## Parameters

| Parameter | Streamlit | Ivy |
|-----------|-----------|-----|
| label | `label` (str) - Brief label for the widget. Supports GitHub-flavored Markdown. | `Label` (string) - Idle state display text. |
| recording label | Not supported | `RecordingLabel` (string) - Text shown while actively recording. |
| sample rate | `sample_rate` (int or None, default 16000) - Target sample rate in Hz. Supported values: 8000, 11025, 16000, 22050, 24000, 32000, 44100, 48000, or None. | Not supported |
| audio format | Not supported (always returns audio/wav) | `MimeType` (string, default "audio/webm") - Audio format, e.g. "audio/webm", "audio/mp4", "audio/wav". |
| chunk interval | Not supported | `ChunkInterval` (int?) - Streaming chunk interval in milliseconds for chunked uploads. |
| help tooltip | `help` (str or None) - Tooltip displayed next to the label. Supports Markdown. | Not supported |
| on change | `on_change` (callable) - Callback invoked when the value changes. | Not supported (use `UseState` reactivity instead) |
| disabled | `disabled` (bool, default False) - Disables the widget. | `Disabled` (bool, default false) - Disables recording. |
| label visibility | `label_visibility` ("visible", "hidden", or "collapsed") - Controls label display. | Not supported |
