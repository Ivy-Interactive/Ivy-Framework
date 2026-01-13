/**
 * Event handler type provided by Ivy to external widgets.
 */
export type IvyEventHandler = (
  eventName: string,
  widgetId: string,
  args: unknown[]
) => void;

/**
 * Geographic coordinates (latitude, longitude).
 */
export interface LatLng {
  lat: number;
  lng: number;
}

/**
 * A marker on the map.
 */
export interface MapMarker {
  id: string;
  position: LatLng;
  popup?: string;
  tooltip?: string;
  draggable?: boolean;
}

/**
 * A polyline on the map.
 */
export interface MapPolyline {
  id: string;
  positions: LatLng[];
  color?: string;
  weight?: number;
  opacity?: number;
}

/**
 * A polygon on the map.
 */
export interface MapPolygon {
  id: string;
  positions: LatLng[];
  color?: string;
  fillColor?: string;
  fillOpacity?: number;
  weight?: number;
}

/**
 * A circle on the map.
 */
export interface MapCircle {
  id: string;
  center: LatLng;
  radius: number;
  color?: string;
  fillColor?: string;
  fillOpacity?: number;
}

/**
 * Tile layer configuration.
 */
export interface TileLayerConfig {
  url: string;
  attribution?: string;
  maxZoom?: number;
  minZoom?: number;
}

/**
 * Map bounds (southwest and northeast corners).
 */
export interface MapBounds {
  southWest: LatLng;
  northEast: LatLng;
}
