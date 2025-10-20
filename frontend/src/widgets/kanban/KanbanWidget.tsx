'use client';

import React from 'react';
import {
  KanbanProvider,
  KanbanBoard,
  KanbanCard,
  KanbanCards,
  KanbanHeader,
  type Task,
  type Column,
} from '@/components/ui/shadcn-io/kanban';
import { useEventHandler } from '@/components/event-handler';

interface KanbanWidgetProps {
  id: string;
  columns?: Column[];
  tasks?: Task[];
  events?: Record<string, unknown>;
  children?: React.ReactNode;
}

export const KanbanWidget: React.FC<KanbanWidgetProps> = ({
  id,
  columns = [],
  tasks = [],
  events,
  children,
}) => {
  const eventHandler = useEventHandler();

  // Extract data from backend kanban structure
  const extractedData = React.useMemo(() => {
    return { tasks, columns };
  }, [children, tasks, columns]);

  const handleCardMove = (
    cardId: string,
    fromColumn: string,
    toColumn: string
  ) => {
    // Always trigger the backend event for card moves
    eventHandler('OnCardMove', id, [
      {
        cardId,
        fromColumn,
        toColumn,
      },
    ]);
  };

  const handleCardAdd = (columnId: string) => {
    if (events?.onCardAdd) {
      eventHandler('OnCardAdd', id, [{ columnId }]);
    }
  };

  const handleCardDelete = (cardId: string) => {
    if (events?.onCardDelete) {
      eventHandler('OnCardDelete', id, [{ cardId }]);
    }
  };

  const handleDataChange = () => {
    // In a backend-first framework, we don't update local state
    // The backend will handle the data updates
  };

  // If no columns or tasks, show empty state
  if (extractedData.columns.length === 0 && extractedData.tasks.length === 0) {
    return (
      <div className="flex items-center justify-center p-8 text-gray-500">
        <div className="text-center">
          <p className="text-lg font-medium">No kanban data available</p>
          <p className="text-sm">
            The backend should provide columns and tasks data.
          </p>
        </div>
      </div>
    );
  }

  return (
    <KanbanProvider
      columns={extractedData.columns}
      data={extractedData.tasks}
      onDataChange={handleDataChange}
      onCardMove={handleCardMove}
      onCardAdd={handleCardAdd}
      onCardDelete={handleCardDelete}
    >
      {column => (
        <KanbanBoard id={column.id} key={column.id}>
          <KanbanHeader>
            <div className="flex items-center gap-2">
              <div
                className="h-3 w-3 rounded-full"
                style={{ backgroundColor: column.color }}
              />
              <span className="font-semibold text-gray-800">{column.name}</span>
              <span className="text-sm text-gray-500">
                (
                {
                  extractedData.tasks.filter(task => task.status === column.id)
                    .length
                }
                )
              </span>
            </div>
          </KanbanHeader>
          <KanbanCards id={column.id}>
            {(task: Task) => (
              <KanbanCard
                column={column.id}
                id={task.id}
                key={task.id}
                name={task.title}
                task={task}
              >
                <div className="flex items-start justify-between gap-2">
                  <div className="flex flex-col gap-1">
                    <p className="m-0 flex-1 font-medium text-sm">
                      {task.title}
                    </p>
                  </div>
                  {task.assignee && (
                    <div className="h-6 w-6 shrink-0 rounded-full bg-gray-200 flex items-center justify-center">
                      <span className="text-xs font-medium">
                        {task.assignee.slice(0, 2).toUpperCase()}
                      </span>
                    </div>
                  )}
                </div>
                <p className="m-0 text-muted-foreground text-xs">
                  Priority {task.priority} • {task.assignee}
                </p>
              </KanbanCard>
            )}
          </KanbanCards>
        </KanbanBoard>
      )}
    </KanbanProvider>
  );
};
