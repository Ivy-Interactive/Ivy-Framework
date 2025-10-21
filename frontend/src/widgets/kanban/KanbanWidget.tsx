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
  slots?: {
    default?: React.ReactNode[];
  };
}

export const KanbanWidget: React.FC<KanbanWidgetProps> = ({
  id,
  columns = [],
  tasks = [],
  events,
  slots,
}) => {
  const eventHandler = useEventHandler();

  // Extract data from backend kanban structure
  const extractedData = React.useMemo(() => {
    // If we have slots with default children (backend kanban columns), parse them
    if (slots?.default && slots.default.length > 0) {
      const extractedTasks: Task[] = [];
      const extractedColumns: Column[] = [];

      // Parse the backend kanban structure
      slots.default.forEach((columnNode, columnIndex) => {
        if (React.isValidElement(columnNode)) {
          // Extract column data from KanbanColumn
          const columnProps = columnNode.props as {
            title?: string;
            columnKey?: string;
            children?: React.ReactNode[];
          };

          const columnTitle = columnProps?.title || `Column ${columnIndex + 1}`;
          const columnId = columnProps?.columnKey || columnTitle;

          // Create column
          const column: Column = {
            id: columnId,
            name: columnTitle,
            color:
              columnIndex === 0
                ? '#6B7280'
                : columnIndex === 1
                  ? '#F59E0B'
                  : '#10B981',
            order: columnIndex + 1,
          };
          extractedColumns.push(column);

          // Extract tasks from KanbanCard children
          const columnChildren = columnProps?.children;
          if (columnChildren && Array.isArray(columnChildren)) {
            columnChildren.forEach(
              (cardNode: React.ReactNode, cardIndex: number) => {
                if (React.isValidElement(cardNode)) {
                  const cardProps = cardNode.props as {
                    cardId?: string;
                    children?: React.ReactNode[];
                  };

                  // Extract card ID from the cardId prop
                  const cardId =
                    cardProps?.cardId || `task-${columnIndex}-${cardIndex}`;

                  // Extract title and description from card content
                  let title = `Task ${cardId}`;
                  let description = '';

                  // Try to extract title and description from card children
                  const cardChildren = cardProps?.children;
                  if (cardChildren && Array.isArray(cardChildren)) {
                    cardChildren.forEach((child: React.ReactNode) => {
                      if (React.isValidElement(child)) {
                        const childProps = child.props as {
                          title?: string;
                          description?: string;
                        };
                        if (childProps?.title) {
                          title = childProps.title;
                        }
                        if (childProps?.description) {
                          description = childProps.description;
                        }
                      }
                    });
                  }

                  const task: Task = {
                    id: cardId,
                    title: title,
                    status: columnId,
                    statusOrder: columnIndex + 1,
                    priority: cardIndex + 1,
                    description: description,
                    assignee: 'Unassigned',
                  };
                  extractedTasks.push(task);
                }
              }
            );
          }
        }
      });

      return { tasks: extractedTasks, columns: extractedColumns };
    }

    return { tasks, columns };
  }, [slots, tasks, columns]);

  const handleCardMove = (
    cardId: string,
    fromColumn: string,
    toColumn: string
  ) => {
    // Always trigger the backend event for card moves
    // The backend expects a tuple (CardId, FromColumn, ToColumn)
    eventHandler('OnMove', id, [cardId, fromColumn, toColumn]);
  };

  const handleCardAdd = (columnId: string) => {
    if (events?.onCardAdd) {
      eventHandler('OnCardAdd', id, [{ columnId }]);
    }
  };

  const handleCardDelete = (cardId: string) => {
    eventHandler('OnDelete', id, [{ cardId }]);
  };

  const handleCardReorder = (
    cardId: string,
    columnId: string,
    newIndex: number
  ) => {
    if (events?.onCardReorder) {
      eventHandler('OnCardReorder', id, [cardId, columnId, newIndex]);
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
      onCardReorder={handleCardReorder}
      onCardAdd={handleCardAdd}
      onCardDelete={handleCardDelete}
    >
      {column => (
        <KanbanBoard id={column.id} key={column.id}>
          <KanbanHeader>
            <div className="flex items-center gap-2 min-w-0">
              <div
                className="h-3 w-3 rounded-full flex-shrink-0"
                style={{ backgroundColor: column.color }}
              />
              <span className="font-semibold text-foreground truncate flex-1 min-w-0">
                {column.name}
              </span>
              <span className="text-sm text-muted-foreground flex-shrink-0">
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
              />
            )}
          </KanbanCards>
        </KanbanBoard>
      )}
    </KanbanProvider>
  );
};
