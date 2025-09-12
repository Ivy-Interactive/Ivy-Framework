using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.Mic, path: ["Widgets"])]
public class AudioRecorderApp() : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("Audio Recorder Widget Examples")
               | Text.P("Demonstrates the AudioRecorder widget for capturing audio input. This widget is for recording audio, not playing it.")
                   .Color(Colors.Secondary)
               | new AudioRecorder("Start recording", "Recording audio...")

               | Text.H2("Disabled")
               | new AudioRecorder("Start recording", "Recording audio...", disabled: true);
    }
}