using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Defines the preload strategy for audio content.</summary>
public enum AudioPreload
{
    /// <summary>No audio data is preloaded.</summary>
    None,
    /// <summary>Only audio metadata (duration, dimensions) is loaded.</summary>
    Metadata,
    /// <summary>The entire audio file is preloaded.</summary>
    Auto
}

/// <summary>A widget for playing audio content with browser controls. Supports common audio formats (MP3, WAV, OGG, AAC, M4A).</summary>
public record Audio : WidgetBase<Audio>
{
    /// <summary>Initializes a new audio widget with the specified source URL.</summary>
    /// <param name="src">The URL or path to the audio source.</param>
    public Audio(string src)
    {
        Src = src;
        Width = Size.Full();
        Height = Size.Units(10); // Default height for audio controls
    }

    /// <summary>Gets or sets the source URL or path for the audio file to play.</summary>
    [Prop] public string Src { get; set; }

    /// <summary>Gets or sets whether the audio should start playing automatically when loaded. Default is false.</summary>
    [Prop] public bool Autoplay { get; set; } = false;

    /// <summary>Gets or sets whether the audio should loop continuously when it reaches the end. Default is false.</summary>
    [Prop] public bool Loop { get; set; } = false;

    /// <summary>Gets or sets whether the audio should be muted by default. Default is false.</summary>
    [Prop] public bool Muted { get; set; } = false;

    /// <summary>Gets or sets the preload strategy. Default is Metadata.</summary>
    [Prop] public AudioPreload Preload { get; set; } = AudioPreload.Metadata;

    /// <summary>Gets or sets whether the browser's default audio controls should be displayed. Default is true.</summary>
    [Prop] public bool Controls { get; set; } = true;
}

/// <summary>Extension methods for configuring Audio widget properties.</summary>
public static class AudioExtensions
{
    /// <summary>Sets the source URL for the audio file.</summary>
    public static Audio Src(this Audio audio, string src)
    {
        return audio with { Src = src };
    }

    /// <summary>Enables or disables autoplay for the audio.</summary>
    public static Audio Autoplay(this Audio audio, bool autoplay = true)
    {
        return audio with { Autoplay = autoplay };
    }

    /// <summary>Enables or disables looping for the audio.</summary>
    public static Audio Loop(this Audio audio, bool loop = true)
    {
        return audio with { Loop = loop };
    }

    /// <summary>Sets the muted state of the audio.</summary>
    public static Audio Muted(this Audio audio, bool muted = true)
    {
        return audio with { Muted = muted };
    }

    /// <summary>Sets the preload strategy for the audio.</summary>
    public static Audio Preload(this Audio audio, AudioPreload preload)
    {
        return audio with { Preload = preload };
    }

    /// <summary>Enables or disables the browser's default audio controls.</summary>
    public static Audio Controls(this Audio audio, bool controls = true)
    {
        return audio with { Controls = controls };
    }
}
