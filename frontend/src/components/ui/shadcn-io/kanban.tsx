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
import { Separator } from '@/components/ui/separator';
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

interface DropTarget {
  column: string;
  index: number;
}

interface KanbanContextType {
  data: Task[];
  columns: Column[];
  draggedCardColumn: string | null;
  setDraggedCardColumn: (column: string | null) => void;
  draggedCardId: string | null;
  setDraggedCardId: (id: string | null) => void;
  dragOverColumn: string | null;
  setDragOverColumn: (column: string | null) => void;
  dropTarget: DropTarget | null;
  setDropTarget: (target: DropTarget | null) => void;
  onCardMove?: (
    cardId: string,
    fromColumn: string,
    toColumn: string,
    targetIndex?: number
  ) => void;
  showCounts?: boolean;
}

const KanbanContext = createContext<KanbanContextType>({
  data: [],
  columns: [],
  draggedCardColumn: null,
  setDraggedCardColumn: () => {},
  draggedCardId: null,
  setDraggedCardId: () => {},
  dragOverColumn: null,
  setDragOverColumn: () => {},
  dropTarget: null,
  setDropTarget: () => {},
});

const useKanbanContext = () => useContext(KanbanContext);

const GAP_HEIGHT = 70;

function adjustIndexForSameColumnMove(
  data: Task[],
  cardId: string,
  column: string,
  targetIndex: number
): number {
  const columnTasks = data.filter(t => t.status === column);
  const draggedIndex = columnTasks.findIndex(t => t.id === cardId);
  if (draggedIndex !== -1 && draggedIndex < targetIndex) {
    return targetIndex - 1;
  }
  return targetIndex;
}

interface KanbanProps {
  columns: Column[];
  data: Task[];
  onCardMove?: (
    cardId: string,
    fromColumn: string,
    toColumn: string,
    targetIndex?: number
  ) => void;
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
  showCounts = true,
  children,
}: KanbanProps) {
  const [draggedCardColumn, setDraggedCardColumn] = useState<string | null>(
    null
  );
  const [draggedCardId, setDraggedCardId] = useState<string | null>(null);
  const [dragOverColumn, setDragOverColumn] = useState<string | null>(null);
  const [dropTarget, setDropTarget] = useState<DropTarget | null>(null);

  return (
    <KanbanContext.Provider
      value={{
        data,
        columns,
        draggedCardColumn,
        setDraggedCardColumn,
        draggedCardId,
        setDraggedCardId,
        dragOverColumn,
        setDragOverColumn,
        dropTarget,
        setDropTarget,
        onCardMove,
        showCounts,
      }}
    >
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
      className={cn('flex h-full bg-background flex-row gap-3', className)}
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
  const {
    onCardMove,
    data,
    draggedCardColumn,
    dragOverColumn,
    setDragOverColumn,
    setDropTarget,
    dropTarget,
    showCounts,
  } = useKanbanContext();

  const columnTaskCount = data.filter(task => task.status === id).length;
  const showDragOver = dragOverColumn === id && draggedCardColumn !== null;
  const widthStyles = width ? getWidth(width) : {};
  const hasExplicitWidth = width && Object.keys(widthStyles).length > 0;

  const handleDragOver = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      e.dataTransfer.dropEffect = 'move';
      if (
        draggedCardColumn &&
        draggedCardColumn !== id &&
        dragOverColumn !== id
      ) {
        setDragOverColumn(id);
      }
    },
    [id, draggedCardColumn, dragOverColumn, setDragOverColumn]
  );

  const handleDragLeave = useCallback(
    (e: React.DragEvent) => {
      if (!e.currentTarget.contains(e.relatedTarget as Node)) {
        setDragOverColumn(null);
        setDropTarget(null);
      }
    },
    [setDragOverColumn, setDropTarget]
  );

  const handleDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      setDragOverColumn(null);

      const cardId = e.dataTransfer.getData('text/plain');
      const task = data.find(t => t.id === cardId);
      if (!cardId || !task) return;

      let targetIndex =
        dropTarget?.column === id ? dropTarget.index : undefined;

      if (targetIndex !== undefined && task.status === id) {
        targetIndex = adjustIndexForSameColumnMove(
          data,
          cardId,
          id,
          targetIndex
        );
      }

      onCardMove?.(cardId, task.status, id, targetIndex);
      setDropTarget(null);
    },
    [id, onCardMove, data, setDragOverColumn, dropTarget, setDropTarget]
  );

  return (
    <div
      className={cn(
        hasExplicitWidth ? 'bg-background' : 'flex-1 bg-background',
        'rounded-lg px-0 py-4 min-h-0 flex flex-col transition-colors min-w-70',
        showDragOver && 'bg-accent rounded-lg',
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
  children: (task: Task, index: number) => ReactNode;
}

export function KanbanCards({ id, children }: KanbanCardsProps) {
  const { data, dropTarget, draggedCardId } = useKanbanContext();
  const columnTasks = data.filter(task => task.status === id);

  const isTargetColumn = dropTarget?.column === id;
  const dropIndex = dropTarget?.index ?? -1;
  const draggedCardIndex = columnTasks.findIndex(t => t.id === draggedCardId);
  const isSameColumnDrag = draggedCardIndex !== -1;
  const showLineIndicator = isTargetColumn && isSameColumnDrag;

  return (
    <ScrollArea className="flex-1 min-h-0">
      <div className="flex flex-col gap-2 p-1">
        {columnTasks.map((task, index) => {
          const isDraggedCard = task.id === draggedCardId;
          const shouldShift =
            isTargetColumn &&
            !isSameColumnDrag &&
            !isDraggedCard &&
            index >= dropIndex;
          const showLineBefore =
            showLineIndicator &&
            index === dropIndex &&
            dropIndex !== draggedCardIndex;
          const showLineAfter =
            showLineIndicator &&
            index === columnTasks.length - 1 &&
            dropIndex === columnTasks.length;

          return (
            <div key={task.id} className="relative">
              {showLineBefore && (
                <Separator className="absolute -top-1 left-0 right-0 h-0.5 bg-muted-foreground/40 z-10" />
              )}
              <div
                style={{
                  transform: shouldShift
                    ? `translateY(${GAP_HEIGHT}px)`
                    : undefined,
                  transition: 'transform 0.2s ease',
                }}
              >
                {children(task, index)}
              </div>
              {showLineAfter && (
                <Separator className="absolute -bottom-1 left-0 right-0 h-0.5 bg-muted-foreground/40 z-10" />
              )}
            </div>
          );
        })}
      </div>
    </ScrollArea>
  );
}

interface KanbanCardProps {
  id: string;
  column: string;
  index: number;
  children: ReactNode;
  className?: string;
}

export function KanbanCard({
  id,
  column,
  index,
  children,
  className,
}: KanbanCardProps) {
  const [isDragging, setIsDragging] = useState(false);
  const cardRef = useRef<HTMLDivElement>(null);
  const {
    onCardMove,
    data,
    setDraggedCardColumn,
    setDraggedCardId,
    setDragOverColumn,
    setDropTarget,
    dropTarget,
    draggedCardId,
  } = useKanbanContext();

  const handleDragStart = useCallback(
    (e: React.DragEvent) => {
      setIsDragging(true);
      setDraggedCardColumn(column);
      setDraggedCardId(id);
      e.dataTransfer.setData('text/plain', id);
      e.dataTransfer.effectAllowed = 'move';
    },
    [id, column, setDraggedCardColumn, setDraggedCardId]
  );

  const handleDragEnd = useCallback(() => {
    setIsDragging(false);
    setDraggedCardColumn(null);
    setDraggedCardId(null);
    setDragOverColumn(null);
    setDropTarget(null);
  }, [
    setDraggedCardColumn,
    setDraggedCardId,
    setDragOverColumn,
    setDropTarget,
  ]);

  const handleDragOver = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      e.dataTransfer.dropEffect = 'move';

      if (id === draggedCardId) return;

      const rect = cardRef.current?.getBoundingClientRect();
      if (!rect) return;

      const insertIndex =
        e.clientY < rect.top + rect.height / 2 ? index : index + 1;

      if (dropTarget?.column !== column || dropTarget?.index !== insertIndex) {
        setDropTarget({ column, index: insertIndex });
      }
    },
    [id, column, index, draggedCardId, dropTarget, setDropTarget]
  );

  const handleDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      e.stopPropagation();

      const droppedCardId = e.dataTransfer.getData('text/plain');
      if (!droppedCardId || droppedCardId === id) {
        setDropTarget(null);
        return;
      }

      const draggedTask = data.find(t => t.id === droppedCardId);
      if (!draggedTask) {
        setDropTarget(null);
        return;
      }

      let targetIndex = dropTarget?.index ?? index;

      if (draggedTask.status === column) {
        targetIndex = adjustIndexForSameColumnMove(
          data,
          droppedCardId,
          column,
          targetIndex
        );
      }

      onCardMove?.(droppedCardId, draggedTask.status, column, targetIndex);
      setDropTarget(null);
    },
    [id, column, index, onCardMove, data, dropTarget, setDropTarget]
  );

  return (
    <div
      ref={cardRef}
      draggable={!!onCardMove}
      onDragStart={handleDragStart}
      onDragEnd={handleDragEnd}
      onDragOver={handleDragOver}
      onDrop={handleDrop}
      className={cn(
        'opacity-100 relative group',
        onCardMove && 'cursor-grab',
        isDragging && 'opacity-50 cursor-grabbing',
        className
      )}
    >
      {children}
    </div>
  );
}

export function KanbanHeader({ children }: { children: ReactNode }) {
  return <>{children}</>;
}

export function KanbanCardContent({ children }: { children: ReactNode }) {
  return <>{children}</>;
}
