namespace Ivy.Widgets.Leaflet;

[ExternalWidget("frontend/dist/Ivy_Widgets_Leaflet.js", ExportName = "Map")]
public record Map : WidgetBase<Map>
{
    public Map(LatLng? center = null, int zoom = 13)
    {
        Center = center ?? new LatLng(51.505, -0.09); // Default to London
        Zoom = zoom;
        Width = Size.Full();
        Height = Size.Full();
    }

    internal Map()
    {
        Center = new LatLng(51.505, -0.09);
        Zoom = 13;
        Width = Size.Full();
        Height = Size.Full();
    }

    [Prop] public LatLng Center { get; init; }

    [Prop] public int Zoom { get; init; } = 13;

    [Prop] public int MinZoom { get; init; } = 0;

    [Prop] public int MaxZoom { get; init; } = 18;

    [Prop] public MapMarker[] Markers { get; init; } = [];

    [Prop] public MapPolyline[] Polylines { get; init; } = [];

    [Prop] public MapPolygon[] Polygons { get; init; } = [];

    [Prop] public MapCircle[] Circles { get; init; } = [];

    [Prop] public string TileUrl { get; init; } = "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png";

    [Prop] public string TileAttribution { get; init; } = "&copy; <a href=\"https://www.openstreetmap.org/copyright\">OpenStreetMap</a> contributors";

    [Prop] public bool ZoomControl { get; init; } = true;

    [Prop] public bool ScrollWheelZoom { get; init; } = true;

    [Prop] public bool DoubleClickZoom { get; init; } = true;

    [Prop] public bool Dragging { get; init; } = true;

    [Event] public Func<Event<Map, MapClickEventArgs>, ValueTask>? OnMapClick { get; init; }

    [Event] public Func<Event<Map, MarkerClickEventArgs>, ValueTask>? OnMarkerClick { get; init; }

    [Event] public Func<Event<Map, MarkerDragEventArgs>, ValueTask>? OnMarkerDrag { get; init; }

    [Event] public Func<Event<Map, ZoomChangeEventArgs>, ValueTask>? OnZoomChange { get; init; }

    [Event] public Func<Event<Map, CenterChangeEventArgs>, ValueTask>? OnCenterChange { get; init; }

    [Event] public Func<Event<Map, BoundsChangeEventArgs>, ValueTask>? OnBoundsChange { get; init; }
}

public record LatLng(double Lat, double Lng);

public record MapMarker
{
    public MapMarker(string id, LatLng position)
    {
        Id = id;
        Position = position;
    }

    public string Id { get; init; }

    public LatLng Position { get; init; }

    public string? Popup { get; init; }

    public string? Tooltip { get; init; }

    public bool Draggable { get; init; } = false;
}

public record MapPolyline
{
    public MapPolyline(string id, params LatLng[] positions)
    {
        Id = id;
        Positions = positions;
    }

    public string Id { get; init; }

    public LatLng[] Positions { get; init; }

    public string Color { get; init; } = "#3388ff";

    public int Weight { get; init; } = 3;

    public double Opacity { get; init; } = 1.0;
}

public record MapPolygon
{
    public MapPolygon(string id, params LatLng[] positions)
    {
        Id = id;
        Positions = positions;
    }

    public string Id { get; init; }

    public LatLng[] Positions { get; init; }

    public string Color { get; init; } = "#3388ff";

    public string FillColor { get; init; } = "#3388ff";

    public double FillOpacity { get; init; } = 0.2;

    public int Weight { get; init; } = 3;
}

public record MapCircle
{
    public MapCircle(string id, LatLng center, double radius)
    {
        Id = id;
        Center = center;
        Radius = radius;
    }

    public string Id { get; init; }

    public LatLng Center { get; init; }

    public double Radius { get; init; }

    public string Color { get; init; } = "#3388ff";

    public string FillColor { get; init; } = "#3388ff";

    public double FillOpacity { get; init; } = 0.2;
}

public record MapClickEventArgs(LatLng Position);

public record MarkerClickEventArgs(string MarkerId, LatLng Position);

public record MarkerDragEventArgs(string MarkerId, LatLng OldPosition, LatLng NewPosition);

public record ZoomChangeEventArgs(int Zoom);

public record CenterChangeEventArgs(LatLng Center);

public record BoundsChangeEventArgs(LatLng SouthWest, LatLng NorthEast);

public static class MapExtensions
{
    public static Map Center(this Map map, LatLng center) =>
        map with { Center = center };

    public static Map Center(this Map map, double lat, double lng) =>
        map with { Center = new LatLng(lat, lng) };

    public static Map Zoom(this Map map, int zoom) =>
        map with { Zoom = zoom };

    public static Map ZoomRange(this Map map, int min, int max) =>
        map with { MinZoom = min, MaxZoom = max };

    public static Map Marker(this Map map, MapMarker marker) =>
        map with { Markers = [.. map.Markers, marker] };

    public static Map Marker(this Map map, string id, double lat, double lng, string? popup = null,
        string? tooltip = null, bool draggable = false) =>
        map with
        {
            Markers =
            [
                .. map.Markers,
                new MapMarker(id, new LatLng(lat, lng)) { Popup = popup, Tooltip = tooltip, Draggable = draggable }
            ]
        };

    public static Map Markers(this Map map, params MapMarker[] markers) =>
        map with { Markers = [.. map.Markers, .. markers] };

    public static Map Polyline(this Map map, MapPolyline polyline) =>
        map with { Polylines = [.. map.Polylines, polyline] };

    public static Map Polyline(this Map map, string id, string color, params LatLng[] positions) =>
        map with { Polylines = [.. map.Polylines, new MapPolyline(id, positions) { Color = color }] };

    public static Map Polygon(this Map map, MapPolygon polygon) =>
        map with { Polygons = [.. map.Polygons, polygon] };

    public static Map Circle(this Map map, MapCircle circle) =>
        map with { Circles = [.. map.Circles, circle] };

    public static Map Circle(this Map map, string id, double lat, double lng, double radius, string? color = null) =>
        map with
        {
            Circles =
            [
                .. map.Circles,
                new MapCircle(id, new LatLng(lat, lng), radius)
                    { Color = color ?? "#3388ff", FillColor = color ?? "#3388ff" }
            ]
        };

    public static Map TileLayer(this Map map, string url, string? attribution = null) =>
        map with { TileUrl = url, TileAttribution = attribution ?? map.TileAttribution };

    public static Map ShowZoomControl(this Map map, bool show = true) =>
        map with { ZoomControl = show };

    public static Map EnableScrollWheelZoom(this Map map, bool enable = true) =>
        map with { ScrollWheelZoom = enable };

    public static Map EnableDoubleClickZoom(this Map map, bool enable = true) =>
        map with { DoubleClickZoom = enable };

    public static Map EnableDragging(this Map map, bool enable = true) =>
        map with { Dragging = enable };

    public static Map OnMapClick(this Map map, Func<Event<Map, MapClickEventArgs>, ValueTask> handler) =>
        map with { OnMapClick = handler };

    public static Map OnMapClick(this Map map, Action<MapClickEventArgs> handler) =>
        map with
        {
            OnMapClick = e =>
            {
                handler(e.Value);
                return ValueTask.CompletedTask;
            }
        };

    public static Map OnMapClick(this Map map, Action<LatLng> handler) =>
        map with
        {
            OnMapClick = e =>
            {
                handler(e.Value.Position);
                return ValueTask.CompletedTask;
            }
        };

    public static Map OnMarkerClick(this Map map, Func<Event<Map, MarkerClickEventArgs>, ValueTask> handler) =>
        map with { OnMarkerClick = handler };

    public static Map OnMarkerClick(this Map map, Action<MarkerClickEventArgs> handler) =>
        map with
        {
            OnMarkerClick = e =>
            {
                handler(e.Value);
                return ValueTask.CompletedTask;
            }
        };

    public static Map OnMarkerClick(this Map map, Action<string> handler) =>
        map with
        {
            OnMarkerClick = e =>
            {
                handler(e.Value.MarkerId);
                return ValueTask.CompletedTask;
            }
        };

    public static Map OnMarkerDrag(this Map map, Func<Event<Map, MarkerDragEventArgs>, ValueTask> handler) =>
        map with { OnMarkerDrag = handler };

    public static Map OnMarkerDrag(this Map map, Action<MarkerDragEventArgs> handler) =>
        map with
        {
            OnMarkerDrag = e =>
            {
                handler(e.Value);
                return ValueTask.CompletedTask;
            }
        };

    public static Map OnZoomChange(this Map map, Func<Event<Map, ZoomChangeEventArgs>, ValueTask> handler) =>
        map with { OnZoomChange = handler };

    public static Map OnZoomChange(this Map map, Action<int> handler) =>
        map with
        {
            OnZoomChange = e =>
            {
                handler(e.Value.Zoom);
                return ValueTask.CompletedTask;
            }
        };

    public static Map OnCenterChange(this Map map, Func<Event<Map, CenterChangeEventArgs>, ValueTask> handler) =>
        map with { OnCenterChange = handler };

    public static Map OnCenterChange(this Map map, Action<LatLng> handler) =>
        map with
        {
            OnCenterChange = e =>
            {
                handler(e.Value.Center);
                return ValueTask.CompletedTask;
            }
        };

    public static Map OnBoundsChange(this Map map, Func<Event<Map, BoundsChangeEventArgs>, ValueTask> handler) =>
        map with { OnBoundsChange = handler };

    public static Map OnBoundsChange(this Map map, Action<BoundsChangeEventArgs> handler) =>
        map with
        {
            OnBoundsChange = e =>
            {
                handler(e.Value);
                return ValueTask.CompletedTask;
            }
        };
}
