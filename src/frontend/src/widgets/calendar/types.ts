import React from "react";

export type CalendarView = "month" | "week" | "day" | "agenda";

export interface CalendarEvent {
  id: string;
  title: string;
  start: Date;
  end: Date;
  color?: string;
  allDay?: boolean;
  widgetId: string;
  content: React.ReactNode;
}

export interface CalendarEventData {
  eventId: string;
  title?: string;
  start?: string;
  end?: string;
  color?: string;
  allDay?: boolean;
  widgetId: string;
  content: React.ReactNode;
}

export interface WidgetNodeChild {
  type: string;
  id: string;
  props: {
    [key: string]: unknown;
  };
  children?: unknown[];
  events: string[];
}

export interface CalendarWidgetProps {
  id: string;
  defaultView?: CalendarView | string;
  defaultDate?: string;
  enableDragDrop?: boolean;
  showToolbar?: boolean;
  events?: string[];
  width?: string;
  height?: string;
  children?: React.ReactNode;
  slots?: {
    default?: React.ReactNode[];
  };
  widgetNodeChildren?: WidgetNodeChild[];
}
