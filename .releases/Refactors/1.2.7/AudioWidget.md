# Refactor Audio Widget to AudioPlayer

The `Audio` widget has been renamed to `AudioPlayer` for better clarity and consistency.

## Goal

Find all instances where the `Audio` widget is used and replace it with `AudioPlayer`.

## Locate Code

Look for C# files (mostly in Views or Components) that instantiate the `Audio` widget.

**Search Query:** `new Audio`

## Required Changes

### Rename Widget

From:
```csharp
new Audio("https://example.com/sound.mp3")
```

To:
```csharp
new AudioPlayer("https://example.com/sound.mp3")
```

From:
```csharp
public Audio MyAudioWidget { get; set; }
```

To:
```csharp
public AudioPlayer MyAudioWidget { get; set; }
```
