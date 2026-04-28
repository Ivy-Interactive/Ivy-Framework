# Video

A content area to display a video. Supports YouTube, Vimeo, and direct links to browser-supported video file types with playback controls.

## Retool

```toolscript
video1.src = "https://www.youtube.com/watch?v=dQw4w9WgXcQ"
video1.loop = true
video1.hideControls = false
video1.play()
video1.pause()
video1.seekTo(5000)
```

## Ivy

Ivy provides two approaches: `VideoPlayer` for direct video files (MP4, WebM), and `Embed` for YouTube/Vimeo URLs with automatic platform detection.

```csharp
// Direct video file playback
new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
    .Autoplay(true)
    .Controls(true)
    .Loop(true)
    .Muted(true)
    .Poster("https://example.com/thumbnail.jpg")
    .Width(Size.Fraction(0.5f))
    .Height(Size.Units(50));

// YouTube/Vimeo via Embed widget (auto-detects platform)
new Embed("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
```

## Parameters

| Parameter          | Documentation                                     | Ivy                                                       |
|--------------------|----------------------------------------------------|------------------------------------------------------------|
| `src`              | Video source URL                                   | `VideoPlayer.Source` or `Embed.Url`                        |
| `loop`             | Loop video playback                                | `.Loop(bool)` modifier                                     |
| `hideControls`     | Hides playback controls                            | `.Controls(bool)` modifier (inverted logic)                |
| `currentTimestamp`  | Playback position (read-only)                     | Not supported                                              |
| `duration`         | Media duration in seconds (read-only)              | Not supported                                              |
| `playbackRate`     | Playback speed multiplier (read-only)              | Not supported                                              |
| `playing`          | Current playback state (read-only)                 | Not supported                                              |
| `volume`           | Volume level 0-1 (read-only)                       | Not supported                                              |
| `hidden`           | Hides component from view                          | `Visible` property                                         |
| `margin`           | Outer spacing                                      | Not supported (use layout spacing)                         |
| `isHiddenOnMobile` | Hide on mobile layout                              | Not supported                                              |
| `isHiddenOnDesktop`| Hide on desktop layout                             | Not supported                                              |
| `play()`           | Start playback                                     | `.Autoplay(true)` (no runtime method)                      |
| `pause()`          | Pause playback                                     | Not supported                                              |
| `seekTo(time)`     | Jump to time in milliseconds                       | Not supported                                              |
| Events: Play       | Playback started                                   | Not supported                                              |
| Events: Pause      | Playback paused                                    | Not supported                                              |
| Events: Ended      | Video finished                                     | Not supported                                              |
| Events: Load       | Video loaded                                       | Not supported                                              |
| N/A                | N/A                                                | `.Autoplay(bool)` modifier                                 |
| N/A                | N/A                                                | `.Muted(bool)` modifier                                    |
| N/A                | N/A                                                | `.Poster(string)` for thumbnail image                      |
| N/A                | N/A                                                | `Embed` auto-detects YouTube/Vimeo/social platforms        |
