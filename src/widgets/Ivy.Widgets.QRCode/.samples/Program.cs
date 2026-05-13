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
            | new QRCode()
                .Value(state.Value)
                .PixelSize(256)
                .Foreground(Colors.Primary)
                .Background(Colors.White)
            | state.ToTextInput();
    }}
}}")
            | (Layout.Vertical()
                | new QRCode()
                    .Value(state.Value)
                    .PixelSize(256)
                    .Foreground(Colors.Primary)
                    .Background(Colors.White)
                    .AlignSelf(Align.Center)
                | state.ToTextInput())
            .WithBox();
    }
}
