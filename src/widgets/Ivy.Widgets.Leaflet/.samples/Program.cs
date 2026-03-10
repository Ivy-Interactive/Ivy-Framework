using Ivy;
using Ivy.Widgets.Leaflet;

var server = new Server();
server.AddApp<MapView>();
await server.RunAsync();

[App]
class MapView : ViewBase
{
    public override object Build() =>
        new Map()
            .Center(51.505, -0.09)
            .Zoom(13)
            .Marker("m1", 51.505, -0.09, popup: "Hello London!")
            .Marker("m2", 51.51, -0.1, popup: "Another spot", draggable: true)
            .Circle("c1", 51.508, -0.11, 200, "#ff6b6b");
}
