import { useEventHandler } from "@/components/event-handler";

export function useKanbanHandlers(widgetId: string, events: string[]) {
  const eventHandler = useEventHandler();

  const handleCardMove = (
    cardId: string,
    _fromColumn: string,
    toColumn: string,
    targetIndex?: number,
  ) => {
    if (events.includes("OnCardMove"))
      eventHandler("OnCardMove", widgetId, [cardId, toColumn, targetIndex]);
  };

  return {
    handleCardMove,
  };
}
