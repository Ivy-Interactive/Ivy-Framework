using Ivy;
using Ivy.Widgets.QRCode;

var server = new Server();
server.AddApp<QRCodeDemo>();
await server.RunAsync();

[App]
class QRCodeDemo : ViewBase
{
    public override object Build()
    {
        var client = UseService<IClientProvider>();
        var state = UseState("https://tendril.ivy.app/");
        return Layout.Vertical().Width(Size.Fit())
            | Text.H1("QRCode")
            | Text.H2("Basic Usage")
            | new CodeBlock(@$"class QRCodeDemo : ViewBase
{{
    public override object Build()
    {{
        var state = UseState({"\"https://tendril.ivy.app/\""});
        return Layout.Vertical()
            | new QRCode().Value(state.Value)
            | state.ToTextInput();
    }}
}}")
            | (Layout.Vertical()
                | new QRCode()
                    .Value(state.Value)
                    .AlignSelf(Align.Center)
                | state.ToTextInput())
            .WithBox()

            | new DropDownMenu(@evt =>
                {
                    ThemeMode selectedTheme = @evt.Value switch
                    {
                        "Light" => ThemeMode.Light,
                        "Dark" => ThemeMode.Dark,
                        _ => ThemeMode.System,
                    };
                    client.SetThemeMode(selectedTheme);
                },
                new Button("Theme").Variant(ButtonVariant.Link).Icon(Icons.SunMoon),
                MenuItem.Default("Light").Icon(Icons.Sun),
                MenuItem.Default("Dark").Icon(Icons.Moon),
                MenuItem.Default("System").Icon(Icons.Computer));
    }
}
