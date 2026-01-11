using Ivy;
using Ivy.Core;
using Ivy.Core.ExternalWidgets;

namespace Ivy.Widgets.Leaflet;

/// <summary>
/// An interactive map widget powered by Leaflet.
/// </summary>
[ExternalWidget("frontend/dist/Ivy_Widgets_Leaflet.js", ExportName = "Map")]
public record Map : WidgetBase<Map>
{
    /// <summary>
    /// Creates a new Map widget with optional initial center and zoom.
    /// </summary>
    /// <param name="center">Initial center coordinates.</param>
    /// <param name="zoom">Initial zoom level (0-18).</param>
    public Map(LatLng? center = null, int zoom = 13)
    {
        Center = center ?? new LatLng(51.505, -0.09); // Default to London
        Zoom = zoom;
    }

    internal Map()
    {
        Center = new LatLng(51.505, -0.09);
        Zoom = 13;
    }

    /// <summary>
    /// The center coordinates of the map.
    /// </summary>
    [Prop] public LatLng Center { get; init; }

    /// <summary>
    /// The zoom level of the map (0-18).
    /// </summary>
    [Prop] public int Zoom { get; init; } = 13;

    /// <summary>
    /// Minimum zoom level allowed.
    /// </summary>
    [Prop] public int MinZoom { get; init; } = 0;

    /// <summary>
    /// Maximum zoom level allowed.
    /// </summary>
    [Prop] public int MaxZoom { get; init; } = 18;

    /// <summary>
    /// Markers to display on the map.
    /// </summary>
    [Prop] public MapMarker[] Markers { get; init; } = [];

    /// <summary>
    /// Polylines to display on the map.
    /// </summary>
    [Prop] public MapPolyline[] Polylines { get; init; } = [];

    /// <summary>
    /// Polygons to display on the map.
    /// </summary>
    [Prop] public MapPolygon[] Polygons { get; init; } = [];

    /// <summary>
    /// Circles to display on the map.
    /// </summary>
    [Prop] public MapCircle[] Circles { get; init; } = [];

    /// <summary>
    /// Tile layer URL template. Defaults to OpenStreetMap.
    /// </summary>
    [Prop] public string TileUrl { get; init; } = "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png";

    /// <summary>
    /// Tile layer attribution text.
    /// </summary>
    [Prop] public string TileAttribution { get; init; } = "&copy; <a href=\"https://www.openstreetmap.org/copyright\">OpenStreetMap</a> contributors";

    /// <summary>
    /// Whether zoom control is visible.
    /// </summary>
    [Prop] public bool ZoomControl { get; init; } = true;

    /// <summary>
    /// Whether scroll wheel zoom is enabled.
    /// </summary>
    [Prop] public bool ScrollWheelZoom { get; init; } = true;

    /// <summary>
    /// Whether double-click zoom is enabled.
    /// </summary>
    [Prop] public bool DoubleClickZoom { get; init; } = true;

    /// <summary>
    /// Whether dragging is enabled.
    /// </summary>
    [Prop] public bool Dragging { get; init; } = true;

    /// <summary>
    /// Event triggered when the map is clicked.
    /// </summary>
    [Event] public Func<Event<Map, MapClickEventArgs>, ValueTask>? OnMapClick { get; init; }

    /// <summary>
    /// Event triggered when a marker is clicked.
    /// </summary>
    [Event] public Func<Event<Map, MarkerClickEventArgs>, ValueTask>? OnMarkerClick { get; init; }

    /// <summary>
    /// Event triggered when a marker is dragged to a new position.
    /// </summary>
    [Event] public Func<Event<Map, MarkerDragEventArgs>, ValueTask>? OnMarkerDrag { get; init; }

    /// <summary>
    /// Event triggered when the map zoom changes.
    /// </summary>
    [Event] public Func<Event<Map, ZoomChangeEventArgs>, ValueTask>? OnZoomChange { get; init; }

    /// <summary>
    /// Event triggered when the map center changes.
    /// </summary>
    [Event] public Func<Event<Map, CenterChangeEventArgs>, ValueTask>? OnCenterChange { get; init; }

    /// <summary>
    /// Event triggered when the map bounds change (after pan or zoom).
    /// </summary>
    [Event] public Func<Event<Map, BoundsChangeEventArgs>, ValueTask>? OnBoundsChange { get; init; }
}

/// <summary>
/// Geographic coordinates (latitude and longitude).
/// </summary>
public record LatLng(double Lat, double Lng);

/// <summary>
/// A marker on the map.
/// </summary>
public record MapMarker
{
    /// <summary>
    /// Creates a new map marker.
    /// </summary>
    /// <param name="id">Unique identifier for the marker.</param>
    /// <param name="position">Geographic position of the marker.</param>
    public MapMarker(string id, LatLng position)
    {
        Id = id;
        Position = position;
    }

    /// <summary>
    /// Unique identifier for the marker.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Geographic position of the marker.
    /// </summary>
    public LatLng Position { get; init; }

    /// <summary>
    /// Popup content displayed when marker is clicked.
    /// </summary>
    public string? Popup { get; init; }

    /// <summary>
    /// Tooltip content displayed on hover.
    /// </summary>
    public string? Tooltip { get; init; }

    /// <summary>
    /// Whether the marker can be dragged.
    /// </summary>
    public bool Draggable { get; init; } = false;
}

/// <summary>
/// A polyline (series of connected line segments) on the map.
/// </summary>
public record MapPolyline
{
    /// <summary>
    /// Creates a new polyline.
    /// </summary>
    /// <param name="id">Unique identifier for the polyline.</param>
    /// <param name="positions">Array of coordinates defining the line.</param>
    public MapPolyline(string id, params LatLng[] positions)
    {
        Id = id;
        Positions = positions;
    }

    /// <summary>
    /// Unique identifier for the polyline.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Array of coordinates defining the line.
    /// </summary>
    public LatLng[] Positions { get; init; }

    /// <summary>
    /// Stroke color (CSS color string).
    /// </summary>
    public string Color { get; init; } = "#3388ff";

    /// <summary>
    /// Stroke width in pixels.
    /// </summary>
    public int Weight { get; init; } = 3;

    /// <summary>
    /// Stroke opacity (0-1).
    /// </summary>
    public double Opacity { get; init; } = 1.0;
}

/// <summary>
/// A polygon on the map.
/// </summary>
public record MapPolygon
{
    /// <summary>
    /// Creates a new polygon.
    /// </summary>
    /// <param name="id">Unique identifier for the polygon.</param>
    /// <param name="positions">Array of coordinates defining the polygon vertices.</param>
    public MapPolygon(string id, params LatLng[] positions)
    {
        Id = id;
        Positions = positions;
    }

    /// <summary>
    /// Unique identifier for the polygon.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Array of coordinates defining the polygon vertices.
    /// </summary>
    public LatLng[] Positions { get; init; }

    /// <summary>
    /// Stroke color (CSS color string).
    /// </summary>
    public string Color { get; init; } = "#3388ff";

    /// <summary>
    /// Fill color (CSS color string).
    /// </summary>
    public string FillColor { get; init; } = "#3388ff";

    /// <summary>
    /// Fill opacity (0-1).
    /// </summary>
    public double FillOpacity { get; init; } = 0.2;

    /// <summary>
    /// Stroke width in pixels.
    /// </summary>
    public int Weight { get; init; } = 3;
}

/// <summary>
/// A circle on the map.
/// </summary>
public record MapCircle
{
    /// <summary>
    /// Creates a new circle.
    /// </summary>
    /// <param name="id">Unique identifier for the circle.</param>
    /// <param name="center">Center coordinates of the circle.</param>
    /// <param name="radius">Radius in meters.</param>
    public MapCircle(string id, LatLng center, double radius)
    {
        Id = id;
        Center = center;
        Radius = radius;
    }

    /// <summary>
    /// Unique identifier for the circle.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Center coordinates of the circle.
    /// </summary>
    public LatLng Center { get; init; }

    /// <summary>
    /// Radius in meters.
    /// </summary>
    public double Radius { get; init; }

    /// <summary>
    /// Stroke color (CSS color string).
    /// </summary>
    public string Color { get; init; } = "#3388ff";

    /// <summary>
    /// Fill color (CSS color string).
    /// </summary>
    public string FillColor { get; init; } = "#3388ff";

    /// <summary>
    /// Fill opacity (0-1).
    /// </summary>
    public double FillOpacity { get; init; } = 0.2;
}

/// <summary>
/// Event arguments for map click events.
/// </summary>
public record MapClickEventArgs(LatLng Position);

/// <summary>
/// Event arguments for marker click events.
/// </summary>
public record MarkerClickEventArgs(string MarkerId, LatLng Position);

/// <summary>
/// Event arguments for marker drag events.
/// </summary>
public record MarkerDragEventArgs(string MarkerId, LatLng OldPosition, LatLng NewPosition);

/// <summary>
/// Event arguments for zoom change events.
/// </summary>
public record ZoomChangeEventArgs(int Zoom);

/// <summary>
/// Event arguments for center change events.
/// </summary>
public record CenterChangeEventArgs(LatLng Center);

/// <summary>
/// Event arguments for bounds change events.
/// </summary>
public record BoundsChangeEventArgs(LatLng SouthWest, LatLng NorthEast);

/// <summary>
/// Extension methods for the Map widget.
/// </summary>
public static class MapExtensions
{
    /// <summary>
    /// Sets the center coordinates of the map.
    /// </summary>
    public static Map Center(this Map map, LatLng center) =>
        map with { Center = center };

    /// <summary>
    /// Sets the center coordinates of the map.
    /// </summary>
    public static Map Center(this Map map, double lat, double lng) =>
        map with { Center = new LatLng(lat, lng) };

    /// <summary>
    /// Sets the zoom level of the map.
    /// </summary>
    public static Map Zoom(this Map map, int zoom) =>
        map with { Zoom = zoom };

    /// <summary>
    /// Sets the minimum and maximum zoom levels.
    /// </summary>
    public static Map ZoomRange(this Map map, int min, int max) =>
        map with { MinZoom = min, MaxZoom = max };

    /// <summary>
    /// Adds a marker to the map.
    /// </summary>
    public static Map Marker(this Map map, MapMarker marker) =>
        map with { Markers = [.. map.Markers, marker] };

    /// <summary>
    /// Adds a marker to the map at the specified position.
    /// </summary>
    public static Map Marker(this Map map, string id, double lat, double lng, string? popup = null, string? tooltip = null, bool draggable = false) =>
        map with { Markers = [.. map.Markers, new MapMarker(id, new LatLng(lat, lng)) { Popup = popup, Tooltip = tooltip, Draggable = draggable }] };

    /// <summary>
    /// Adds multiple markers to the map.
    /// </summary>
    public static Map Markers(this Map map, params MapMarker[] markers) =>
        map with { Markers = [.. map.Markers, .. markers] };

    /// <summary>
    /// Adds a polyline to the map.
    /// </summary>
    public static Map Polyline(this Map map, MapPolyline polyline) =>
        map with { Polylines = [.. map.Polylines, polyline] };

    /// <summary>
    /// Adds a polyline to the map with the specified points.
    /// </summary>
    public static Map Polyline(this Map map, string id, string color, params LatLng[] positions) =>
        map with { Polylines = [.. map.Polylines, new MapPolyline(id, positions) { Color = color }] };

    /// <summary>
    /// Adds a polygon to the map.
    /// </summary>
    public static Map Polygon(this Map map, MapPolygon polygon) =>
        map with { Polygons = [.. map.Polygons, polygon] };

    /// <summary>
    /// Adds a circle to the map.
    /// </summary>
    public static Map Circle(this Map map, MapCircle circle) =>
        map with { Circles = [.. map.Circles, circle] };

    /// <summary>
    /// Adds a circle to the map at the specified position.
    /// </summary>
    public static Map Circle(this Map map, string id, double lat, double lng, double radius, string? color = null) =>
        map with { Circles = [.. map.Circles, new MapCircle(id, new LatLng(lat, lng), radius) { Color = color ?? "#3388ff", FillColor = color ?? "#3388ff" }] };

    /// <summary>
    /// Sets the tile layer URL template.
    /// </summary>
    public static Map TileLayer(this Map map, string url, string? attribution = null) =>
        map with { TileUrl = url, TileAttribution = attribution ?? map.TileAttribution };

    /// <summary>
    /// Enables or disables zoom control.
    /// </summary>
    public static Map ShowZoomControl(this Map map, bool show = true) =>
        map with { ZoomControl = show };

    /// <summary>
    /// Enables or disables scroll wheel zoom.
    /// </summary>
    public static Map EnableScrollWheelZoom(this Map map, bool enable = true) =>
        map with { ScrollWheelZoom = enable };

    /// <summary>
    /// Enables or disables double-click zoom.
    /// </summary>
    public static Map EnableDoubleClickZoom(this Map map, bool enable = true) =>
        map with { DoubleClickZoom = enable };

    /// <summary>
    /// Enables or disables map dragging.
    /// </summary>
    public static Map EnableDragging(this Map map, bool enable = true) =>
        map with { Dragging = enable };

    /// <summary>
    /// Handles map click events.
    /// </summary>
    public static Map HandleMapClick(this Map map, Func<Event<Map, MapClickEventArgs>, ValueTask> handler) =>
        map with { OnMapClick = handler };

    /// <summary>
    /// Handles map click events.
    /// </summary>
    public static Map HandleMapClick(this Map map, Action<MapClickEventArgs> handler) =>
        map with { OnMapClick = e => { handler(e.Value); return ValueTask.CompletedTask; } };

    /// <summary>
    /// Handles map click events with position.
    /// </summary>
    public static Map HandleMapClick(this Map map, Action<LatLng> handler) =>
        map with { OnMapClick = e => { handler(e.Value.Position); return ValueTask.CompletedTask; } };

    /// <summary>
    /// Handles marker click events.
    /// </summary>
    public static Map HandleMarkerClick(this Map map, Func<Event<Map, MarkerClickEventArgs>, ValueTask> handler) =>
        map with { OnMarkerClick = handler };

    /// <summary>
    /// Handles marker click events.
    /// </summary>
    public static Map HandleMarkerClick(this Map map, Action<MarkerClickEventArgs> handler) =>
        map with { OnMarkerClick = e => { handler(e.Value); return ValueTask.CompletedTask; } };

    /// <summary>
    /// Handles marker click events with marker ID.
    /// </summary>
    public static Map HandleMarkerClick(this Map map, Action<string> handler) =>
        map with { OnMarkerClick = e => { handler(e.Value.MarkerId); return ValueTask.CompletedTask; } };

    /// <summary>
    /// Handles marker drag events.
    /// </summary>
    public static Map HandleMarkerDrag(this Map map, Func<Event<Map, MarkerDragEventArgs>, ValueTask> handler) =>
        map with { OnMarkerDrag = handler };

    /// <summary>
    /// Handles marker drag events.
    /// </summary>
    public static Map HandleMarkerDrag(this Map map, Action<MarkerDragEventArgs> handler) =>
        map with { OnMarkerDrag = e => { handler(e.Value); return ValueTask.CompletedTask; } };

    /// <summary>
    /// Handles zoom change events.
    /// </summary>
    public static Map HandleZoomChange(this Map map, Func<Event<Map, ZoomChangeEventArgs>, ValueTask> handler) =>
        map with { OnZoomChange = handler };

    /// <summary>
    /// Handles zoom change events.
    /// </summary>
    public static Map HandleZoomChange(this Map map, Action<int> handler) =>
        map with { OnZoomChange = e => { handler(e.Value.Zoom); return ValueTask.CompletedTask; } };

    /// <summary>
    /// Handles center change events.
    /// </summary>
    public static Map HandleCenterChange(this Map map, Func<Event<Map, CenterChangeEventArgs>, ValueTask> handler) =>
        map with { OnCenterChange = handler };

    /// <summary>
    /// Handles center change events.
    /// </summary>
    public static Map HandleCenterChange(this Map map, Action<LatLng> handler) =>
        map with { OnCenterChange = e => { handler(e.Value.Center); return ValueTask.CompletedTask; } };

    /// <summary>
    /// Handles bounds change events.
    /// </summary>
    public static Map HandleBoundsChange(this Map map, Func<Event<Map, BoundsChangeEventArgs>, ValueTask> handler) =>
        map with { OnBoundsChange = handler };

    /// <summary>
    /// Handles bounds change events.
    /// </summary>
    public static Map HandleBoundsChange(this Map map, Action<BoundsChangeEventArgs> handler) =>
        map with { OnBoundsChange = e => { handler(e.Value); return ValueTask.CompletedTask; } };
}
