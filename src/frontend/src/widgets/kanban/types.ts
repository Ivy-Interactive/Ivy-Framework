import { Task } from "@/components/ui/shadcn-io/kanban";
import { Densities } from "@/types/density";

export interface Column {
  id: string;
  name: string;
  color: string;
  order: number;
  icon?: string;
}

/** Column definition as serialized from the backend Kanban.Columns prop. */
export interface ProvidedColumn {
  id: string | number;
  name?: string;
  color?: string;
  order?: number;
  icon?: string;
}

export interface TaskWithWidgetId extends Task {
  widgetId: string;
}

export interface CardData {
  cardId: string;
  priority?: number;
  widgetId: string;
  content: React.ReactNode;
  columnKey?: string; // Column/group key from backend Column prop
  columnName?: string; // Human-friendly display name from backend ColumnName prop
}

export interface ExtractedKanbanData {
  tasks: TaskWithWidgetId[];
  columns: Column[];
  cards: CardData[];
}

export interface KanbanWidgetProps {
  id: string;
  columns?: ProvidedColumn[];
  tasks?: Task[];
  events?: string[];
  width?: string;
  height?: string;
  columnWidth?: string;
  showCounts?: boolean;
  density?: Densities;
  children?: React.ReactNode;
  slots?: {
    default?: React.ReactNode[];
  };
  widgetNodeChildren?: Array<{
    type: string;
    id: string;
    props: {
      [key: string]: unknown;
    };
    children?: unknown[];
    events: string[];
  }>;
}
