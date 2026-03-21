import React from "react";
import { parseISO } from "date-fns";
import type { CalendarEvent, WidgetNodeChild } from "./types";

export function useCalendarData(
  slots: { default?: React.ReactNode[] } | undefined,
  widgetNodeChildren?: WidgetNodeChild[],
): CalendarEvent[] {
  return React.useMemo(() => {
    if (!widgetNodeChildren || widgetNodeChildren.length === 0) {
      return [];
    }

    const events: CalendarEvent[] = [];

    widgetNodeChildren.forEach((widgetNode, index) => {
      if (widgetNode.type === "Ivy.CalendarEvent") {
        const eventId = (widgetNode.props.eventId as string) || widgetNode.id;
        const title = (widgetNode.props.title as string) || "";
        const startStr = widgetNode.props.start as string | undefined;
        const endStr = widgetNode.props.end as string | undefined;
        const color = widgetNode.props.color as string | undefined;
        const allDay = widgetNode.props.allDay as boolean | undefined;
        const widgetId = widgetNode.id;
        const hasChildren = widgetNode.children && widgetNode.children.length > 0;

        if (!startStr) return;

        let start: Date;
        let end: Date;

        try {
          start = parseISO(startStr);
          end = endStr ? parseISO(endStr) : new Date(start.getTime() + 60 * 60 * 1000);
        } catch {
          return;
        }

        if (isNaN(start.getTime())) return;
        if (isNaN(end.getTime())) {
          end = new Date(start.getTime() + 60 * 60 * 1000);
        }

        events.push({
          id: eventId,
          title,
          start,
          end,
          color,
          allDay: allDay ?? false,
          widgetId,
          content: hasChildren ? slots?.default?.[index] || null : null,
        });
      }
    });

    return events;
  }, [slots, widgetNodeChildren]);
}
