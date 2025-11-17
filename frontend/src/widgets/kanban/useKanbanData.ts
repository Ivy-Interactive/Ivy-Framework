import React from 'react';
import { Task } from '@/components/ui/shadcn-io/kanban';
import type {
  Column,
  TaskWithWidgetId,
  CardData,
  ExtractedKanbanData,
} from './types';

export function useKanbanData(
  slots: { default?: React.ReactNode[] } | undefined,
  tasks: Task[],
  columns: Column[],
  columnWidths: Record<string, string>
): ExtractedKanbanData {
  return React.useMemo(() => {
    if (slots?.default && slots.default.length > 0) {
      const extractedCards: CardData[] = [];

      slots.default.forEach(cardNode => {
        if (React.isValidElement(cardNode)) {
          const cardProps = (
            cardNode.props as {
              children?: { props?: Record<string, unknown> };
            }
          )?.children?.props as Record<string, unknown>;

          const cardId = cardProps?.cardId as string | undefined;
          const priority = cardProps?.priority as number | undefined;
          const widgetId = cardProps?.id as string | undefined;

          if (cardId && widgetId) {
            extractedCards.push({
              cardId,
              priority,
              widgetId,
              content: cardNode,
            });
          }
        }
      });

      const statusMap = new Map<string, Task[]>();
      tasks.forEach(task => {
        if (!statusMap.has(task.status)) {
          statusMap.set(task.status, []);
        }
        statusMap.get(task.status)!.push(task);
      });

      const extractedColumns: Column[] = Array.from(statusMap.keys()).map(
        (status, index) => ({
          id: status,
          name: status,
          color: '',
          order: index,
          width: columnWidths[status],
        })
      );

      const cardToTaskMap = new Map<string, Task>();
      tasks.forEach(task => {
        cardToTaskMap.set(task.id, task);
      });

      const extractedTasks: TaskWithWidgetId[] = extractedCards
        .map(card => {
          const task = cardToTaskMap.get(card.cardId);
          if (task) {
            return {
              ...task,
              widgetId: card.widgetId,
            };
          }
          return null;
        })
        .filter((task): task is TaskWithWidgetId => task !== null);

      return {
        tasks: extractedTasks,
        columns: extractedColumns,
        cards: extractedCards,
      };
    }

    return {
      tasks: tasks.map(t => ({ ...t, widgetId: t.id })),
      columns,
      cards: [],
    };
  }, [slots, tasks, columns, columnWidths]);
}
