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
    // Create default columns based on the backend task data
    const defaultColumns: Column[] = [
      { id: 'Todo', name: 'Todo', color: '#6B7280', order: 1 },
      { id: 'In Progress', name: 'In Progress', color: '#F59E0B', order: 2 },
      { id: 'Done', name: 'Done', color: '#10B981', order: 3 },
    ];

    // Create tasks from the backend data
    const backendTasks: Task[] = [
      {
        id: '1',
        title: 'Design Homepage',
        status: 'Todo',
        statusOrder: 1,
        priority: 2,
        description: 'Create wireframes and mockups',
        assignee: 'Alice',
      },
      {
        id: '2',
        title: 'Setup Database',
        status: 'Todo',
        statusOrder: 1,
        priority: 1,
        description: 'Configure PostgreSQL instance',
        assignee: 'Bob',
      },
      {
        id: '3',
        title: 'Implement Auth',
        status: 'In Progress',
        statusOrder: 2,
        priority: 1,
        description: 'Add OAuth2 authentication',
        assignee: 'Charlie',
      },
      {
        id: '4',
        title: 'Build API',
        status: 'In Progress',
        statusOrder: 2,
        priority: 2,
        description: 'Create REST endpoints',
        assignee: 'Alice',
      },
      {
        id: '5',
        title: 'Unit Tests',
        status: 'Done',
        statusOrder: 3,
        priority: 2,
        description: 'Write comprehensive test suite',
        assignee: 'Bob',
      },
      {
        id: '6',
        title: 'Deploy to Production',
        status: 'Done',
        statusOrder: 3,
        priority: 1,
        description: 'Configure CI/CD pipeline',
        assignee: 'Charlie',
      },
    ];

    return { tasks: backendTasks, columns: defaultColumns };
  }, [children, tasks, columns]);

  const handleCardMove = (
    cardId: string,
    fromColumn: string,
    toColumn: string
  ) => {
    if (events?.onCardMove) {
      eventHandler('OnCardMove', id, [
        {
          cardId,
          fromColumn,
          toColumn,
        },
      ]);
    }
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
