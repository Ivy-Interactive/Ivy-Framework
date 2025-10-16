using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.Volume2, path: ["Widgets"], searchHints: ["sound", "playback", "media", "mp3", "music", "audio"])]
public class AudioPlayerApp : SampleBase
{
    protected override object? BuildSample()
    {
        var client = UseService<IClientProvider>();

        // Basic audio player with default settings

        var basicAudio = new Audio("https://www.learningcontainer.com/wp-content/uploads/2020/02/Kalimba.mp3")
            .TestId("audio-basic");

        // Audio player with custom settings
        var customAudio = new Audio("https://www.learningcontainer.com/wp-content/uploads/2020/02/Kalimba.mp3")
            .Loop(true)
            .Preload(AudioPreload.Auto)
            .TestId("audio-looping");

        // Muted audio player (useful for autoplay scenarios)
        var mutedAudio = new Audio("https://www.learningcontainer.com/wp-content/uploads/2020/02/Kalimba.mp3")
            .Muted(true)
            .Autoplay(true)
            .Loop(true)
            .TestId("audio-muted-autoplay");

        // Audio without controls (programmatic control only)
        var noControlsAudio = new Audio("https://www.learningcontainer.com/wp-content/uploads/2020/02/Kalimba.mp3")
            .Controls(false)
            .Muted(true)
            .TestId("audio-no-controls");

        // Custom sized audio player
        var customSizedAudio = new Audio("https://www.learningcontainer.com/wp-content/uploads/2020/02/Kalimba.mp3")
            .Width(Size.Fraction(0.5f))
            .Height(Size.Units(12))
            .TestId("audio-custom-sized");

        // Theme awareness audio player (separate instance to avoid reuse issues)
        var themeAudio = new Audio("https://www.learningcontainer.com/wp-content/uploads/2020/02/Kalimba.mp3")
            .TestId("audio-theme");

        return Layout.Vertical()
               | Text.H1("Audio Player Widget Examples")
               | Text.P("Demonstrates various configurations of the Audio widget for playing audio content. This widget is for audio playback, not recording. The audio player is theme-aware and adapts to light/dark themes.")
               | Layout.Vertical().Gap(6)
                   | (new Card(
                       Layout.Vertical().Gap(4)
                       | Text.H4("Basic Audio Player")
                       | Text.Small("Default audio player with standard browser controls.")
                       | basicAudio
                   ).Title("Basic Usage"))
                   | (new Card(
                       Layout.Vertical().Gap(4)
                       | Text.H4("Looping Audio with Preload")
                       | Text.Small("Audio player configured to loop continuously with auto preload.")
                       | customAudio
                   ).Title("Custom Configuration"))
                   | (new Card(
                       Layout.Vertical().Gap(4)
                       | Text.H4("Muted Autoplay Audio")
                       | Text.Small("Muted audio that starts playing automatically and loops. Muted autoplay is more likely to be allowed by browsers.")
                       | mutedAudio
                   ).Title("Autoplay Example"))
                   | (new Card(
                       Layout.Vertical().Gap(4)
                       | Text.H4("Audio Without Controls")
                       | Text.Small("Audio element without browser controls for programmatic control scenarios.")
                       | noControlsAudio
                       | new Button("Toggle Play/Pause", _ => client.Toast("In a real app, this would control the audio programmatically"))
                           .Variant(ButtonVariant.Outline)
                           .TestId("toggle-play-pause-button")
                   ).Title("Programmatic Control"))
                   | (new Card(
                       Layout.Vertical().Gap(4)
                       | Text.H4("Custom Sized Audio Player")
                       | Text.Small("Audio player with custom width and height dimensions.")
                       | customSizedAudio
                   ).Title("Custom Sizing"))
                  | (new Card(
                      Layout.Vertical().Gap(4)
                      | Text.H4("Theme Awareness")
                      | Text.Small("The audio player automatically adapts to your current theme (light/dark mode). The controls, background, and text colors adjust accordingly.")
                      | themeAudio
                      | Text.Small("Try switching between light and dark themes to see the audio player adapt!")
                  ).Title("Theme Integration"))
                  ;
    }
}
