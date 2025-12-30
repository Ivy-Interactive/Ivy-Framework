import { useEventHandler } from '@/components/event-handler';

export function useKanbanHandlers(widgetId: string) {
  const eventHandler = useEventHandler();

  const handleCardMove = (
    cardId: string,
    _fromColumn: string,
    toColumn: string,
    targetIndex?: number
  ) => {
    eventHandler('OnCardMove', widgetId, [cardId, toColumn, targetIndex]);
  };

  return {
    handleCardMove,
  };
}
