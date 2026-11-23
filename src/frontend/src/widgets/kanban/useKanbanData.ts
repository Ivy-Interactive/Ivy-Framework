import React from "react";
import { Task } from "@/components/ui/shadcn-io/kanban";
import type {
  Column,
  ProvidedColumn,
  TaskWithWidgetId,
  CardData,
  ExtractedKanbanData,
} from "./types";

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
    case "Todo":
      return 1;
    case "In Progress":
      return 2;
    case "Done":
      return 3;
    default:
      return 0;
  }
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

function sortColumnKeysByBackendOrder(columnKeys: string[]): string[] {
  return [...columnKeys].sort((a, b) => {
    return getStatusOrder(a) - getStatusOrder(b);
  });
}

function normalizeProvidedColumns(columns: ProvidedColumn[]): Column[] {
  return columns.map((column, index) => ({
    id: String(column.id),
    name: column.name ?? String(column.id),
    color: column.color ?? "",
    order: column.order ?? index,
    icon: column.icon,
  }));
}

/**
 * Builds the final column list: provided (static) columns first in their
 * declared order, followed by any extra columns discovered on the cards.
 * Without provided columns, falls back to deriving columns from the cards.
 */
function buildColumns(
  dataColumnKeys: string[],
  columnNameMap: Map<string, string>,
  providedColumns: Column[],
): Column[] {
  if (providedColumns.length > 0) {
    const knownIds = new Set(providedColumns.map((c) => c.id));
    const extras = dataColumnKeys.filter((key) => !knownIds.has(key));
    return [
      ...providedColumns,
      ...extras.map((key, index) => ({
        id: key,
        name: columnNameMap.get(key) ?? key,
        color: "",
        order: providedColumns.length + index,
      })),
    ];
  }

  const sortedKeys = sortColumnKeysByBackendOrder(dataColumnKeys);
  return sortedKeys.map((key, index) => ({
    id: key,
    name: columnNameMap.get(key) ?? key,
    color: "",
    order: index,
  }));
}

export function useKanbanData(
  slots: { default?: React.ReactNode[] } | undefined,
  tasks: Task[],
  columns: ProvidedColumn[],
  widgetNodeChildren?: WidgetNodeChild[],
): ExtractedKanbanData {
  return React.useMemo(() => {
    const providedColumns = normalizeProvidedColumns(columns);
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

        const extractedColumns = buildColumns(allColumnKeys, columnNameMap, providedColumns);
        const columnIndexById = new Map(extractedColumns.map((c, i) => [c.id, i]));

        const extractedTasks: TaskWithWidgetId[] = extractedCards.map((card) => {
          const column = card.columnKey || "Default";
          const columnIndex = columnIndexById.get(column) ?? 0;

          return {
            id: card.cardId,
            title: "",
            status: column,
            statusOrder: columnIndex,
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

      const extractedColumns = buildColumns(statusKeys, columnNameMap, providedColumns);

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
      columns: providedColumns,
      cards: [],
    };
  }, [slots, tasks, columns, widgetNodeChildren]);
}
