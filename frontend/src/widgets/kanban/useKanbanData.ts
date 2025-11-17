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
          // Try multiple paths to extract props - widgets can be structured differently
          const cardProps = cardNode.props as Record<string, unknown>;

          // Try direct props first (most common case)
          let cardId = cardProps?.cardId as string | undefined;
          let priority = cardProps?.priority as number | undefined;
          let widgetId = cardProps?.id as string | undefined;

          // If not found, try nested structure (some widget renderers nest props)
          if (!cardId && cardProps?.children) {
            const childrenProps = (
              cardProps.children as { props?: Record<string, unknown> }
            )?.props;
            if (childrenProps) {
              cardId = childrenProps.cardId as string | undefined;
              priority = childrenProps.priority as number | undefined;
              widgetId = childrenProps.id as string | undefined;
            }
          }

          // Extract card if we have at least a widgetId (cardId might be optional)
          if (widgetId) {
            extractedCards.push({
              cardId: cardId || widgetId, // Use widgetId as fallback for cardId
              priority,
              widgetId,
              content: cardNode,
            });
          }
        }
      });

      // If we have cards but no tasks, create tasks and columns from cards
      // Cards come grouped by status in order from backend - ColumnWidths keys are the column names
      if (extractedCards.length > 0 && tasks.length === 0) {
        const columnKeys = Object.keys(columnWidths);

        const extractedColumns: Column[] =
          columnKeys.length > 0
            ? columnKeys.map((key, index) => ({
                id: key,
                name: key,
                color: '',
                order: index,
                width: columnWidths[key],
              }))
            : [
                {
                  id: 'Default',
                  name: 'Default',
                  color: '',
                  order: 0,
                },
              ];

        // Cards come grouped by status consecutively from backend
        // We need to detect group boundaries to distribute correctly
        // Since cards are grouped consecutively, distribute proportionally
        // This is a heuristic - ideally backend would send status with each card
        const extractedTasks: TaskWithWidgetId[] = [];
        let currentColumnIndex = 0;
        let cardsInCurrentColumn = 0;
        const estimatedCardsPerColumn = Math.floor(
          extractedCards.length / extractedColumns.length
        );
        const remainder = extractedCards.length % extractedColumns.length;

        extractedCards.forEach(card => {
          // Distribute cards: first columns get one extra card if remainder exists
          const expectedCardsInColumn =
            estimatedCardsPerColumn + (currentColumnIndex < remainder ? 1 : 0);

          if (
            cardsInCurrentColumn >= expectedCardsInColumn &&
            currentColumnIndex < extractedColumns.length - 1
          ) {
            currentColumnIndex++;
            cardsInCurrentColumn = 0;
          }

          extractedTasks.push({
            id: card.cardId,
            title: '', // Card widget renders its own content
            status: extractedColumns[currentColumnIndex].id,
            statusOrder: currentColumnIndex,
            priority: card.priority || 0,
            description: '', // Card widget renders its own content
            assignee: '',
            widgetId: card.widgetId,
          });

          cardsInCurrentColumn++;
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

    // Fallback: use provided tasks and columns directly
    return {
      tasks: tasks.map(t => ({ ...t, widgetId: t.id })),
      columns,
      cards: [],
    };
  }, [slots, tasks, columns, columnWidths]);
}
