# st.audio

Display an audio player. Supports URLs, local file paths, raw bytes, and NumPy arrays as audio sources.

## Streamlit

```python
import streamlit as st
import numpy as np

# From a file
st.audio("cat-purr.mp3", format="audio/mpeg", loop=True)

# From bytes
audio_file = open("myaudio.ogg", "rb")
audio_bytes = audio_file.read()
st.audio(audio_bytes, format="audio/ogg")

# From a NumPy array (440 Hz sine wave)
sample_rate = 44100
seconds = 2
t = np.linspace(0, seconds, seconds * sample_rate, False)
note_la = np.sin(440 * t * 2 * np.pi)
st.audio(note_la, sample_rate=sample_rate)
```

## Ivy

```csharp
new AudioPlayer("https://example.com/cat-purr.mp3")
    .Loop(true)
    .Autoplay(true)
    .Muted(true)
    .Preload(AudioPreload.Auto)
```

## Parameters

| Parameter | Streamlit | Ivy |
|-----------|-----------|-----|
| data / src | `data` - URL string, file path, raw bytes, BytesIO, or NumPy array | `src` (constructor) - URL string only |
| format | `format` (str, default `"audio/wav"`) - MIME type for the audio file | Not supported |
| start_time | `start_time` (int/float/timedelta/str, default `0`) - Playback start position in seconds | Not supported |
| end_time | `end_time` (int/float/timedelta/str, default `None`) - Playback end position | Not supported |
| sample_rate | `sample_rate` (int, default `None`) - Samples per second for NumPy array input | Not supported |
| loop | `loop` (bool, default `False`) - Repeat audio after finishing | `.Loop(bool)` |
| autoplay | `autoplay` (bool, default `False`) - Start playback automatically | `.Autoplay(bool)` |
| muted | Not supported | `.Muted(bool)` - Mute audio initially |
| controls | Not supported | `.Controls(bool)` - Show/hide player controls |
| preload | Not supported | `.Preload(AudioPreload)` - Loading strategy: `None`, `Metadata`, or `Auto` |
