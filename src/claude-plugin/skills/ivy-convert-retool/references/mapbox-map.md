# Mapbox Map

An embedded map using [Mapbox](https://www.mapbox.com/) to display latitude and longitude coordinates as points of interest. Supports custom markers using emoji and symbols, default coordinates and zoom level, and GeoJSON to render custom shapes for highlighting locations. Triggers queries on pan, zoom, and point hover/select events.

## Retool

```toolscript
// Mapbox Map configured in the inspector
mapboxMap1.latitude = 37.7749
mapboxMap1.longitude = -122.4194
mapboxMap1.zoom = 12
mapboxMap1.points = [
  { id: "1", latitude: 37.7749, longitude: -122.4194, color: "red" },
  { id: "2", latitude: 37.7849, longitude: -122.4094, color: "blue" }
]
mapboxMap1.geoJson = {
  type: "FeatureCollection",
  features: [{
    type: "Feature",
    geometry: {
      type: "Polygon",
      coordinates: [[[-122.5, 37.7], [-122.3, 37.7], [-122.3, 37.8], [-122.5, 37.8], [-122.5, 37.7]]]
    },
    properties: { name: "SF Area" }
  }]
}
mapboxMap1.fitBoundsToGeoJson = true

// Select a point programmatically
mapboxMap1.selectPoint(37.7749, -122.4194)

// Set map center
mapboxMap1.setMapCenter({ latitude: 37.7749, longitude: -122.4194 })

// Read state
mapboxMap1.selectedPoint
mapboxMap1.hoveredOverPoint
mapboxMap1.visiblePoints

// Scroll into view
mapboxMap1.scrollIntoView({ behavior: "smooth", block: "center" })
```

## Ivy

Ivy does not have a native map widget. Use an `Iframe` to embed a Mapbox GL JS map, or build a custom `ExternalWidget` wrapping a React map component.

```csharp
// Simple embed via Iframe
new Iframe("https://api.mapbox.com/styles/v1/mapbox/streets-v12.html?access_token=YOUR_TOKEN#12/37.7749/-122.4194")
    .Width(Size.Full())
    .Height(Size.Units(120));

// For full interactivity, use an ExternalWidget with a React map library
[ExternalWidget(
    "frontend/dist/MapWidget.js",
    StylePath = "frontend/dist/style.css",
    ExportName = "MapWidget",
    GlobalName = "MyApp_Widgets_MapWidget")]
public record MapWidget : WidgetBase<MapWidget>
{
    [Prop] public double Latitude { get; set; }
    [Prop] public double Longitude { get; set; }
    [Prop] public int Zoom { get; set; }
    [Prop] public string? GeoJson { get; set; }
    [Event] public Func<Event<MapWidget>, ValueTask>? OnPointSelected { get; set; }
    [Event] public Func<Event<MapWidget>, ValueTask>? OnViewportChange { get; set; }
}
```

## Parameters

| Parameter              | Documentation                                                  | Ivy                                                          |
|------------------------|----------------------------------------------------------------|--------------------------------------------------------------|
| `latitude`             | The latitudinal position of the map center.                    | Not supported (use Iframe URL or ExternalWidget `[Prop]`)    |
| `longitude`            | The longitudinal position of the map center.                   | Not supported (use Iframe URL or ExternalWidget `[Prop]`)    |
| `zoom`                 | The zoom level, from 0 to 16.                                  | Not supported (use Iframe URL or ExternalWidget `[Prop]`)    |
| `points`               | A list of map points (id, latitude, longitude, color).         | Not supported (requires ExternalWidget)                      |
| `geoJson`              | GeoJSON objects to display on the map, such as polygons.       | Not supported (requires ExternalWidget)                      |
| `geoJsonLayerStyle`    | Mapbox GeoJSON layer styles that override other styling.       | Not supported (requires ExternalWidget)                      |
| `fitBoundsToGeoJson`   | Whether to fit the map bounds to the GeoJSON data.             | Not supported (requires ExternalWidget)                      |
| `latitudeColumnName`   | The key for latitude values if different from `latitude`.      | Not supported                                                |
| `longitudeColumnName`  | The key for longitude values if different from `longitude`.    | Not supported                                                |
| `showCurrentLngLat`    | Whether to show latitude/longitude coordinates on the map.     | Not supported                                                |
| `selectedPoint`        | The currently selected point (read-only).                      | Not supported (requires ExternalWidget)                      |
| `hoveredOverPoint`     | The point on which the cursor hovers (read-only).              | Not supported (requires ExternalWidget)                      |
| `visiblePoints`        | A list of map points currently visible (read-only).            | Not supported (requires ExternalWidget)                      |
| `hidden`               | Whether the component is hidden from view.                     | `.Visible(bool)` on Iframe                                   |
| `isHiddenOnDesktop`    | Whether to hide in the desktop layout.                         | Not supported                                                |
| `isHiddenOnMobile`     | Whether to hide in the mobile layout.                          | Not supported                                                |
| `maintainSpaceWhenHidden` | Whether to take up space on the canvas when hidden.         | Not supported                                                |
| `id`                   | The unique identifier (name).                                  | Variable name in C#                                          |
| `margin`               | The amount of margin (Normal or None).                         | Not supported (use layout containers)                        |
| `style`                | Custom style options.                                          | `.Width()`, `.Height()`, `.Scale()` on Iframe                |

### Methods

| Method               | Documentation                                                    | Ivy                                               |
|----------------------|------------------------------------------------------------------|----------------------------------------------------|
| `selectPoint()`      | Select a point on the map by latitude and longitude.             | Not supported (requires ExternalWidget)            |
| `setMapCenter()`     | Set the center of the map to given coordinates.                  | Not supported (requires ExternalWidget)            |
| `scrollIntoView()`   | Scroll the canvas so the component appears in the visible area.  | Not supported                                      |

### Events

| Event                     | Documentation                              | Ivy                                                    |
|---------------------------|--------------------------------------------|---------------------------------------------------------|
| On Point Selected         | A point of interest is selected.           | Not supported (requires ExternalWidget `[Event]`)       |
| On Point Hovered Over     | A point of interest is hovered over.       | Not supported (requires ExternalWidget `[Event]`)       |
| On Point Hovered Over End | A point is no longer hovered over.         | Not supported (requires ExternalWidget `[Event]`)       |
| On Viewport Change        | The map viewport is changed.               | Not supported (requires ExternalWidget `[Event]`)       |
