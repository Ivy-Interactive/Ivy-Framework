using System.Collections.Immutable;

namespace Ivy.Samples.Shared.Apps.Widgets.Inputs;

[App(icon: Icons.Zap, group: ["Widgets", "Inputs"], isVisible: false, title: "Input Events")]
public class InputEventsApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical().Gap(8)
            | Layout.Vertical().Gap(2)
                | Text.H1("Input Events Lifecycle")
                | Text.P("Testing event propagation for **Focus**, **Blur**, and **AutoFocus** across all Ivy input widgets. Use the tabs below to switch between different input types.")
            | Layout.Tabs(
                new Tab("TextInput", new TextInputEvents()),
                new Tab("NumberInput", new NumberInputEvents()),
                new Tab("NumberRange", new NumberRangeInputEvents()),
                new Tab("BoolInput", new BoolInputEvents()),
                new Tab("SelectInput", new SelectInputEvents()),
                new Tab("AsyncSelectInput", new AsyncSelectInputEvents()),
                new Tab("DateTimeInput", new DateTimeInputEvents()),
                new Tab("DateRangeInput", new DateRangeInputEvents()),
                new Tab("ColorInput", new ColorInputEvents()),
                new Tab("CodeInput", new CodeInputEvents()),
                new Tab("FeedbackInput", new FeedbackInputEvents()),
                new Tab("FileInput", new FileInputEvents()),
                new Tab("AudioInput", new AudioInputEventsView()),
                new Tab("CameraInput", new CameraInputEventsView()),
                new Tab("Signature", new SignatureInputEventsView())
            ).Variant(TabsVariant.Content);
    }
}

public abstract class BaseInputEvents<T> : ViewBase
{
    protected abstract string Name { get; }
    protected abstract object CreateInput(IState<T> state);
    protected virtual T DefaultValue => default!;

    public override object? Build()
    {
        var blurCount = UseState(0);
        var focusCount = UseState(0);
        var state = UseState(DefaultValue);

        return Layout.Vertical().Gap(6)
            | Layout.Tabs(
                new Tab("OnFocus", Layout.Vertical().Gap(4)
                    | Text.P($"The **OnFocus** event fires when the {Name} gains focus (via mouse or keyboard tab).")
                    | new Card(
                        Layout.Vertical().Center().Gap(6)
                            | ((IAnyInput)CreateInput(state)).OnFocus(_ => focusCount.Set(focusCount.Value + 1))
                            | Layout.Vertical().Center().Gap(1)
                                | Text.H2(focusCount.Value.ToString()).Color(Colors.Primary)
                                | Text.Muted("Focus Count")
                      ).Width(Size.Units(120))
                ),
                new Tab("OnBlur", Layout.Vertical().Gap(4)
                    | Text.P($"The **OnBlur** event fires when the {Name} loses focus.")
                    | new Card(
                        Layout.Vertical().Center().Gap(6)
                            | ((IAnyInput)CreateInput(state)).OnBlur(_ => blurCount.Set(blurCount.Value + 1))
                            | Layout.Vertical().Center().Gap(1)
                                | Text.H2(blurCount.Value.ToString()).Color(Colors.Orange)
                                | Text.Muted("Blur Count")
                      ).Width(Size.Units(120))
                ),
                new Tab("AutoFocus", Layout.Vertical().Gap(4)
                    | Text.P($"The **AutoFocus** property should automatically focus the {Name} when it is mounted.")
                    | new Card(
                        Layout.Vertical().Center().Gap(6)
                            | ((IAnyInput)CreateInput(state)).AutoFocus()
                            | Text.Lead("Focused!").Color(Colors.Green)
                            | Text.P("The field above should have a focus ring immediately.").Small().Muted()
                      ).Width(Size.Units(120))
                )
            ).Variant(TabsVariant.Tabs);
    }
}

public class TextInputEvents : BaseInputEvents<string>
{
    protected override string Name => "TextInput";
    protected override object CreateInput(IState<string> state) => state.ToTextInput().Placeholder("Focus me...");
}

public class NumberInputEvents : BaseInputEvents<double>
{
    protected override string Name => "NumberInput";
    protected override object CreateInput(IState<double> state) => state.ToNumberInput();
}

public class NumberRangeInputEvents : BaseInputEvents<(double, double)>
{
    protected override string Name => "NumberRangeInput";
    protected override (double, double) DefaultValue => (20, 80);
    protected override object CreateInput(IState<(double, double)> state) => state.ToNumberRangeInput().Min(0).Max(100);
}

public class BoolInputEvents : BaseInputEvents<bool>
{
    protected override string Name => "BoolInput";
    protected override object CreateInput(IState<bool> state) => state.ToBoolInput().Label("Checkbox Event Test");
}

public class SelectInputEvents : BaseInputEvents<string>
{
    protected override string Name => "SelectInput";
    protected override string DefaultValue => "Option 2";
    protected override object CreateInput(IState<string> state) => state.ToSelectInput(["Option 1", "Option 2", "Option 3"]);
}

public class AsyncSelectInputEvents : BaseInputEvents<string>
{
    protected override string Name => "AsyncSelectInput";
    protected override object CreateInput(IState<string> state)
    {
        return state.ToAsyncSelectInput(Search, Lookup, placeholder: "Select a color...");
    }

    private QueryResult<Option<string>[]> Search(IViewContext context, string query)
    {
        return context.UseQuery<Option<string>[], (string, string)>(
            key: (nameof(Search), query),
            fetcher: _ => Task.FromResult(new[] { "Red", "Green", "Blue", "Yellow", "Purple" }
                .Where(x => x.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(x => new Option<string>(x, x))
                .ToArray()));
    }

    private QueryResult<Option<string>?> Lookup(IViewContext context, string? value)
    {
        return context.UseQuery<Option<string>?, (string, string?)>(
            key: (nameof(Lookup), value),
            fetcher: _ => Task.FromResult(string.IsNullOrEmpty(value) ? null : new Option<string>(value, value)));
    }
}

public class DateTimeInputEvents : BaseInputEvents<DateTime?>
{
    protected override string Name => "DateTimeInput";
    protected override object CreateInput(IState<DateTime?> state) => state.ToDateTimeInput();
}

public class DateRangeInputEvents : BaseInputEvents<(DateOnly?, DateOnly?)>
{
    protected override string Name => "DateRangeInput";
    protected override object CreateInput(IState<(DateOnly?, DateOnly?)> state) => state.ToDateRangeInput();
}

public class ColorInputEvents : BaseInputEvents<string>
{
    protected override string Name => "ColorInput";
    protected override string DefaultValue => "#6366f1";
    protected override object CreateInput(IState<string> state) => state.ToColorInput();
}

public class CodeInputEvents : BaseInputEvents<string>
{
    protected override string Name => "CodeInput";
    protected override string DefaultValue => "// Test event handling in CodeInput\nconsole.log('Focused');";
    protected override object CreateInput(IState<string> state) => state.ToCodeInput().Language(Languages.Javascript);
}

public class FeedbackInputEvents : BaseInputEvents<int>
{
    protected override string Name => "FeedbackInput";
    protected override int DefaultValue => 0;
    protected override object CreateInput(IState<int> state) => state.ToFeedbackInput();
}

public class FileInputEvents : ViewBase
{
    public override object? Build()
    {
        var blurCount = UseState(0);
        var focusCount = UseState(0);
        var files = UseState(ImmutableArray.Create<FileUpload<byte[]>>());
        var upload = UseUpload(MemoryStreamUploadHandler.Create(files));

        return Layout.Vertical().Gap(6)
            | Layout.Tabs(
                new Tab("OnFocus", Layout.Vertical().Gap(4)
                    | Text.P("The **OnFocus** event fires when the FileInput gains focus.")
                    | new Card(
                        Layout.Vertical().Center().Gap(6)
                            | files.ToFileInput(upload).OnFocus(_ => focusCount.Set(focusCount.Value + 1))
                            | Layout.Vertical().Center().Gap(1)
                                | Text.H2(focusCount.Value.ToString()).Color(Colors.Primary)
                                | Text.Muted("Focus Count")
                      ).Width(Size.Units(120))
                ),
                new Tab("OnBlur", Layout.Vertical().Gap(4)
                    | Text.P("The **OnBlur** event fires when the FileInput loses focus.")
                    | new Card(
                        Layout.Vertical().Center().Gap(6)
                            | files.ToFileInput(upload).OnBlur(_ => blurCount.Set(blurCount.Value + 1))
                            | Layout.Vertical().Center().Gap(1)
                                | Text.H2(blurCount.Value.ToString()).Color(Colors.Orange)
                                | Text.Muted("Blur Count")
                      ).Width(Size.Units(120))
                ),
                new Tab("AutoFocus", Layout.Vertical().Gap(4)
                    | Text.P("The **AutoFocus** property should automatically focus the FileInput when it is mounted.")
                    | new Card(
                        Layout.Vertical().Center().Gap(6)
                            | files.ToFileInput(upload).AutoFocus()
                            | Text.Lead("Focused!").Color(Colors.Green)
                      ).Width(Size.Units(120))
                )
            ).Variant(TabsVariant.Tabs);
    }
}

public class AudioInputEventsView : ViewBase
{
    public override object? Build()
    {
        var blurCount = UseState(0);
        var focusCount = UseState(0);
        var dummyUpload = UseUpload(
            (fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask,
            defaultContentType: "audio/webm"
        );

        return Layout.Vertical().Gap(6)
            | Layout.Tabs(
                new Tab("OnFocus", Layout.Vertical().Gap(4)
                    | Text.P("The **OnFocus** event fires when the AudioInput gains focus.")
                    | new Card(
                        Layout.Vertical().Center().Gap(6)
                            | new AudioInput(dummyUpload.Value, "Focus me").OnFocus(_ => focusCount.Set(focusCount.Value + 1))
                            | Layout.Vertical().Center().Gap(1)
                                | Text.H2(focusCount.Value.ToString()).Color(Colors.Primary)
                                | Text.Muted("Focus Count")
                      ).Width(Size.Units(120))
                ),
                new Tab("OnBlur", Layout.Vertical().Gap(4)
                    | Text.P("The **OnBlur** event fires when the AudioInput loses focus.")
                    | new Card(
                        Layout.Vertical().Center().Gap(6)
                            | new AudioInput(dummyUpload.Value, "Blur me").OnBlur(_ => blurCount.Set(blurCount.Value + 1))
                            | Layout.Vertical().Center().Gap(1)
                                | Text.H2(blurCount.Value.ToString()).Color(Colors.Orange)
                                | Text.Muted("Blur Count")
                      ).Width(Size.Units(120))
                ),
                new Tab("AutoFocus", Layout.Vertical().Gap(4)
                    | Text.P("The **AutoFocus** property should automatically focus the AudioInput when it is mounted.")
                    | new Card(
                        Layout.Vertical().Center().Gap(6)
                            | new AudioInput(dummyUpload.Value, "AutoFocused").AutoFocus()
                            | Text.Lead("Focused!").Color(Colors.Green)
                      ).Width(Size.Units(120))
                )
            ).Variant(TabsVariant.Tabs);
    }
}

public class CameraInputEventsView : ViewBase
{
    public override object? Build()
    {
        var blurCount = UseState(0);
        var focusCount = UseState(0);
        var dummyUpload = UseUpload(
            (fileUpload, stream, cancellationToken) => System.Threading.Tasks.Task.CompletedTask,
            defaultContentType: "image/png"
        );

        return Layout.Vertical().Gap(6)
            | Layout.Tabs(
                new Tab("OnFocus", Layout.Vertical().Gap(4)
                    | Text.P("The **OnFocus** event fires when the CameraInput gains focus.")
                    | new Card(
                        Layout.Vertical().Center().Gap(6)
                            | new CameraInput(dummyUpload.Value, "Focus me").OnFocus(_ => focusCount.Set(focusCount.Value + 1))
                            | Layout.Vertical().Center().Gap(1)
                                | Text.H2(focusCount.Value.ToString()).Color(Colors.Primary)
                                | Text.Muted("Focus Count")
                      ).Width(Size.Units(120))
                ),
                new Tab("OnBlur", Layout.Vertical().Gap(4)
                    | Text.P("The **OnBlur** event fires when the CameraInput loses focus.")
                    | new Card(
                        Layout.Vertical().Center().Gap(6)
                            | new CameraInput(dummyUpload.Value, "Blur me").OnBlur(_ => blurCount.Set(blurCount.Value + 1))
                            | Layout.Vertical().Center().Gap(1)
                                | Text.H2(blurCount.Value.ToString()).Color(Colors.Orange)
                                | Text.Muted("Blur Count")
                      ).Width(Size.Units(120))
                ),
                new Tab("AutoFocus", Layout.Vertical().Gap(4)
                    | Text.P("The **AutoFocus** property should automatically focus the CameraInput when it is mounted.")
                    | new Card(
                        Layout.Vertical().Center().Gap(6)
                            | new CameraInput(dummyUpload.Value, "AutoFocused").AutoFocus()
                            | Text.Lead("Focused!").Color(Colors.Green)
                      ).Width(Size.Units(120))
                )
            ).Variant(TabsVariant.Tabs);
    }
}

public class SignatureInputEventsView : BaseInputEvents<byte[]?>
{
    protected override string Name => "SignatureInput";
    protected override object CreateInput(IState<byte[]?> state) => state.ToSignatureInput();
}
