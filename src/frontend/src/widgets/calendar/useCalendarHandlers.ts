import { useEventHandler } from "@/components/event-handler";

export function useCalendarHandlers(widgetId: string) {
  const eventHandler = useEventHandler();

  const handleEventClick = (eventId: string) => {
    eventHandler("OnEventClick", widgetId, [eventId]);
  };

  const handleEventMove = (eventId: string, newStart: string, newEnd: string) => {
    eventHandler("OnEventMove", widgetId, [eventId, newStart, newEnd]);
  };

  const handleEventResize = (eventId: string, newStart: string, newEnd: string) => {
    eventHandler("OnEventResize", widgetId, [eventId, newStart, newEnd]);
  };

  const handleSelectSlot = (start: string, end: string) => {
    eventHandler("OnSelectSlot", widgetId, [start, end]);
  };

  return {
    handleEventClick,
    handleEventMove,
    handleEventResize,
    handleSelectSlot,
  };
}
