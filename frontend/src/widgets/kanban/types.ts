import { Task } from '@/components/ui/shadcn-io/kanban';

export interface Column {
  id: string;
  name: string;
  color: string;
  order: number;
  width?: string;
}

export interface TaskWithWidgetId extends Task {
  widgetId: string;
}

export interface CardData {
  cardId: string;
  priority?: number;
  widgetId: string;
  content: React.ReactNode;
  columnKey?: string;
}

export interface ExtractedKanbanData {
  tasks: TaskWithWidgetId[];
  columns: Column[];
  cards: CardData[];
}

export interface KanbanWidgetProps {
  id: string;
  columns?: Column[];
  tasks?: Task[];
  events?: Record<string, unknown>;
  width?: string;
  height?: string;
  allowDelete?: boolean;
  allowMove?: boolean;
  columnWidths?: Record<string, string>;
  children?: React.ReactNode;
  slots?: {
    default?: React.ReactNode[];
  };
}
