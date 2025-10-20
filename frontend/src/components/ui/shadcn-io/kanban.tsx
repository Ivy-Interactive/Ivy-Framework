'use client';

import {
  createContext,
  useContext,
  useState,
  useCallback,
  ReactNode,
} from 'react';
import {
  DndContext,
  DragEndEvent,
  DragOverlay,
  DragStartEvent,
  PointerSensor,
  useSensor,
  useSensors,
  useDroppable,
} from '@dnd-kit/core';
import {
  SortableContext,
  verticalListSortingStrategy,
} from '@dnd-kit/sortable';
import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { Card } from '@/components/ui/card';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { cn } from '@/lib/utils';

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
  order: number;
}

interface KanbanContextType {
  columns: Column[];
  data: Task[];
  onDataChange: (data: Task[]) => void;
  onCardMove?: (cardId: string, fromColumn: string, toColumn: string) => void;
  onCardAdd?: (columnId: string) => void;
  onCardDelete?: (cardId: string) => void;
}

const KanbanContext = createContext<KanbanContextType | null>(null);

const useKanbanContext = () => {
  const context = useContext(KanbanContext);
  if (!context) {
    throw new Error('useKanbanContext must be used within a KanbanProvider');
  }
  return context;
};

// Kanban Provider
interface KanbanProviderProps {
  columns: Column[];
  data: Task[];
  onDataChange: (data: Task[]) => void;
  onCardMove?: (cardId: string, fromColumn: string, toColumn: string) => void;
  onCardAdd?: (columnId: string) => void;
  onCardDelete?: (cardId: string) => void;
  children: (column: Column) => ReactNode;
}

export function KanbanProvider({
  columns,
  data,
  onDataChange,
  onCardMove,
  onCardAdd,
  onCardDelete,
  children,
}: KanbanProviderProps) {
  const [activeId, setActiveId] = useState<string | null>(null);

  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: {
        distance: 8,
      },
    })
  );

  const handleDragStart = useCallback((event: DragStartEvent) => {
    setActiveId(event.active.id as string);
  }, []);

  const handleDragEnd = useCallback(
    (event: DragEndEvent) => {
      const { active, over } = event;
      setActiveId(null);

      if (!over) {
        return;
      }

      const activeId = active.id as string;
      const overId = over.id as string;

      // Find the task being dragged
      const activeTask = data.find(task => task.id === activeId);
      if (!activeTask) {
        return;
      }

      // Determine the target column
      let targetColumnId: string;

      // Check if we're dropping on a column or on another card
      const overTask = data.find(task => task.id === overId);
      if (overTask) {
        targetColumnId = overTask.status;
      } else {
        // Dropping on a column - find the column that matches the overId
        const targetColumn = columns.find(col => col.id === overId);
        if (targetColumn) {
          targetColumnId = targetColumn.id;
        } else {
          // If overId doesn't match any column, check if it's a column ID from the data
          // This handles cases where the overId might be the column ID but not in our columns array
          const possibleColumnId = overId;
          if (columns.some(col => col.id === possibleColumnId)) {
            targetColumnId = possibleColumnId;
          } else {
            // Last resort: try to extract column from the overId or use the first available column
            // For now, let's try to use the overId as the column ID
            targetColumnId = overId;
          }
        }
      }

      // Always trigger the move event for now, let the backend handle the logic
      // This is a workaround for the drag-and-drop detection issue
      onCardMove?.(activeId, activeTask.status, targetColumnId);

      // Note: We don't update local state here since this is a backend-first framework
      // The backend will handle the state update and re-render the component
    },
    [data, columns, onCardMove]
  );

  const contextValue: KanbanContextType = {
    columns,
    data,
    onDataChange,
    onCardMove,
    onCardAdd,
    onCardDelete,
  };

  return (
    <KanbanContext.Provider value={contextValue}>
      <DndContext
        sensors={sensors}
        onDragStart={handleDragStart}
        onDragEnd={handleDragEnd}
      >
        <div className="flex gap-4 overflow-x-auto p-4">
          {columns
            .sort((a, b) => a.order - b.order)
            .map(column => children(column))}
        </div>
        <DragOverlay>
          {activeId ? (
            <KanbanCard
              id={activeId}
              column={data.find(t => t.id === activeId)?.status || ''}
              name={data.find(t => t.id === activeId)?.title || ''}
              task={data.find(t => t.id === activeId)}
            />
          ) : null}
        </DragOverlay>
      </DndContext>
    </KanbanContext.Provider>
  );
}

// Kanban Board
interface KanbanBoardProps {
  id: string;
  children: ReactNode;
}

export function KanbanBoard({ id, children }: KanbanBoardProps) {
  const { setNodeRef, isOver } = useDroppable({
    id,
    data: {
      type: 'column',
      columnId: id,
    },
  });

  return (
    <div
      ref={setNodeRef}
      id={id}
      className={`flex flex-col w-80 bg-gray-50 rounded-lg p-4 min-h-[600px] transition-colors duration-200 ${
        isOver ? 'bg-blue-50 border-2 border-blue-300' : ''
      }`}
    >
      {children}
    </div>
  );
}

// Kanban Header
interface KanbanHeaderProps {
  children: ReactNode;
}

export function KanbanHeader({ children }: KanbanHeaderProps) {
  return (
    <div className="flex items-center justify-between mb-4">{children}</div>
  );
}

// Kanban Cards Container
interface KanbanCardsProps {
  id: string;
  children: (task: Task) => ReactNode;
}

export function KanbanCards({ id, children }: KanbanCardsProps) {
  const { data } = useKanbanContext();
  const columnTasks = data
    .filter(task => task.status === id)
    .sort((a, b) => a.priority - b.priority);

  return (
    <SortableContext
      items={columnTasks.map(task => task.id)}
      strategy={verticalListSortingStrategy}
    >
      <div className="flex flex-col gap-3 flex-1">
        {columnTasks.map(task => (
          <div key={task.id}>{children(task)}</div>
        ))}
      </div>
    </SortableContext>
  );
}

// Kanban Card
interface KanbanCardProps {
  id: string;
  column: string;
  name: string;
  task?: Task;
  children?: ReactNode;
}

export function KanbanCard({ id, name, task, children }: KanbanCardProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({
    id,
    data: {
      type: 'card',
      card: task,
    },
  });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
  };

  return (
    <Card
      ref={setNodeRef}
      style={style}
      {...attributes}
      {...listeners}
      className={cn(
        'p-3 cursor-grab active:cursor-grabbing',
        isDragging && 'opacity-50'
      )}
    >
      <div className="flex items-start justify-between gap-2">
        <div className="flex flex-col gap-1 flex-1">
          <p className="m-0 flex-1 font-medium text-sm">{name}</p>
          {task?.description && (
            <p className="m-0 text-gray-500 text-xs">{task.description}</p>
          )}
        </div>
        {task?.assignee && (
          <Avatar className="h-6 w-6 shrink-0">
            <AvatarFallback className="text-xs">
              {task.assignee.slice(0, 2).toUpperCase()}
            </AvatarFallback>
          </Avatar>
        )}
      </div>

      <div className="mt-2 flex items-center justify-between text-xs text-gray-500">
        <span className="px-2 py-1 bg-gray-100 rounded">
          Priority {task?.priority || 1}
        </span>
        <span>{task?.assignee || 'Unassigned'}</span>
      </div>

      {children}
    </Card>
  );
}
