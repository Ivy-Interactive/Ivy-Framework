using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Volume2, path: ["Widgets", "Primitives"], searchHints: ["sound", "playback", "media", "mp3", "music", "player"])]
public class AudioApp : SampleBase
{
    protected override object? BuildSample()
    {
        var client = UseService<IClientProvider>();

        // Basic audio player with default settings
        var basicAudio = new Audio("https://www.learningcontainer.com/wp-content/uploads/2020/02/Kalimba.mp3");

        // Audio player with custom settings
        var customAudio = new Audio("https://www.learningcontainer.com/wp-content/uploads/2020/02/Kalimba.mp3")
            .Loop(true)
            .Preload(AudioPreload.Auto);

        // Muted audio player (useful for autoplay scenarios)
        var mutedAudio = new Audio("https://www.learningcontainer.com/wp-content/uploads/2020/02/Kalimba.mp3")
            .Muted(true)
            .Autoplay(true)
            .Loop(true);

        // Audio without controls (programmatic control only)
        var noControlsAudio = new Audio("https://www.learningcontainer.com/wp-content/uploads/2020/02/Kalimba.mp3")
            .Controls(false)
            .Muted(true);

        // Custom sized audio player
        var customSizedAudio = new Audio("https://www.learningcontainer.com/wp-content/uploads/2020/02/Kalimba.mp3")
            .Width(Size.Fraction(0.5f))
            .Height(Size.Units(12));

        return Layout.Vertical()
            | Text.H2("Audio Widget Examples")
            | Text.P("Demonstrates various configurations of the Audio widget for playing audio content.")
            | Layout.Vertical().Gap(6)
                | (new Card(
                    Layout.Vertical().Gap(4)
                    | Text.H4("Basic Audio Player")
                    | Text.P("Default audio player with standard browser controls.")
                    | basicAudio
                ).Title("Basic Usage"))
                | (new Card(
                    Layout.Vertical().Gap(4)
                    | Text.H4("Looping Audio with Preload")
                    | Text.P("Audio player configured to loop continuously with auto preload.")
                    | customAudio
                ).Title("Custom Configuration"))
                | (new Card(
                    Layout.Vertical().Gap(4)
                    | Text.H4("Muted Autoplay Audio")
                    | Text.P("Muted audio that starts playing automatically and loops. Muted autoplay is more likely to be allowed by browsers.")
                    | mutedAudio
                ).Title("Autoplay Example"))
                | (new Card(
                    Layout.Vertical().Gap(4)
                    | Text.H4("Audio Without Controls")
                    | Text.P("Audio element without browser controls for programmatic control scenarios.")
                    | noControlsAudio
                    | new Button("Toggle Play/Pause", _ => client.Toast("In a real app, this would control the audio programmatically"))
                        .Variant(ButtonVariant.Outline)
                ).Title("Programmatic Control"))
                | (new Card(
                    Layout.Vertical().Gap(4)
                    | Text.H4("Custom Sized Audio Player")
                    | Text.P("Audio player with custom width and height dimensions.")
                    | customSizedAudio
                ).Title("Custom Sizing"))
            | Layout.Vertical().Gap(4)
                | Text.H3("Usage Examples")
                | new Code("""
                    // Basic audio player
                    var audio = new Audio("path/to/audio.mp3");
                    
                    // Audio with custom settings
                    var customAudio = new Audio("path/to/audio.mp3")
                        .Loop(true)
                        .Preload(AudioPreload.Auto)
                        .Muted(true);
                    
                    // Custom sized audio
                    var sizedAudio = new Audio("path/to/audio.mp3")
                        .Width(Size.Fraction(0.5f))
                        .Height(Size.Units(12));
                    """, Languages.Csharp);
    }
}
