import { useEventHandler } from "@/components/event-handler";

export function useCalendarHandlers(widgetId: string, events: string[]) {
  const eventHandler = useEventHandler();

  const handleEventClick = (eventId: string) => {
    if (events.includes("OnEventClick")) eventHandler("OnEventClick", widgetId, [eventId]);
  };

  const handleEventMove = (eventId: string, newStart: string, newEnd: string) => {
    if (events.includes("OnEventMove"))
      eventHandler("OnEventMove", widgetId, [eventId, newStart, newEnd]);
  };

  const handleEventResize = (eventId: string, newStart: string, newEnd: string) => {
    if (events.includes("OnEventResize"))
      eventHandler("OnEventResize", widgetId, [eventId, newStart, newEnd]);
  };

  const handleSelectSlot = (start: string, end: string) => {
    if (events.includes("OnSelectSlot")) eventHandler("OnSelectSlot", widgetId, [start, end]);
  };

  return {
    handleEventClick,
    handleEventMove,
    handleEventResize,
    handleSelectSlot,
  };
}
