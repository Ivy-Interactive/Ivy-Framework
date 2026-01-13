import React from 'react';
import { Task } from '@/components/ui/shadcn-io/kanban';
import type {
  Column,
  TaskWithWidgetId,
  CardData,
  ExtractedKanbanData,
} from './types';

interface WidgetNodeChild {
  type: string;
  id: string;
  props: {
    [key: string]: unknown;
  };
  children?: unknown[];
  events: string[];
}

function getStatusOrder(status: string): number {
  switch (status) {
    case 'Todo':
      return 1;
    case 'In Progress':
      return 2;
    case 'Done':
      return 3;
    default:
      return 0;
  }
}

function extractColumnKeysFromCards(cards: CardData[]): string[] {
  const columnSet = new Set<string>();
  cards.forEach(card => {
    if (card.columnKey) {
      columnSet.add(card.columnKey);
    }
  });
  return Array.from(columnSet);
}

function sortColumnKeysByBackendOrder(columnKeys: string[]): string[] {
  return [...columnKeys].sort((a, b) => {
    return getStatusOrder(a) - getStatusOrder(b);
  });
}

export function useKanbanData(
  slots: { default?: React.ReactNode[] } | undefined,
  tasks: Task[],
  columns: Column[],
  widgetNodeChildren?: WidgetNodeChild[]
): ExtractedKanbanData {
  return React.useMemo(() => {
    if (widgetNodeChildren && widgetNodeChildren.length > 0) {
      const extractedCards: CardData[] = [];

      widgetNodeChildren.forEach((widgetNode, index) => {
        if (widgetNode.type === 'Ivy.KanbanCard') {
          const cardId = widgetNode.props.cardId as string | undefined;
          const priority = widgetNode.props.priority as number | undefined;
          const column = widgetNode.props.column as string | undefined;
          const widgetId = widgetNode.id;

          if (widgetId) {
            extractedCards.push({
              cardId: cardId || widgetId,
              priority,
              widgetId,
              content: slots?.default?.[index] || null,
              columnKey: column,
            });
          }
        }
      });

      if (extractedCards.length > 0 && tasks.length === 0) {
        const allColumnKeys = extractColumnKeysFromCards(extractedCards);

        const finalColumnKeys = sortColumnKeysByBackendOrder(allColumnKeys);

        const extractedColumns: Column[] = finalColumnKeys.map(
          (key, index) => ({
            id: key,
            name: key,
            color: '',
            order: index,
          })
        );

        const extractedTasks: TaskWithWidgetId[] = extractedCards.map(card => {
          const column = card.columnKey || 'Default';
          const columnIndex = finalColumnKeys.indexOf(column);

          return {
            id: card.cardId,
            title: '',
            status: column,
            statusOrder: columnIndex >= 0 ? columnIndex : 0,
            priority: card.priority || 0,
            description: '',
            assignee: '',
            widgetId: card.widgetId,
          };
        });

        return {
          tasks: extractedTasks,
          columns: extractedColumns,
          cards: extractedCards,
        };
      }

      const statusMap = new Map<string, Task[]>();
      tasks.forEach(task => {
        if (!statusMap.has(task.status)) {
          statusMap.set(task.status, []);
        }
        statusMap.get(task.status)!.push(task);
      });

      const statusKeys = Array.from(statusMap.keys());

      const columnKeys = sortColumnKeysByBackendOrder(statusKeys);

      const extractedColumns: Column[] = columnKeys.map((status, index) => ({
        id: status,
        name: status,
        color: '',
        order: index,
      }));

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
  }, [slots, tasks, columns, widgetNodeChildren]);
}
