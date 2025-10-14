using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;


[App(icon: Icons.Mic, path: ["Widgets", "Inputs"], searchHints: ["microphone", "recording", "voice", "audio", "capture", "sound"])]

public class AudioRecorderApp() : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("Audio Recorder Widget Examples")
               | Text.P("Demonstrates the AudioRecorder widget for capturing audio input. This widget is for recording audio, not playing it. The recorder interface is theme-aware and adapts to light/dark themes.")
               | Text.H2("Sizes")
               | CreateSizesSection()
               | Text.H2("Basic Examples")
               | Layout.Vertical().Gap(6)
                   | (new Card(
                       Layout.Vertical().Gap(4)
                       | Text.H4("Basic Audio Recorder")
                       | Text.Small("Default audio recorder with microphone access.")
                       | new AudioRecorder("Start recording", "Recording audio...")
                   ).Title("Basic Usage"))
                   | (new Card(
                       Layout.Vertical().Gap(4)
                       | Text.H4("Disabled Audio Recorder")
                       | Text.Small("Audio recorder in disabled state.")
                       | new AudioRecorder("Start recording", "Recording audio...", disabled: true)
                   ).Title("Disabled State"));
    }

    private object CreateSizesSection()
    {
        return Layout.Grid().Columns(4)
               | Text.InlineCode("Description")
               | Text.InlineCode("Small")
               | Text.InlineCode("Medium")
               | Text.InlineCode("Large")

               | Text.InlineCode("Audio Recorder")
               | new AudioRecorder("Start recording", "Recording audio...").Small()
               | new AudioRecorder("Start recording", "Recording audio...")
               | new AudioRecorder("Start recording", "Recording audio...").Large()

               | Text.InlineCode("Disabled State")
               | new AudioRecorder("Start recording", "Recording audio...", disabled: true).Small()
               | new AudioRecorder("Start recording", "Recording audio...", disabled: true)
               | new AudioRecorder("Start recording", "Recording audio...", disabled: true).Large();
    }
}