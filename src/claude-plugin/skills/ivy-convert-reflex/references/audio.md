# Audio

Displays an audio player that can play audio files from local assets or external URLs, with browser-native controls for playback.

## Reflex

```python
rx.audio(
    url="/test.mp3",
    controls=True,
    playing=False,
    loop=False,
    muted=False,
    volume=0.8,
)
```

## Ivy

```csharp
new AudioPlayer("https://example.com/test.mp3")
    .Controls(true)
    .Loop(false)
    .Muted(false)
    .Autoplay(false)
    .Preload(AudioPreload.Auto)
```

## Parameters

| Parameter          | Documentation                                    | Ivy                                            |
|--------------------|--------------------------------------------------|-------------------------------------------------|
| `src` / `url`      | The audio source path (local or external URL)    | `Src` (passed via constructor)                  |
| `controls`         | Show/hide the player controls                    | `Controls`                                      |
| `loop`             | Repeat audio when finished                       | `Loop`                                          |
| `muted`            | Mute audio output                                | `Muted`                                         |
| `playing`          | Programmatically control play/pause state        | Not supported (use `Autoplay` for initial play) |
| `volume`           | Set playback volume level (0-1)                  | Not supported                                   |
| `light`            | Light mode appearance for the player             | Not supported                                   |
| `config`           | Additional configuration dictionary              | Not supported                                   |
| `disable_remote_playback` | Disable remote playback functionality     | Not supported                                   |
| `autoplay`         | Not a direct prop (use `playing`)                | `Autoplay`                                      |
| `preload`          | Not supported                                    | `Preload` (None / Metadata / Auto)              |
| `width` / `height` | Inherited from base component                    | `Width` / `Height`                              |
