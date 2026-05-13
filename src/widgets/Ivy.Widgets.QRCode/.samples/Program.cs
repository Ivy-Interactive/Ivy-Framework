using Ivy;
using Ivy.Widgets.QRCode;

var server = new Server();
server.AddApp<QRCodeApp>();
await server.RunAsync();

[App]
class QRCodeApp : ViewBase
{
    public override object Build()
    {
        var state = UseState("https://tendril.ivy.app/");
        return Layout.Vertical()
            | new QRCode().Value(state.Value).PixelSize(256)
            | state.ToTextInput();
    }
}
