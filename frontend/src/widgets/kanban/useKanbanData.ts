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

/**
 * Get status order matching backend's GetStatusOrder function
 * This ensures frontend column order matches backend ColumnOrder
 */
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

/**
 * Sort column keys by backend order (from columnWidths keys) or fallback to status order
 */
function sortColumnKeysByBackendOrder(
  columnKeys: string[],
  columnWidths: Record<string, string>
): string[] {
  const columnWidthsKeys = Object.keys(columnWidths);

  // If columnWidths has keys, use that order (backend ColumnOrder is preserved in columnWidths keys)
  if (columnWidthsKeys.length > 0) {
    // Create a map of columnWidths order
    const orderMap = new Map<string, number>();
    columnWidthsKeys.forEach((key, index) => {
      orderMap.set(key, index);
    });

    // Sort by columnWidths order, then by status order for missing ones
    return [...columnKeys].sort((a, b) => {
      const orderA = orderMap.has(a)
        ? orderMap.get(a)!
        : getStatusOrder(a) + 1000;
      const orderB = orderMap.has(b)
        ? orderMap.get(b)!
        : getStatusOrder(b) + 1000;
      return orderA - orderB;
    });
  }

  // Fallback: sort by status order
  return [...columnKeys].sort((a, b) => {
    return getStatusOrder(a) - getStatusOrder(b);
  });
}

export function useKanbanData(
  slots: { default?: React.ReactNode[] } | undefined,
  tasks: Task[],
  columns: Column[],
  columnWidths: Record<string, string>,
  widgetNodeChildren?: WidgetNodeChild[]
): ExtractedKanbanData {
  return React.useMemo(() => {
    if (widgetNodeChildren && widgetNodeChildren.length > 0) {
      const extractedCards: CardData[] = [];

      // Extract data from widget node structure - backend dictates the structure
      widgetNodeChildren.forEach((widgetNode, index) => {
        if (widgetNode.type === 'Ivy.KanbanCard') {
          // Backend serializes CardId as cardId, Priority as priority, Column as column (camelCase)
          const cardId = widgetNode.props.cardId as string | undefined;
          const priority = widgetNode.props.priority as number | undefined;
          const column = widgetNode.props.column as string | undefined;
          const widgetId = widgetNode.id;

          // Use cardId from props, fallback to widgetId if not provided
          if (widgetId) {
            extractedCards.push({
              cardId: cardId || widgetId,
              priority,
              widgetId,
              content: slots?.default?.[index] || null,
              columnKey: column, // Backend sends group key as Column prop
            });
          }
        }
      });

      // If we have cards but no tasks, create tasks and columns from cards
      // Backend sends Column prop on each card with the group key from groupBySelector
      // Each card knows which column it belongs to via the Column prop
      if (extractedCards.length > 0 && tasks.length === 0) {
        // Collect unique column values from cards to create columns
        const columnSet = new Set<string>();
        extractedCards.forEach(card => {
          if (card.columnKey) {
            columnSet.add(card.columnKey);
          }
        });

        // Get all column keys from cards
        const allColumnKeys = Array.from(columnSet);

        // Sort by backend order (columnWidths keys preserve backend ColumnOrder)
        const finalColumnKeys = sortColumnKeysByBackendOrder(
          allColumnKeys,
          columnWidths
        );

        // Create columns from column values, preserving backend order
        const extractedColumns: Column[] = finalColumnKeys.map(
          (key, index) => ({
            id: key,
            name: key,
            color: '',
            order: index,
            width: columnWidths[key],
          })
        );

        // Create tasks from cards using Column prop from backend
        const extractedTasks: TaskWithWidgetId[] = extractedCards.map(card => {
          // Use Column prop from backend to determine which column this card belongs to
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

      // Normal case: we have tasks, create columns from task statuses
      const statusMap = new Map<string, Task[]>();
      tasks.forEach(task => {
        if (!statusMap.has(task.status)) {
          statusMap.set(task.status, []);
        }
        statusMap.get(task.status)!.push(task);
      });

      // Get all column keys from task statuses
      const statusKeys = Array.from(statusMap.keys());

      // Sort by backend order (columnWidths keys preserve backend ColumnOrder)
      const columnKeys = sortColumnKeysByBackendOrder(statusKeys, columnWidths);

      const extractedColumns: Column[] = columnKeys.map((status, index) => ({
        id: status,
        name: status,
        color: '',
        order: index,
        width: columnWidths[status],
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

    // Fallback: use provided tasks and columns directly
    return {
      tasks: tasks.map(t => ({ ...t, widgetId: t.id })),
      columns,
      cards: [],
    };
  }, [slots, tasks, columns, columnWidths, widgetNodeChildren]);
}
