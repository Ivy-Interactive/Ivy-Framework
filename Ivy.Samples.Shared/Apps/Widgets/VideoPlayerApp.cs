using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.Film, path: ["Widgets"], isVisible: true, searchHints: ["media", "youtube", "playback", "video", "streaming", "embed"])]
public class VideoPlayerApp : SampleBase
{
    protected override object? BuildSample()
    {
        var client = UseService<IClientProvider>();

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

        // Custom sizing
        var customSizedVideo = new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
            .Width(Size.Fraction(0.5f))
            .Height(Size.Units(50));

        // Poster image
        var posterVideo = new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
            .Poster("https://www.w3schools.com/html/pic_trulli.jpg");

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
                    | Text.Small("Default video player with standard browser controls.")
                    | basicVideo
                ).Title("Basic Usage"))

                // Autoplay & Looping
                | (new Card(
                    Layout.Vertical().Gap(4)
                    | Text.H4("Muted Autoplay Video")
                    | Text.Small("Muted autoplay video (browsers usually allow muted autoplay).")
                    | autoplayMutedVideo
                    | Text.H4("Looping Video")
                    | loopingVideo
                ).Title("Autoplay and Looping"))

                // Controls Toggle
                | (new Card(
                    Layout.Vertical().Gap(4)
                    | Text.H4("Controls Toggle")
                    | Text.Small("Video with controls enabled.")
                    | withControls
                    | Text.Small("Video without controls (programmatic control only).")
                    | withoutControls
                    | new Button("Toggle Play/Pause", _ => client.Toast("In a real app, this would control the video programmatically"))
                        .Variant(ButtonVariant.Outline)
                ).Title("Controls Example"))

                // Custom Sizing
                | (new Card(
                    Layout.Vertical().Gap(4)
                    | Text.H4("Custom Sized Video Player")
                    | Text.Small("Video player with 50% width and fixed height.")
                    | customSizedVideo
                ).Title("Custom Sizing"))

                // Poster
                | (new Card(
                    Layout.Vertical().Gap(4)
                    | Text.H4("Poster Image")
                    | Text.Small("Video player with a preview image before playback.")
                    | posterVideo
                ).Title("Poster Example"))

                // YouTube
                | (new Card(
                    Layout.Vertical().Gap(4)
                    | Text.H4("YouTube Video Embed")
                    | Text.Small("Embed YouTube videos directly by URL. Supports normal videos, Shorts, and timecodes.")
                        | new VideoPlayer("https://www.youtube.com/watch?v=dQw4w9WgXcQ&t=100s")
                            .Width(Size.Fraction(0.5f))
                            .Height(Size.Units(100))
                        | new VideoPlayer("https://www.youtube.com/shorts/41iWg91yFv0")
                            .Width(Size.Fraction(0.5f))
                            .Height(Size.Units(100))
                ).Title("YouTube Example"));
    }
}
