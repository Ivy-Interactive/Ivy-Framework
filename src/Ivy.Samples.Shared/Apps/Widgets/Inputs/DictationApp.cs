namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.Mic, group: ["Widgets", "Inputs"], searchHints: ["speech", "voice", "dictation", "transcription", "microphone", "stt"])]
public class DictationApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Tabs(
            new Tab("Basic", new DictationBasicSample()),
            new Tab("Multiline", new DictationMultilineSample())
        ).Variant(TabsVariant.Content);
    }
}

public class DictationBasicSample : ViewBase
{
    public override object? Build()
    {
        var text = UseState("");

        return Layout.Vertical()
            | Text.P("TextInput with speech-to-text dictation. Click the microphone icon to start recording, click again to stop. The audio is sent to the server for transcription and the result is appended to the input.")
            | text.ToTextInput().Placeholder("Click mic to dictate...").EnableDictation()
            | Text.Muted($"Value: {text.Value}");
    }
}

public class DictationMultilineSample : ViewBase
{
    public override object? Build()
    {
        var multilineText = UseState("");

        return Layout.Vertical()
            | multilineText.ToTextareaInput().Placeholder("Dictate into a textarea...").EnableDictation()
            | Text.Muted($"Value: {multilineText.Value}");
    }
}
