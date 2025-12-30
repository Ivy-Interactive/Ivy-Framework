---
searchHints:
  - sound
  - playback
  - media
  - mp3
  - music
  - audio
---

# Audio Player

<Ingress>
Play audio content with browser controls. Supports common audio formats and provides customizable playback options.
</Ingress>

The `AudioPlayer` [widget](../../../01_Onboarding/02_Concepts/Widgets.md) displays an audio player with browser controls in your [app](../../../01_Onboarding/02_Concepts/Apps.md). This widget is for playing audio files, not recording them.

## Basic Usage

Create a simple audio player:

```csharp demo-below
new AudioPlayer("https://www.learningcontainer.com/wp-content/uploads/2020/02/Kalimba.mp3")
```

## Configuration Options

### Autoplay and Looping

Configure automatic playback and looping:

```csharp demo-tabs
Layout.Vertical().Gap(4)
| Text.H4("Muted Autoplay (browsers allow this)")
| new AudioPlayer("https://www.learningcontainer.com/wp-content/uploads/2020/02/Kalimba.mp3")
    .Autoplay(true)
    .Muted(true)
| Text.H4("Looping Audio")
| new AudioPlayer("https://www.learningcontainer.com/wp-content/uploads/2020/02/Kalimba.mp3")
    .Loop(true)
```

### Preload Strategy

Control how much audio data is loaded:

```csharp demo-tabs
Layout.Vertical().Gap(4)
| Text.Small("Preload: None (no data loaded)")
| new AudioPlayer("https://www.learningcontainer.com/wp-content/uploads/2020/02/Kalimba.mp3")
    .Preload(AudioPreload.None)
| Text.Small("Preload: Metadata (duration and basic info)")
| new AudioPlayer("https://www.learningcontainer.com/wp-content/uploads/2020/02/Kalimba.mp3")
    .Preload(AudioPreload.Metadata)
| Text.Small("Preload: Auto (entire file)")
| new AudioPlayer("https://www.learningcontainer.com/wp-content/uploads/2020/02/Kalimba.mp3")
    .Preload(AudioPreload.Auto)
```

<WidgetDocs Type="Ivy.AudioPlayer" ExtensionTypes="Ivy.AudioPlayerExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/AudioPlayer.cs"/>
