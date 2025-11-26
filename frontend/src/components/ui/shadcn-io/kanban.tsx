'use client';

import {
  createContext,
  useContext,
  useState,
  useCallback,
  useRef,
  ReactNode,
} from 'react';
import { ScrollArea } from '@/components/ui/scroll-area';
import { cn } from '@/lib/utils';
import { getWidth } from '@/lib/styles';

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
  showCounts?: boolean;
}

const KanbanContext = createContext<KanbanContextType>({
  data: [],
  columns: [],
  draggedCardColumn: null,
  setDraggedCardColumn: () => {},
});

export const useKanbanContext = () => useContext(KanbanContext);

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
  showCounts?: boolean;
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
  showCounts = true,
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
    showCounts,
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

interface KanbanBoardProps {
  children: ReactNode;
  className?: string;
}

export function KanbanBoard({ children, className }: KanbanBoardProps) {
  return (
    <div
      className={cn('flex h-full bg-background', className)}
      style={{ minWidth: 'fit-content', maxWidth: '100%' }}
    >
      {children}
    </div>
  );
}

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
  const { onCardMove, data, draggedCardColumn, showCounts } =
    useKanbanContext();

  const columnTaskCount = data.filter(task => task.status === id).length;

  const handleDragOver = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      e.dataTransfer.dropEffect = 'move';

      if (draggedCardColumn && draggedCardColumn !== id) {
        setIsDragOver(true);
      }
    },
    [id, draggedCardColumn]
  );

  const handleDragLeave = useCallback((e: React.DragEvent) => {
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
      const task = data.find(t => t.id === cardId);
      if (!task) return;

      onCardMove?.(cardId, task.status, id);
    },
    [id, onCardMove, data]
  );

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
      <div className="px-2">
        <h3 className="font-semibold text-foreground flex items-center gap-2">
          {color && (
            <div
              className="h-3 w-3 rounded-full"
              style={{ backgroundColor: color }}
            />
          )}
          {name || id}
          {showCounts && (
            <span className="text-muted-foreground text-sm font-normal">
              ({columnTaskCount})
            </span>
          )}
        </h3>
      </div>
      {children}
    </div>
  );
}

interface KanbanCardsProps {
  id: string;
  children: (task: Task) => ReactNode;
}

export function KanbanCards({ id, children }: KanbanCardsProps) {
  const { data } = useKanbanContext();
  const columnTasks = data.filter(task => task.status === id);

  return (
    <ScrollArea className="flex-1 min-h-0">
      <div className="flex flex-col gap-3 p-2">
        {columnTasks.map(task => (
          <div key={task.id}>{children(task)}</div>
        ))}
      </div>
    </ScrollArea>
  );
}

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
  const justDraggedRef = useRef(false);
  const { onCardMove, data, setDraggedCardColumn } = useKanbanContext();

  const handleDragStart = useCallback(
    (e: React.DragEvent) => {
      setIsDragging(true);
      justDraggedRef.current = false;
      setDraggedCardColumn(column);
      e.dataTransfer.setData('text/plain', id);
      e.dataTransfer.effectAllowed = 'move';
    },
    [id, column, setDraggedCardColumn]
  );

  const handleDragEnd = useCallback(() => {
    setIsDragging(false);
    setDraggedCardColumn(null);
    justDraggedRef.current = true;
    setTimeout(() => {
      justDraggedRef.current = false;
    }, 100);
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
      e.stopPropagation();
      setIsDragOver(false);

      const draggedCardId = e.dataTransfer.getData('text/plain');
      if (!draggedCardId || draggedCardId === id) return;

      const draggedTask = data.find(t => t.id === draggedCardId);
      if (!draggedTask) return;

      const columnTasks = data.filter(task => task.status === column);
      const targetIndex = columnTasks.findIndex(task => task.id === id);

      onCardMove?.(draggedCardId, draggedTask.status, column, targetIndex);
    },
    [id, column, onCardMove, data]
  );

  return (
    <div
      draggable={!!onCardMove}
      onDragStart={handleDragStart}
      onDragEnd={handleDragEnd}
      onDragOver={handleDragOver}
      onDragLeave={handleDragLeave}
      onDrop={handleDrop}
      className={cn(
        'opacity-100 transition-all relative group',
        onCardMove && 'cursor-grab',
        isDragging && 'opacity-50 cursor-grabbing',
        isDragOver &&
          'bg-accent border-2 border-accent-foreground border-dashed',
        className
      )}
    >
      {children}
    </div>
  );
}

interface KanbanHeaderProps {
  children: ReactNode;
}

export function KanbanHeader({ children }: KanbanHeaderProps) {
  return <>{children}</>;
}

interface KanbanCardContentProps {
  children: ReactNode;
}

export function KanbanCardContent({ children }: KanbanCardContentProps) {
  return <>{children}</>;
}
