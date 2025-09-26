using Ivy.Core;

namespace Ivy;

/// <summary>
/// A video player control that allows users to play video from a given source.
/// </summary>
public record VideoPlayer : WidgetBase<VideoPlayer>
{
    /// <summary>
    /// Initializes a new instance with basic configuration.
    /// </summary>
    /// <param name="source">The video source URL or file path.</param>
    /// <param name="autoplay">Whether the video should start playing automatically.</param>
    /// <param name="controls">Whether playback controls should be displayed.</param>
    /// <param name="muted">Whether the video should be muted by default.</param>
    /// <param name="loop">Whether the video should loop continuously.</param>
    /// <param name="poster">URL of an image to display before the video starts or while loading.</param>
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

    /// <summary>Gets or sets the video source URL or file path.</summary>
    [Prop] public string? Source { get; set; }

    /// <summary>Gets or sets whether the video should start playing automatically.</summary>
    [Prop] public bool Autoplay { get; set; }

    /// <summary>Gets or sets whether playback controls should be displayed.</summary>
    [Prop] public bool Controls { get; set; }

    /// <summary>Gets or sets whether the video should be muted by default.</summary>
    [Prop] public bool Muted { get; set; }

    /// <summary>Gets or sets whether the video should loop continuously.</summary>
    [Prop] public bool Loop { get; set; }

    /// <summary>URL of an image shown before the video starts or while loading.</summary>
    [Prop] public string? Poster { get; set; }

}

/// <summary>
/// Provides extension methods for configuring video players.
/// </summary>
public static class VideoPlayerExtensions
{
    /// <summary>Sets the video source URL or file path.</summary>
    public static VideoPlayer Source(this VideoPlayer widget, string source)
    {
        return widget with { Source = source };
    }

    /// <summary>Sets autoplay mode.</summary>
    public static VideoPlayer Autoplay(this VideoPlayer widget, bool autoplay = true)
    {
        return widget with { Autoplay = autoplay };
    }

    /// <summary>Enables or disables playback controls.</summary>
    public static VideoPlayer Controls(this VideoPlayer widget, bool controls = true)
    {
        return widget with { Controls = controls };
    }

    /// <summary>Sets muted state.</summary>
    public static VideoPlayer Muted(this VideoPlayer widget, bool muted = true)
    {
        return widget with { Muted = muted };
    }

    /// <summary>Sets looping behavior.</summary>
    public static VideoPlayer Loop(this VideoPlayer widget, bool loop = true)
    {
        return widget with { Loop = loop };
    }

    /// <summary>Sets the poster image URL to display before the video starts or while loading.</summary>
    public static VideoPlayer Poster(this VideoPlayer widget, string? poster = null)
    {
        return widget with { Poster = poster };
    }

    /// <summary>Sets a unique identifier for the video player instance.</summary> 
    public static VideoPlayer Id(this VideoPlayer widget, string id)
    {
        return widget with { Id = id };
    }
}
