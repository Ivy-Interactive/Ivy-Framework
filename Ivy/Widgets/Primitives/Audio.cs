using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>Audio preload strategy.</summary>
public enum AudioPreload
{
    /// <summary>No preload.</summary>
    None,
    /// <summary>Metadata only.</summary>
    Metadata,
    /// <summary>Full audio file.</summary>
    Auto
}

/// <summary>Audio player widget with browser controls. Supports MP3, WAV, OGG, AAC, M4A. Default size: full width, 10 units height.</summary>
public record Audio : WidgetBase<Audio>
{
    /// <summary>Initializes audio widget.</summary>
    /// <param name="src">Audio source URL/path.</param>
    public Audio(string src)
    {
        Src = src;
        Width = Size.Full();
        Height = Size.Units(10);
    }

    /// <summary>Audio source URL/path.</summary>
    [Prop] public string Src { get; set; }

    /// <summary>Autoplay. Default: false.</summary>
    [Prop] public bool Autoplay { get; set; } = false;

    /// <summary>Loop playback. Default: false.</summary>
    [Prop] public bool Loop { get; set; } = false;

    /// <summary>Muted. Default: false.</summary>
    [Prop] public bool Muted { get; set; } = false;

    /// <summary>Preload strategy. Default: Metadata.</summary>
    [Prop] public AudioPreload Preload { get; set; } = AudioPreload.Metadata;

    /// <summary>Show controls. Default: true.</summary>
    [Prop] public bool Controls { get; set; } = true;
}

/// <summary>Extension methods for Audio widget configuration.</summary>
public static class AudioExtensions
{
    /// <summary>Sets audio source.</summary>
    public static Audio Src(this Audio audio, string src) => audio with { Src = src };

    /// <summary>Sets autoplay.</summary>
    public static Audio Autoplay(this Audio audio, bool autoplay = true) => audio with { Autoplay = autoplay };

    /// <summary>Sets loop.</summary>
    public static Audio Loop(this Audio audio, bool loop = true) => audio with { Loop = loop };

    /// <summary>Sets muted.</summary>
    public static Audio Muted(this Audio audio, bool muted = true) => audio with { Muted = muted };

    /// <summary>Sets preload strategy.</summary>
    public static Audio Preload(this Audio audio, AudioPreload preload) => audio with { Preload = preload };

    /// <summary>Sets controls visibility.</summary>
    public static Audio Controls(this Audio audio, bool controls = true) => audio with { Controls = controls };
}
