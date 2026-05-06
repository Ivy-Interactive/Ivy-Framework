# Video

Displays video content from local assets or external sources with playback controls.

## Reflex

```python
rx.video(url="/test.mp4", controls=True, loop=True, muted=True)
```

## Ivy

```csharp
new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
    .Controls(true)
    .Loop(true)
    .Muted(true)
```

## Parameters

| Parameter                | Documentation                                      | Ivy                        |
|--------------------------|----------------------------------------------------|----------------------------|
| `url` / `src`            | Video source path or URL                           | `Source`                   |
| `playing`                | Controls playback state                            | `Autoplay`                 |
| `loop`                   | Repeats video when finished                        | `Loop`                     |
| `controls`               | Shows player controls (default true)               | `Controls`                 |
| `light`                  | Enables light mode preview (thumbnail)             | `Poster`                   |
| `volume`                 | Sets playback volume (float)                       | Not supported              |
| `muted`                  | Mutes audio                                        | `Muted`                    |
| `config`                 | Player configuration dictionary                    | Not supported              |
| `disable_remote_playback`| Disables remote playback                           | Not supported              |
| `width`                  | Player width                                       | `Width`                    |
| `height`                 | Player height                                      | `Height`                   |
| N/A                      | Preview image shown before playback                | `Poster`                   |
| N/A                      | Scaling configuration                              | `Scale`                    |
| N/A                      | Visibility toggle                                  | `Visible`                  |
