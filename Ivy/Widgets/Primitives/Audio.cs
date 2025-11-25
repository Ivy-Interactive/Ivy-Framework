using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum AudioPreload
{
    None,
    Metadata,
    Auto
}

public record Audio : WidgetBase<Audio>
{
    public Audio(string src)
    {
        Src = src;
        Width = Size.Full();
        Height = Size.Units(10);
    }

    [Prop] public string Src { get; set; }

    [Prop] public bool Autoplay { get; set; } = false;

    [Prop] public bool Loop { get; set; } = false;

    [Prop] public bool Muted { get; set; } = false;

    [Prop] public AudioPreload Preload { get; set; } = AudioPreload.Metadata;

    [Prop] public bool Controls { get; set; } = true;
}

public static class AudioExtensions
{
    public static Audio Src(this Audio audio, string src) => audio with { Src = src };

    public static Audio Autoplay(this Audio audio, bool autoplay = true) => audio with { Autoplay = autoplay };

    public static Audio Loop(this Audio audio, bool loop = true) => audio with { Loop = loop };

    public static Audio Muted(this Audio audio, bool muted = true) => audio with { Muted = muted };

    public static Audio Preload(this Audio audio, AudioPreload preload) => audio with { Preload = preload };

    public static Audio Controls(this Audio audio, bool controls = true) => audio with { Controls = controls };
}
