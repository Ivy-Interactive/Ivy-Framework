namespace Ivy.Samples.Shared.Apps.Advanced;

[App(icon: Icons.Code, path: ["Advanced"])]
public class XamlBuilderApp : ViewBase
{
    private static readonly Dictionary<string, string> Examples = new()
    {
        ["Simple Layout"] = """<StackLayout Orientation="Vertical"><Badge Title="Hello" /><Badge Title="World" Variant="Success" /></StackLayout>""",
        ["Buttons"] = """<StackLayout Orientation="Horizontal" Gap="2"><Button Title="Primary" Variant="Primary" /><Button Title="Secondary" Variant="Secondary" /><Button Title="Destructive" Variant="Destructive" /></StackLayout>""",
        ["Card"] = """<Card><Badge Title="Content inside a card" /></Card>""",
        ["Nested Layout"] = """<StackLayout Orientation="Vertical" Gap="4"><StackLayout Orientation="Horizontal" Gap="2"><Badge Title="A" /><Badge Title="B" /></StackLayout><StackLayout Orientation="Horizontal" Gap="2"><Badge Title="C" Variant="Destructive" /><Badge Title="D" Variant="Success" /></StackLayout></StackLayout>""",
    };

    private static readonly string[] ExampleNames = [.. Examples.Keys];

    private const string DefaultXaml = """<StackLayout Orientation="Vertical"><Badge Title="Hello" /><Badge Title="World" Variant="Success" /></StackLayout>""";

    public override object? Build()
    {
        var xml = UseState(DefaultXaml);
        var selectedExample = UseState("Simple Layout");

        UseEffect(() => xml.Set(Examples[selectedExample.Value]), selectedExample);

        object preview;
        try
        {
            if (string.IsNullOrWhiteSpace(xml.Value))
            {
                preview = Text.Muted("Enter XML to see a preview");
            }
            else
            {
                var builder = new XamlBuilder();
                preview = builder.Build(xml.Value);
            }
        }
        catch (Exception ex)
        {
            preview = Callout.Error(ex.Message);
        }

        return Layout.Horizontal().Gap(4)
               | (Layout.Vertical().Gap(2).Width(Size.Half())
                  | selectedExample.ToSelectInput(ExampleNames)
                  | xml.ToCodeInput().Language(Languages.Xml).Height(Size.Full()))
               | (Layout.Vertical().Width(Size.Half())
                  | preview);
    }
}
