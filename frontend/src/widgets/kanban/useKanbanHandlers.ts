import { useEventHandler } from '@/components/event-handler';
import type { TaskWithWidgetId } from './types';

export function useKanbanHandlers(widgetId: string, tasks: TaskWithWidgetId[]) {
  const eventHandler = useEventHandler();

  const handleCardMove = (
    cardId: string,
    _fromColumn: string,
    toColumn: string,
    targetIndex?: number
  ) => {
    eventHandler('OnCardMove', widgetId, [cardId, toColumn, targetIndex]);
  };

  const handleCardClick = (cardId: string) => {
    const task = tasks.find(t => t.id === cardId);
    if (task?.widgetId) {
      eventHandler('OnClick', task.widgetId, [cardId]);
    }
  };

  const handleCardDelete = (cardId: string) => {
    eventHandler('OnDelete', widgetId, [cardId]);
  };

  return {
    handleCardMove,
    handleCardClick,
    handleCardDelete,
  };
}
