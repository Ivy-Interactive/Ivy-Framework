import React, { useEffect, useRef } from "react";
import {
  MapContainer,
  TileLayer,
  Marker,
  Popup,
  Tooltip,
  Polyline,
  Polygon,
  Circle,
  useMapEvents,
  useMap,
} from "react-leaflet";
import L from "leaflet";
import type { EventHandler, LatLng, MapMarker, MapPolyline, MapPolygon, MapCircle } from "./types";
import { getWidth, getHeight } from "./styles";
import { injectLeafletStyles } from "./leaflet-styles";

// Fix for default marker icons in Leaflet with bundlers
const defaultIcon = L.icon({
  iconUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png",
  iconRetinaUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png",
  shadowUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png",
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41],
});

L.Marker.prototype.options.icon = defaultIcon;

interface MapProps {
  /** Unique widget ID provided by Ivy */
  id: string;
  /** Width prop from Ivy */
  width?: string;
  /** Height prop from Ivy */
  height?: string;
  /** Event handler provided by Ivy */
  eventHandler: EventHandler;
  /** List of registered events */
  events?: string[];
  /** Center coordinates of the map */
  center?: LatLng;
  /** Zoom level (0-18) */
  zoom?: number;
  /** Minimum zoom level */
  minZoom?: number;
  /** Maximum zoom level */
  maxZoom?: number;
  /** Markers to display */
  markers?: MapMarker[];
  /** Polylines to display */
  polylines?: MapPolyline[];
  /** Polygons to display */
  polygons?: MapPolygon[];
  /** Circles to display */
  circles?: MapCircle[];
  /** Tile layer URL template */
  tileUrl?: string;
  /** Tile layer attribution */
  tileAttribution?: string;
  /** Whether to show zoom control */
  zoomControl?: boolean;
  /** Whether scroll wheel zoom is enabled */
  scrollWheelZoom?: boolean;
  /** Whether double-click zoom is enabled */
  doubleClickZoom?: boolean;
  /** Whether dragging is enabled */
  dragging?: boolean;
}

/**
 * Internal component to handle map events.
 */
const MapEventHandler: React.FC<{
  id: string;
  eventHandler: EventHandler;
  events: string[];
}> = ({ id, eventHandler, events }) => {
  useMapEvents({
    click: (e) => {
      if (events.includes("OnMapClick")) {
        eventHandler("OnMapClick", id, [{ position: { lat: e.latlng.lat, lng: e.latlng.lng } }]);
      }
    },
    zoomend: (e) => {
      if (events.includes("OnZoomChange")) {
        const map = e.target;
        eventHandler("OnZoomChange", id, [{ zoom: map.getZoom() }]);
      }
    },
    moveend: (e) => {
      const map = e.target;
      const center = map.getCenter();
      const bounds = map.getBounds();
      if (events.includes("OnCenterChange")) {
        eventHandler("OnCenterChange", id, [{ center: { lat: center.lat, lng: center.lng } }]);
      }
      if (events.includes("OnBoundsChange")) {
        eventHandler("OnBoundsChange", id, [
          {
            southWest: { lat: bounds.getSouthWest().lat, lng: bounds.getSouthWest().lng },
            northEast: { lat: bounds.getNorthEast().lat, lng: bounds.getNorthEast().lng },
          },
        ]);
      }
    },
  });

  return null;
};

/**
 * Component to sync external center/zoom changes to the map.
 */
const MapSync: React.FC<{
  center?: LatLng;
  zoom?: number;
}> = ({ center, zoom }) => {
  const map = useMap();
  const prevCenter = useRef(center);
  const prevZoom = useRef(zoom);

  useEffect(() => {
    if (
      center &&
      zoom !== undefined &&
      (prevCenter.current?.lat !== center.lat ||
        prevCenter.current?.lng !== center.lng ||
        prevZoom.current !== zoom)
    ) {
      map.setView([center.lat, center.lng], zoom);
      prevCenter.current = center;
      prevZoom.current = zoom;
    }
  }, [map, center, zoom]);

  return null;
};

/**
 * A draggable marker that emits drag events.
 */
const DraggableMarker: React.FC<{
  marker: MapMarker;
  widgetId: string;
  eventHandler: EventHandler;
  events: string[];
}> = ({ marker, widgetId, eventHandler, events }) => {
  const markerRef = useRef<L.Marker>(null);

  const handleClick = () => {
    if (events.includes("OnMarkerClick")) {
      eventHandler("OnMarkerClick", widgetId, [
        {
          markerId: marker.id,
          position: { lat: marker.position.lat, lng: marker.position.lng },
        },
      ]);
    }
  };

  const handleDragEnd = () => {
    if (events.includes("OnMarkerDrag")) {
      const m = markerRef.current;
      if (m) {
        const newPos = m.getLatLng();
        eventHandler("OnMarkerDrag", widgetId, [
          {
            markerId: marker.id,
            oldPosition: { lat: marker.position.lat, lng: marker.position.lng },
            newPosition: { lat: newPos.lat, lng: newPos.lng },
          },
        ]);
      }
    }
  };

  return (
    <Marker
      ref={markerRef}
      position={[marker.position.lat, marker.position.lng]}
      draggable={marker.draggable}
      eventHandlers={{
        click: handleClick,
        dragend: handleDragEnd,
      }}
    >
      {marker.popup && <Popup>{marker.popup}</Popup>}
      {marker.tooltip && <Tooltip>{marker.tooltip}</Tooltip>}
    </Marker>
  );
};

export const Map: React.FC<MapProps> = ({
  id,
  width = "Full",
  height = "Full",
  eventHandler,
  events = [],
  center = { lat: 51.505, lng: -0.09 },
  zoom = 13,
  minZoom = 0,
  maxZoom = 18,
  markers = [],
  polylines = [],
  polygons = [],
  circles = [],
  tileUrl = "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
  tileAttribution = '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
  zoomControl = true,
  scrollWheelZoom = true,
  doubleClickZoom = true,
  dragging = true,
}) => {
  // Inject Leaflet CSS on component mount
  useEffect(() => {
    injectLeafletStyles();
  }, []);

  const containerStyle: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  // If no height specified, use a default height
  const mapStyle: React.CSSProperties = {
    width: "100%",
    height: height ? "100%" : "400px",
  };

  return (
    <div style={containerStyle} className="ivy-leaflet-map">
      <MapContainer
        center={[center.lat, center.lng]}
        zoom={zoom}
        minZoom={minZoom}
        maxZoom={maxZoom}
        zoomControl={zoomControl}
        scrollWheelZoom={scrollWheelZoom}
        doubleClickZoom={doubleClickZoom}
        dragging={dragging}
        style={mapStyle}
      >
        <TileLayer url={tileUrl} attribution={tileAttribution} />

        <MapEventHandler id={id} eventHandler={eventHandler} events={events} />
        <MapSync center={center} zoom={zoom} />

        {/* Render markers */}
        {markers.map((marker) => (
          <DraggableMarker
            key={marker.id}
            marker={marker}
            widgetId={id}
            eventHandler={eventHandler}
            events={events}
          />
        ))}

        {/* Render polylines */}
        {polylines.map((polyline) => (
          <Polyline
            key={polyline.id}
            positions={polyline.positions.map((p) => [p.lat, p.lng] as [number, number])}
            pathOptions={{
              color: polyline.color,
              weight: polyline.weight,
              opacity: polyline.opacity,
            }}
          />
        ))}

        {/* Render polygons */}
        {polygons.map((polygon) => (
          <Polygon
            key={polygon.id}
            positions={polygon.positions.map((p) => [p.lat, p.lng] as [number, number])}
            pathOptions={{
              color: polygon.color,
              fillColor: polygon.fillColor,
              fillOpacity: polygon.fillOpacity,
              weight: polygon.weight,
            }}
          />
        ))}

        {/* Render circles */}
        {circles.map((circle) => (
          <Circle
            key={circle.id}
            center={[circle.center.lat, circle.center.lng]}
            radius={circle.radius}
            pathOptions={{
              color: circle.color,
              fillColor: circle.fillColor,
              fillOpacity: circle.fillOpacity,
            }}
          />
        ))}
      </MapContainer>
    </div>
  );
};
