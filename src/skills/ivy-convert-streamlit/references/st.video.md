# st.video

Display a video player. Supports URLs (including YouTube), local file paths, and raw video data. The player includes native browser controls for playback.

## Streamlit

```python
import streamlit as st

video_file = open("myvideo.mp4", "rb")
video_bytes = video_file.read()

st.video(video_bytes)
```

## Ivy

```csharp
new VideoPlayer("https://example.com/video.mp4")
    .Autoplay(true)
    .Muted(true)
    .Loop(true)
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| data | Video source: URL, local file path, or raw video data (str, Path, bytes, io.BytesIO, numpy.ndarray) | `Source` (string URL only) |
| format | MIME type for the video file, default `"video/mp4"` | Not supported |
| start_time | Playback start point in seconds, timedelta, or string (e.g. `"20s"`), default `0` | Not supported |
| subtitles | Subtitle data in `.vtt` or `.srt` format; dict for multiple tracks with labels | Not supported |
| end_time | Playback stop point, same formats as `start_time` | Not supported |
| loop | Enable video looping, default `False` | `Loop` (bool) |
| autoplay | Auto-start playback (requires `muted=True` for browsers), default `False` | `Autoplay` (bool) |
| muted | Silence audio during playback, default `False` | `Muted` (bool) |
| — | — | `Controls` (bool) — show/hide browser playback controls |
| — | — | `Poster` (string) — preview image URL before playback |
