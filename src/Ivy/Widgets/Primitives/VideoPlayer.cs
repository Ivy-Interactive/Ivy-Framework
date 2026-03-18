using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Plays video files.
/// </summary>
public record VideoPlayer : WidgetBase<VideoPlayer>
{
    public VideoPlayer(
        string? source = null,
        bool autoplay = false,
        bool controls = true,
        bool muted = false,
        bool loop = false,
        string? poster = null,
        float? volume = null) : this()
    {
        Source = source;
        Autoplay = autoplay;
        Controls = controls;
        Muted = muted;
        Loop = loop;
        Poster = poster;
        Volume = volume.HasValue ? Math.Clamp(volume.Value, 0f, 1f) : null;
    }

    internal VideoPlayer()
    {
        Id = Guid.NewGuid().ToString();
    }

    [Prop] public string? Source { get; set; }

    [Prop] public bool Autoplay { get; set; }

    [Prop] public bool Controls { get; set; } = true;

    [Prop] public bool Muted { get; set; }

    [Prop] public bool Loop { get; set; }

    [Prop] public string? Poster { get; set; }

    [Prop] public float? Volume { get; set; }
}

public static class VideoPlayerExtensions
{
    public static VideoPlayer Source(this VideoPlayer widget, string source) => widget with { Source = source };

    public static VideoPlayer Autoplay(this VideoPlayer widget, bool autoplay = true) => widget with { Autoplay = autoplay };

    public static VideoPlayer Controls(this VideoPlayer widget, bool controls = true) => widget with { Controls = controls };

    public static VideoPlayer Muted(this VideoPlayer widget, bool muted = true) => widget with { Muted = muted };

    public static VideoPlayer Loop(this VideoPlayer widget, bool loop = true) => widget with { Loop = loop };

    public static VideoPlayer Poster(this VideoPlayer widget, string? poster = null) => widget with { Poster = poster };

    public static VideoPlayer Volume(this VideoPlayer widget, float? volume = null)
        => widget with { Volume = volume.HasValue ? Math.Clamp(volume.Value, 0f, 1f) : null };

    public static VideoPlayer Id(this VideoPlayer widget, string id) => widget with { Id = id };
}
