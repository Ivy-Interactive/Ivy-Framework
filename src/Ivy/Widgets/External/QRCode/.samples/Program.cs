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
        var basicExampleQrContent = UseState("https://tendril.ivy.app/");
        var fullExampleContent = UseState("https://tendril.ivy.app/");
        var pixelSize = UseState(300);
        var foregroundColor = UseState(Colors.Emerald);
        var backgroundColor = UseState(Colors.White);
        var errorCorrectionLevel = UseState(QrErrorCorrectionLevel.Low);

        var fullExampleToolbar = Layout.Horizontal().Gap(2).Width(Size.Fit())
            | foregroundColor.ToColorInput().Variant(ColorInputVariant.SwatchPicker).WithLabel("Foreground")
            | backgroundColor.ToColorInput().Variant(ColorInputVariant.SwatchPicker).WithLabel("Background")
            | pixelSize.ToNumberInput().Min(100).Max(1000).WithLabel("PixelSize")
            | errorCorrectionLevel.ToSelectInput().WithLabel("ErrorCorrectionLevel");

        return Layout.Vertical().Gap(16).Width(Size.Auto())
            | (Layout.Vertical()
                | Text.H1("QRCode")
                | Text.H2("Basic Usage")
                | new CodeBlock(@$"class QRCodeBasicDemo : ViewBase
{{
    public override object Build()
    {{
        var basicExampleQrContent = UseState({"\"https://tendril.ivy.app/\""});

        return Layout.Vertical()
            | new QRCode().Value(basicExampleQrContent.Value)
            | basicExampleQrContent.ToTextInput();
    }}
}}")
                | (Layout.Vertical()
                    | new QRCode()
                        .Value(basicExampleQrContent.Value)
                        .AlignSelf(Align.Center)
                    | basicExampleQrContent.ToTextInput())
                .WithBox())

            | (Layout.Vertical()
                | Text.H2("Full Props Usage")
                | new CodeBlock(@$"class QRCodeFullDemo : ViewBase
{{
    public override object Build()
    {{
        return new QRCode()
            .Value({$"\"{fullExampleContent.Value}\""})
            .PixelSize({pixelSize.Value})
            .Foreground(Colors.{foregroundColor.Value})
            .Background(Colors.{backgroundColor.Value})
            .ErrorCorrectionLevel(QrErrorCorrectionLevel.{errorCorrectionLevel.Value});
    }}
}}")
                | (Layout.Vertical()
                    | new QRCode()
                        .Value(fullExampleContent.Value)
                        .PixelSize(pixelSize.Value)
                        .Foreground(foregroundColor.Value)
                        .Background(backgroundColor.Value)
                        .ErrorCorrectionLevel(errorCorrectionLevel.Value)
                        .AlignSelf(Align.Center)
                    | fullExampleContent.ToTextInput()
                    | fullExampleToolbar)
                .WithBox())

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
