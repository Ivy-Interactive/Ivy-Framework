using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.Mic, path: ["Widgets"])]
public class AudioRecorderApp() : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("Audio recorder")
               | new AudioRecorder("Start recording", "Recording audio...")

               | Text.H2("Disabled")
               | new AudioRecorder("Start recording", "Recording audio...", disabled: true);
    }
}