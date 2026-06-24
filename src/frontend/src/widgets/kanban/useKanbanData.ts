import React from "react";
import { Task } from "@/components/ui/shadcn-io/kanban";
import type { Column, TaskWithWidgetId, CardData, ExtractedKanbanData } from "./types";

interface WidgetNodeChild {
  type: string;
  id: string;
  props: {
    [key: string]: unknown;
  };
  children?: unknown[];
  events: string[];
}

function extractColumnKeysFromCards(cards: CardData[]): string[] {
  const columnSet = new Set<string>();
  cards.forEach((card) => {
    if (card.columnKey) {
      columnSet.add(card.columnKey);
    }
  });
  return Array.from(columnSet);
}

function buildColumnNameMap(cards: CardData[]): Map<string, string> {
  const map = new Map<string, string>();
  cards.forEach((card) => {
    if (card.columnKey && card.columnName && !map.has(card.columnKey)) {
      map.set(card.columnKey, card.columnName);
    }
  });
  return map;
}

export function useKanbanData(
  slots: { default?: React.ReactNode[] } | undefined,
  tasks: Task[],
  columns: Column[],
  widgetNodeChildren?: WidgetNodeChild[],
): ExtractedKanbanData {
  return React.useMemo(() => {
    const kanbanChildren = (widgetNodeChildren || []).filter((c) => c.type === "Ivy.KanbanCard");
    if (kanbanChildren.length > 0) {
      const extractedCards: CardData[] = [];

      kanbanChildren.forEach((widgetNode, index) => {
        if (widgetNode.type === "Ivy.KanbanCard") {
          const cardId = widgetNode.props.cardId as string | undefined;
          const priority = widgetNode.props.priority as number | undefined;
          const column = widgetNode.props.column as string | undefined;
          const columnName = widgetNode.props.columnName as string | undefined;
          const widgetId = widgetNode.id;

          if (widgetId) {
            extractedCards.push({
              cardId: cardId || widgetId,
              priority,
              widgetId,
              content: slots?.default?.[index] || null,
              columnKey: column,
              columnName: columnName,
            });
          }
        }
      });

      if (extractedCards.length > 0 && tasks.length === 0) {
        const allColumnKeys = extractColumnKeysFromCards(extractedCards);
        const columnNameMap = buildColumnNameMap(extractedCards);

        const finalColumnKeys = allColumnKeys;

        const extractedColumns: Column[] = finalColumnKeys.map((key, index) => ({
          id: key,
          name: columnNameMap.get(key) ?? key,
          color: "",
          order: index,
        }));

        const extractedTasks: TaskWithWidgetId[] = extractedCards.map((card) => {
          const column = card.columnKey || "Default";
          const columnIndex = finalColumnKeys.indexOf(column);

          return {
            id: card.cardId,
            title: "",
            status: column,
            statusOrder: columnIndex >= 0 ? columnIndex : 0,
            priority: card.priority || 0,
            description: "",
            assignee: "",
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
      tasks.forEach((task) => {
        if (!statusMap.has(task.status)) {
          statusMap.set(task.status, []);
        }
        statusMap.get(task.status)!.push(task);
      });

      const statusKeys = Array.from(statusMap.keys());
      const columnNameMap = buildColumnNameMap(extractedCards);

      const columnKeys = statusKeys;

      const extractedColumns: Column[] = columnKeys.map((status, index) => ({
        id: status,
        name: columnNameMap.get(status) ?? status,
        color: "",
        order: index,
      }));

      const cardToTaskMap = new Map<string, Task>();
      tasks.forEach((task) => {
        cardToTaskMap.set(task.id, task);
      });

      const extractedTasks = extractedCards.reduce<TaskWithWidgetId[]>((acc, card) => {
        const task = cardToTaskMap.get(card.cardId);
        if (task) {
          acc.push({ ...task, widgetId: card.widgetId });
        }
        return acc;
      }, []);

      return {
        tasks: extractedTasks,
        columns: extractedColumns,
        cards: extractedCards,
      };
    }

    return {
      tasks: tasks.map((t) => ({ ...t, widgetId: t.id })),
      columns,
      cards: [],
    };
  }, [slots, tasks, columns, widgetNodeChildren]);
}
