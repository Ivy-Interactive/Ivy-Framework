namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.Mic, group: ["Widgets", "Inputs"], searchHints: ["speech", "voice", "dictation", "transcription", "microphone", "stt"])]
public class DictationApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical()
               | Text.H1("Dictation")
               | Text.P("TextInput with speech-to-text dictation. Click the microphone icon to start recording, click again to stop. The audio is sent to the server for transcription and the result is appended to the input.")
               | Layout.Tabs(
                   new Tab("Basic", new DictationBasicTab()),
                   new Tab("Multiline", new DictationMultilineTab())
               ).Variant(TabsVariant.Content);
    }
}

public class DictationBasicTab : ViewBase
{
    public override object Build()
    {
        var text = UseState("");

        return Layout.Vertical()
               | text.ToTextInput().Placeholder("Click mic to dictate...").EnableDictation()
               | Text.Muted($"Value: {text.Value}");
    }
}

public class DictationMultilineTab : ViewBase
{
    public override object Build()
    {
        var multilineText = UseState("");

        return Layout.Vertical()
               | multilineText.ToTextareaInput().Placeholder("Dictate into a textarea...").EnableDictation()
               | Text.Muted($"Value: {multilineText.Value}");
    }
}
