using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record SubtitleTrack
{
    public string Source { get; set; } = "";
    public string? Label { get; set; }
}

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
        float? volume = null,
        int? startTime = null,
        int? endTime = null,
        double? playbackRate = null) : this()
    {
        Source = source;
        Autoplay = autoplay;
        Controls = controls;
        Muted = muted;
        Loop = loop;
        Poster = poster;
        Volume = volume.HasValue ? Math.Clamp(volume.Value, 0f, 1f) : null;
        StartTime = startTime;
        EndTime = endTime;
        PlaybackRate = playbackRate.HasValue ? Math.Max(0.25, playbackRate.Value) : null;
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

    [Prop] public int? StartTime { get; set; }

    [Prop] public int? EndTime { get; set; }

    [Prop] public double? PlaybackRate { get; set; }

    [Prop] public List<SubtitleTrack>? Subtitles { get; set; }

    [Event] public EventHandler<Event<VideoPlayer>>? OnPlay { get; set; }

    [Event] public EventHandler<Event<VideoPlayer>>? OnPause { get; set; }

    [Event] public EventHandler<Event<VideoPlayer>>? OnEnded { get; set; }

    [Event] public EventHandler<Event<VideoPlayer>>? OnLoaded { get; set; }
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

    public static VideoPlayer StartTime(this VideoPlayer widget, int? startTime = null) => widget with { StartTime = startTime };

    public static VideoPlayer EndTime(this VideoPlayer widget, int? endTime = null) => widget with { EndTime = endTime };

    public static VideoPlayer PlaybackRate(this VideoPlayer widget, double rate) => widget with { PlaybackRate = Math.Max(0.25, rate) };

    public static VideoPlayer Subtitles(this VideoPlayer widget, string source, string? label = null)
    {
        var tracks = widget.Subtitles ?? new List<SubtitleTrack>();
        tracks.Add(new SubtitleTrack { Source = source, Label = label });
        return widget with { Subtitles = tracks };
    }

    public static VideoPlayer Id(this VideoPlayer widget, string id) => widget with { Id = id };

    public static VideoPlayer OnPlay(this VideoPlayer widget, Func<Event<VideoPlayer>, ValueTask> onPlay) => widget with { OnPlay = new(onPlay) };

    public static VideoPlayer OnPlay(this VideoPlayer widget, Action<Event<VideoPlayer>> onPlay) => widget with { OnPlay = new(onPlay.ToValueTask()) };

    public static VideoPlayer OnPlay(this VideoPlayer widget, Action onPlay) => widget with { OnPlay = new(_ => { onPlay(); return ValueTask.CompletedTask; }) };

    public static VideoPlayer OnPause(this VideoPlayer widget, Func<Event<VideoPlayer>, ValueTask> onPause) => widget with { OnPause = new(onPause) };

    public static VideoPlayer OnPause(this VideoPlayer widget, Action<Event<VideoPlayer>> onPause) => widget with { OnPause = new(onPause.ToValueTask()) };

    public static VideoPlayer OnPause(this VideoPlayer widget, Action onPause) => widget with { OnPause = new(_ => { onPause(); return ValueTask.CompletedTask; }) };

    public static VideoPlayer OnEnded(this VideoPlayer widget, Func<Event<VideoPlayer>, ValueTask> onEnded) => widget with { OnEnded = new(onEnded) };

    public static VideoPlayer OnEnded(this VideoPlayer widget, Action<Event<VideoPlayer>> onEnded) => widget with { OnEnded = new(onEnded.ToValueTask()) };

    public static VideoPlayer OnEnded(this VideoPlayer widget, Action onEnded) => widget with { OnEnded = new(_ => { onEnded(); return ValueTask.CompletedTask; }) };

    public static VideoPlayer OnLoaded(this VideoPlayer widget, Func<Event<VideoPlayer>, ValueTask> onLoaded) => widget with { OnLoaded = new(onLoaded) };

    public static VideoPlayer OnLoaded(this VideoPlayer widget, Action<Event<VideoPlayer>> onLoaded) => widget with { OnLoaded = new(onLoaded.ToValueTask()) };

    public static VideoPlayer OnLoaded(this VideoPlayer widget, Action onLoaded) => widget with { OnLoaded = new(_ => { onLoaded(); return ValueTask.CompletedTask; }) };
}
