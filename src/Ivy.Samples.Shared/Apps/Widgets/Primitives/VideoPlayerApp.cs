
namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Film, group: ["Widgets"], isVisible: true, searchHints: ["media", "youtube", "playback", "video", "streaming", "embed"])]
public class VideoPlayerApp : SampleBase
{
    protected override object? BuildSample()
    {
        var client = UseService<IClientProvider>();

        // Event callbacks
        var playCount = UseState(0);
        var pauseCount = UseState(0);
        var completedState = UseState(false);
        var loadedState = UseState(false);

        var eventVideo = new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
            .HandlePlay(_ => playCount.Set(playCount.Value + 1))
            .HandlePause(_ => pauseCount.Set(pauseCount.Value + 1))
            .HandleEnded(_ => completedState.Set(true))
            .HandleLoaded(_ => loadedState.Set(true))
            .Height(Size.Units(50));

        // Basic video player
        var basicVideo = new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4");

        // Autoplay (muted) and looping video
        var autoplayMutedVideo = new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
            .Autoplay(true)
            .Muted(true);

        var loopingVideo = new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
            .Loop(true);

        // Controls toggle
        var withControls = new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
            .Controls(true);

        var withoutControls = new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
            .Controls(false);

        // Volume control
        var halfVolumeVideo = new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
            .Volume(0.5f);

        var quietVideo = new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
            .Volume(0.2f);

        // Custom sizing
        var customSizedVideo = new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
            .Width(Size.Fraction(0.5f))
            .Height(Size.Units(50));

        // Poster image
        var posterVideo = new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
            .Poster("https://www.w3schools.com/html/pic_trulli.jpg");

        // StartTime and EndTime
        var startTimeVideo = new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
            .StartTime(5);

        var segmentVideo = new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
            .StartTime(2)
            .EndTime(6);

        var youtubeSegment = new VideoPlayer("https://www.youtube.com/watch?v=dQw4w9WgXcQ")
            .StartTime(30)
            .EndTime(60)
            .Height(Size.Units(100));

        // YouTube video
        var youtubeVideo = new VideoPlayer("https://www.youtube.com/watch?v=dQw4w9WgXcQ&t=100")
            .Height(Size.Units(100));

        return Layout.Vertical()
            | Text.H2("Video Player Widget Examples")
            | Text.P("Demonstrates various configurations of the VideoPlayer widget, consistent with the documentation.")
            | Layout.Vertical().Gap(6)

                // Basic Usage
                | (new Card(
                    Layout.Vertical().Gap(4)
                    | Text.H4("Basic Video Player")
                    | Text.P("Default video player with standard browser controls.").Small()
                    | basicVideo
                ).Title("Basic Usage"))

                // Autoplay & Looping
                | (new Card(
                    Layout.Vertical().Gap(4)
                    | Text.H4("Muted Autoplay Video")
                    | Text.P("Muted autoplay video (browsers usually allow muted autoplay).").Small()
                    | autoplayMutedVideo
                    | Text.H4("Looping Video")
                    | loopingVideo
                ).Title("Autoplay and Looping"))

                // Controls Toggle
                | (new Card(
                    Layout.Vertical().Gap(4)
                    | Text.H4("Controls Toggle")
                    | Text.P("Video with controls enabled.").Small()
                    | withControls
                    | Text.P("Video without controls (programmatic control only).").Small()
                    | withoutControls
                    | new Button("Toggle Play/Pause", _ => client.Toast("In a real app, this would control the video programmatically"))
                        .Variant(ButtonVariant.Outline)
                ).Title("Controls Example"))

                // Volume Control
                | (new Card(
                    Layout.Vertical().Gap(4)
                    | Text.H4("50% Volume")
                    | Text.P("Video player set to 50% volume.").Small()
                    | halfVolumeVideo
                    | Text.H4("20% Volume")
                    | Text.P("Video player set to 20% volume (quiet background).").Small()
                    | quietVideo
                ).Title("Volume Control"))

                // Custom Sizing
                | (new Card(
                    Layout.Vertical().Gap(4)
                    | Text.H4("Custom Sized Video Player")
                    | Text.P("Video player with 50% width and fixed height.").Small()
                    | customSizedVideo
                ).Title("Custom Sizing"))

                // Poster
                | (new Card(
                    Layout.Vertical().Gap(4)
                    | Text.H4("Poster Image")
                    | Text.P("Video player with a preview image before playback.").Small()
                    | posterVideo
                ).Title("Poster Example"))

                // Time Range
                | (new Card(
                    Layout.Vertical().Gap(4)
                    | Text.H4("Start Time")
                    | Text.P("Video starts at 5 seconds.").Small()
                    | startTimeVideo
                    | Text.H4("Segment Playback")
                    | Text.P("Video plays from 2s to 6s.").Small()
                    | segmentVideo
                    | Text.H4("YouTube Segment")
                    | Text.P("YouTube video plays from 30s to 60s.").Small()
                    | youtubeSegment
                ).Title("Time Range Control"))

                // Event Callbacks
                | (new Card(
                    Layout.Vertical().Gap(4)
                    | Text.H4("Event Callbacks")
                    | Text.P("Track video playback events for analytics, sequential content, or coordinated UI.").Small()
                    | eventVideo
                    | (Layout.Horizontal().Gap(4)
                        | new Badge($"Play: {playCount.Value}").Variant(BadgeVariant.Secondary)
                        | new Badge($"Pause: {pauseCount.Value}").Variant(BadgeVariant.Secondary)
                        | new Badge(completedState.Value ? "Completed" : "Not completed")
                            .Variant(completedState.Value ? BadgeVariant.Success : BadgeVariant.Secondary)
                        | new Badge(loadedState.Value ? "Loaded" : "Loading")
                            .Variant(loadedState.Value ? BadgeVariant.Success : BadgeVariant.Secondary))
                ).Title("Event Callbacks"))

                // Playback Rate
                | (new Card(
                    Layout.Vertical().Gap(4)
                    | Text.H4("1.5x Speed (Fast)")
                    | Text.P("Play tutorial or lecture at increased speed.").Small()
                    | new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
                        .PlaybackRate(1.5)
                    | Text.H4("0.5x Speed (Slow)")
                    | Text.P("Slow motion for detailed review.").Small()
                    | new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
                        .PlaybackRate(0.5)
                    | Text.H4("2x Speed (Very Fast)")
                    | Text.P("Quickly scrub through long recordings.").Small()
                    | new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
                        .PlaybackRate(2.0)
                ).Title("Playback Rate"))

                // Subtitles
                | (new Card(
                    Layout.Vertical().Gap(4)
                    | Text.H4("Single Subtitle Track")
                    | Text.P("Video with a single English subtitle track.").Small()
                    | new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
                        .Subtitles("https://example.com/subs_en.vtt", "English")
                    | Text.H4("Multiple Subtitle Tracks")
                    | Text.P("Video with multiple subtitle tracks for different languages.").Small()
                    | new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
                        .Subtitles("https://example.com/subs_en.vtt", "English")
                        .Subtitles("https://example.com/subs_es.vtt", "Spanish")
                        .Subtitles("https://example.com/subs_fr.vtt", "French")
                ).Title("Subtitles"))

                // YouTube
                | (new Card(
                    Layout.Vertical().Gap(4)
                    | Text.H4("YouTube Video Embed")
                    | Text.P("Embed YouTube videos directly by URL. Supports normal videos, Shorts, and timecodes.").Small()
                        | new VideoPlayer("https://www.youtube.com/watch?v=dQw4w9WgXcQ&t=100s")
                            .Width(Size.Fraction(0.5f))
                            .Height(Size.Units(100))
                        | new VideoPlayer("https://www.youtube.com/shorts/41iWg91yFv0")
                            .Width(Size.Fraction(0.5f))
                            .Height(Size.Units(100))
                ).Title("YouTube Example"));
    }
}
