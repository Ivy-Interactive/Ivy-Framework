using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets;

<<<<<<< HEAD
[App(icon: Icons.Mic, path: ["Widgets"])]
=======
[App(icon: Icons.Mic, path: ["Widgets", "Inputs"])]
>>>>>>> 9bfa842a (changed folder structuring and naming convention (#813))
public class AudioRecorderApp() : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
<<<<<<< HEAD
               | Text.H1("Audio Recorder Widget Examples")
               | Text.P("Demonstrates the AudioRecorder widget for capturing audio input. This widget is for recording audio, not playing it. The recorder interface is theme-aware and adapts to light/dark themes.")
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
=======
               | Text.H1("Audio recorder")
               | new AudioRecorder("Start recording", "Recording audio...")

               | Text.H2("Disabled")
               | new AudioRecorder("Start recording", "Recording audio...", disabled: true);
>>>>>>> 9bfa842a (changed folder structuring and naming convention (#813))
    }
}