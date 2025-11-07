'use client';

import {
  createContext,
  useContext,
  useState,
  useCallback,
  ReactNode,
} from 'react';
import { ScrollArea } from '@/components/ui/scroll-area';
import { cn } from '@/lib/utils';
import { getWidth } from '@/lib/styles';

// Types
export interface Task {
  id: string;
  title: string;
  status: string;
  statusOrder: number;
  priority: number;
  description: string;
  assignee: string;
}

export interface Column {
  id: string;
  name: string;
  color: string;
}

// Context
interface KanbanContextType {
  data: Task[];
  columns: Column[];
  draggedCardColumn: string | null;
  setDraggedCardColumn: (column: string | null) => void;
  onCardMove?: (
    cardId: string,
    fromColumn: string,
    toColumn: string,
    targetIndex?: number
  ) => void;
  onCardReorder?: (cardId: string, fromIndex: number, toIndex: number) => void;
  onCardClick?: (cardId: string) => void;
  onCardDelete?: (cardId: string) => void;
}

const KanbanContext = createContext<KanbanContextType>({
  data: [],
  columns: [],
  draggedCardColumn: null,
  setDraggedCardColumn: () => {},
});

export const useKanbanContext = () => useContext(KanbanContext);

// Main Kanban Component
interface KanbanProps {
  columns: Column[];
  data: Task[];
  onCardMove?: (
    cardId: string,
    fromColumn: string,
    toColumn: string,
    targetIndex?: number
  ) => void;
  onCardReorder?: (cardId: string, fromIndex: number, toIndex: number) => void;
  onCardClick?: (cardId: string) => void;
  onCardDelete?: (cardId: string) => void;
  children: (components: {
    KanbanBoard: typeof KanbanBoard;
    KanbanColumn: typeof KanbanColumn;
    KanbanCards: typeof KanbanCards;
    KanbanCard: typeof KanbanCard;
    KanbanHeader: typeof KanbanHeader;
    KanbanCardContent: typeof KanbanCardContent;
  }) => ReactNode;
}

export function Kanban({
  columns,
  data,
  onCardMove,
  onCardReorder,
  onCardClick,
  onCardDelete,
  children,
}: KanbanProps) {
  const [draggedCardColumn, setDraggedCardColumn] = useState<string | null>(
    null
  );

  const contextValue: KanbanContextType = {
    data,
    columns,
    draggedCardColumn,
    setDraggedCardColumn,
    onCardMove,
    onCardReorder,
    onCardClick,
    onCardDelete,
  };

  return (
    <KanbanContext.Provider value={contextValue}>
      {children({
        KanbanBoard,
        KanbanColumn,
        KanbanCards,
        KanbanCard,
        KanbanHeader,
        KanbanCardContent,
      })}
    </KanbanContext.Provider>
  );
}

// KanbanBoard Component
interface KanbanBoardProps {
  children: ReactNode;
  className?: string;
}

export function KanbanBoard({ children, className }: KanbanBoardProps) {
  return (
    <div className={cn('flex gap-4 h-full bg-background min-w-fit', className)}>
      {children}
    </div>
  );
}

// KanbanColumn Component
interface KanbanColumnProps {
  id: string;
  name?: string;
  color?: string;
  width?: string;
  children: ReactNode;
  className?: string;
}

export function KanbanColumn({
  id,
  name,
  color,
  width,
  children,
  className,
}: KanbanColumnProps) {
  const [isDragOver, setIsDragOver] = useState(false);
  const { onCardMove, data, draggedCardColumn } = useKanbanContext();

  const handleDragOver = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      e.dataTransfer.dropEffect = 'move';

      // Only show drag over styling if dragging from a different column
      if (draggedCardColumn && draggedCardColumn !== id) {
        setIsDragOver(true);
      }
    },
    [id, draggedCardColumn]
  );

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    // Only set drag over to false if we're leaving the column entirely
    if (!e.currentTarget.contains(e.relatedTarget as Node)) {
      setIsDragOver(false);
    }
  }, []);

  const handleDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      setIsDragOver(false);

      const cardId = e.dataTransfer.getData('text/plain');
      if (!cardId) return;
      // Find the task to get the source column
      const task = data.find(t => t.id === cardId);
      if (!task) return;

      // Drop on column event

      // Send move event to backend - let backend handle positioning at the end
      onCardMove?.(cardId, task.status, id);
    },
    [id, onCardMove, data]
  );

  // Only show drag-over styling when actively dragging AND hovering over this column
  const showDragOver = isDragOver && draggedCardColumn !== null;

  const widthStyles = width ? getWidth(width) : {};
  const hasExplicitWidth = width && Object.keys(widthStyles).length > 0;

  return (
    <div
      className={cn(
        hasExplicitWidth ? 'bg-background' : 'flex-1 bg-background',
        'rounded-lg px-0 py-4 min-h-0 flex flex-col transition-colors min-w-70',
        showDragOver &&
          'bg-accent border-2 border-accent-foreground border-dashed rounded-lg',
        className
      )}
      style={widthStyles}
      onDragOver={handleDragOver}
      onDragLeave={handleDragLeave}
      onDrop={handleDrop}
    >
      {/* Column Header */}
      <div className="px-3">
        <h3 className="font-semibold text-foreground flex items-center gap-2">
          {color && (
            <div
              className="h-3 w-3 rounded-full"
              style={{ backgroundColor: color }}
            />
          )}
          {name || id}
        </h3>
      </div>
      {children}
    </div>
  );
}

// KanbanCards Component
interface KanbanCardsProps {
  id: string;
  children: (task: Task) => ReactNode;
}

export function KanbanCards({ id, children }: KanbanCardsProps) {
  const { data } = useKanbanContext();
  const columnTasks = data.filter(task => task.status === id);

  return (
    <ScrollArea className="flex-1 min-h-0">
      <div className="flex flex-col gap-3 p-3">
        {columnTasks.map(task => (
          <div key={task.id}>{children(task)}</div>
        ))}
      </div>
    </ScrollArea>
  );
}

// KanbanCard Component
interface KanbanCardProps {
  id: string;
  column: string;
  children: ReactNode;
  className?: string;
}

export function KanbanCard({
  id,
  column,
  children,
  className,
}: KanbanCardProps) {
  const [isDragging, setIsDragging] = useState(false);
  const [isDragOver, setIsDragOver] = useState(false);
  const [isHovered, setIsHovered] = useState(false);
  const { onCardMove, data, setDraggedCardColumn, onCardDelete } =
    useKanbanContext();

  const handleDragStart = useCallback(
    (e: React.DragEvent) => {
      // KanbanCard drag start
      setIsDragging(true);
      setDraggedCardColumn(column);
      e.dataTransfer.setData('text/plain', id);
      e.dataTransfer.effectAllowed = 'move';
    },
    [id, column, setDraggedCardColumn]
  );

  const handleDragEnd = useCallback(() => {
    setIsDragging(false);
    setDraggedCardColumn(null);
  }, [setDraggedCardColumn]);

  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    e.dataTransfer.dropEffect = 'move';
    setIsDragOver(true);
  }, []);

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    if (!e.currentTarget.contains(e.relatedTarget as Node)) {
      setIsDragOver(false);
    }
  }, []);

  const handleDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      e.stopPropagation(); // Prevent column drop from also triggering
      setIsDragOver(false);

      const draggedCardId = e.dataTransfer.getData('text/plain');
      if (!draggedCardId || draggedCardId === id) return;

      // Find the dragged task to get the source column
      const draggedTask = data.find(t => t.id === draggedCardId);
      if (!draggedTask) return;

      // Find the target position - where this card is in the column
      const columnTasks = data.filter(task => task.status === column);
      const targetIndex = columnTasks.findIndex(task => task.id === id);

      // Drop on card event

      // Send move event to backend with precise target index
      onCardMove?.(draggedCardId, draggedTask.status, column, targetIndex);
    },
    [id, column, onCardMove, data]
  );

  const handleDelete = useCallback(
    (e: React.MouseEvent) => {
      e.stopPropagation();
      e.preventDefault();
      onCardDelete?.(id);
    },
    [id, onCardDelete]
  );

  return (
    <div
      draggable
      onDragStart={handleDragStart}
      onDragEnd={handleDragEnd}
      onDragOver={handleDragOver}
      onDragLeave={handleDragLeave}
      onDrop={handleDrop}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
      className={cn(
        'cursor-grab opacity-100 transition-all relative group',
        isDragging && 'opacity-50 cursor-grabbing',
        isDragOver &&
          'bg-accent border-2 border-accent-foreground border-dashed',
        className
      )}
    >
      {onCardDelete && isHovered && !isDragging && (
        <button
          onClick={handleDelete}
          className="absolute top-1.5 right-1.5 z-10 h-5 w-5 rounded-sm bg-muted/80 hover:bg-destructive text-muted-foreground hover:text-destructive-foreground flex items-center justify-center opacity-0 group-hover:opacity-100 transition-all cursor-pointer"
          aria-label="Delete card"
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
            className="h-3 w-3"
          >
            <path d="M3 6h18" />
            <path d="M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6" />
            <path d="M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2" />
          </svg>
        </button>
      )}
      {children}
    </div>
  );
}

// KanbanHeader Component
interface KanbanHeaderProps {
  children: ReactNode;
}

export function KanbanHeader({ children }: KanbanHeaderProps) {
  return <>{children}</>;
}

// KanbanCardContent Component
interface KanbanCardContentProps {
  children: ReactNode;
}

export function KanbanCardContent({ children }: KanbanCardContentProps) {
  return <>{children}</>;
}
