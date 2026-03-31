namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.Mic, group: ["Widgets", "Inputs"], searchHints: ["speech", "voice", "dictation", "transcription", "microphone", "stt"])]
public class DictationApp : SampleBase
{
    protected override object? BuildSample()
    {
        var text = UseState("");
        var multilineText = UseState("");

        return Layout.Vertical()
               | Text.H1("Dictation")
               | Text.P("TextInput with speech-to-text dictation. Click the microphone icon to start recording, click again to stop. The audio is sent to the server for transcription and the result is appended to the input.")
               | Text.H2("Basic Dictation")
               | text.ToTextInput().Placeholder("Click mic to dictate...").EnableDictation()
               | Text.Muted($"Value: {text.Value}")
               | Text.H2("Multiline Dictation")
               | multilineText.ToTextareaInput().Placeholder("Dictate into a textarea...").EnableDictation()
               | Text.Muted($"Value: {multilineText.Value}");
    }
}
