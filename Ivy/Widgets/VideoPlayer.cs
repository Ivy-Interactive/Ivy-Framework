using Ivy.Core;

namespace Ivy;

public record VideoPlayer : WidgetBase<VideoPlayer>
{
    public VideoPlayer(
        string? source = null,
        bool autoplay = false,
        bool controls = true,
        bool muted = false,
        bool loop = false,
        string? poster = null)
    {
        Source = source;
        Autoplay = autoplay;
        Controls = controls;
        Muted = muted;
        Loop = loop;
        Poster = poster;
        Id = Guid.NewGuid().ToString();
    }

    [Prop] public string? Source { get; set; }

    [Prop] public bool Autoplay { get; set; }

    [Prop] public bool Controls { get; set; }

    [Prop] public bool Muted { get; set; }

    [Prop] public bool Loop { get; set; }

    [Prop] public string? Poster { get; set; }

}

public static class VideoPlayerExtensions
{
    public static VideoPlayer Source(this VideoPlayer widget, string source)
    {
        return widget with { Source = source };
    }

    public static VideoPlayer Autoplay(this VideoPlayer widget, bool autoplay = true)
    {
        return widget with { Autoplay = autoplay };
    }

    public static VideoPlayer Controls(this VideoPlayer widget, bool controls = true)
    {
        return widget with { Controls = controls };
    }

    public static VideoPlayer Muted(this VideoPlayer widget, bool muted = true)
    {
        return widget with { Muted = muted };
    }

    public static VideoPlayer Loop(this VideoPlayer widget, bool loop = true)
    {
        return widget with { Loop = loop };
    }

    public static VideoPlayer Poster(this VideoPlayer widget, string? poster = null)
    {
        return widget with { Poster = poster };
    }

    public static VideoPlayer Id(this VideoPlayer widget, string id)
    {
        return widget with { Id = id };
    }
}
