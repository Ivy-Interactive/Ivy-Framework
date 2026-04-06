// ReSharper disable once CheckNamespace
namespace Ivy;

public enum AudioPreload
{
    None,
    Metadata,
    Auto
}

/// <summary>
/// Plays audio files.
/// </summary>
public record AudioPlayer : WidgetBase<AudioPlayer>
{
    public AudioPlayer(string src) : this()
    {
        Src = src;
    }

    internal AudioPlayer()
    {
        Width = Size.Full();
        Height = Size.Units(10);
    }

    [Prop] public string Src { get; set; } = string.Empty;

    [Prop] public bool Autoplay { get; set; } = false;

    [Prop] public bool Loop { get; set; } = false;

    [Prop] public bool Muted { get; set; } = false;

    [Prop] public AudioPreload Preload { get; set; } = AudioPreload.Metadata;

    [Prop] public bool Controls { get; set; } = true;
}

public static class AudioExtensions
{
    public static AudioPlayer Src(this AudioPlayer audio, string src) => audio with { Src = src };

    public static AudioPlayer Autoplay(this AudioPlayer audio, bool autoplay = true) => audio with { Autoplay = autoplay };

    public static AudioPlayer Loop(this AudioPlayer audio, bool loop = true) => audio with { Loop = loop };

    public static AudioPlayer Muted(this AudioPlayer audio, bool muted = true) => audio with { Muted = muted };

    public static AudioPlayer Preload(this AudioPlayer audio, AudioPreload preload) => audio with { Preload = preload };

    public static AudioPlayer Controls(this AudioPlayer audio, bool controls = true) => audio with { Controls = controls };
}
