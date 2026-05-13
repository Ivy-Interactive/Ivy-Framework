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
        var state = UseState("https://ivy-interactive.com");
        return Layout.Vertical().Children(
            new QRCode().Value(state.Value).PixelSize(256),
            Input.Text(state, "URL")
        );
    }
}
