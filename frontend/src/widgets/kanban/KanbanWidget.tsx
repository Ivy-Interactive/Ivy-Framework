import React from 'react';
import { Kanban, type Task } from '@/components/ui/shadcn-io/kanban';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { useEventHandler } from '@/components/event-handler';
import { getWidth, getHeight } from '@/lib/styles';

interface Column {
  id: string;
  name: string;
  color: string;
  order: number;
  widgetId: string;
  width?: string;
}

interface TaskWithWidgetId extends Task {
  widgetId: string;
}

interface KanbanWidgetProps {
  id: string;
  columns?: Column[];
  tasks?: Task[];
  events?: Record<string, unknown>;
  width?: string;
  height?: string;
  allowDelete?: boolean;
  children?: React.ReactNode;
  slots?: {
    default?: React.ReactNode[];
  };
}

export const KanbanWidget: React.FC<KanbanWidgetProps> = ({
  id,
  columns = [],
  tasks = [],
  width,
  height,
  allowDelete = false,
  slots,
}) => {
  const eventHandler = useEventHandler();
  // Extract data from backend kanban structure
  const extractedData = React.useMemo(() => {
    if (slots?.default && slots.default.length > 0) {
      const extractedTasks: TaskWithWidgetId[] = [];
      const extractedColumns: Column[] = [];

      // Parse the backend kanban structure
      slots.default.forEach((columnNode, columnIndex) => {
        if (React.isValidElement(columnNode)) {
          // The actual column props are nested in children.props
          const columnProps = (
            columnNode.props as {
              children?: { props?: Record<string, unknown> };
            }
          )?.children?.props as Record<string, unknown>;

          // Create column from backend data
          const column: Column = {
            id: columnProps?.columnKey as string,
            name: columnProps?.title as string,
            color: columnProps?.color as string,
            order: (columnProps?.order as number) || 999,
            widgetId:
              (columnProps?.id as string) || (columnProps?.columnKey as string),
            width: columnProps?.width as string | undefined,
          };
          extractedColumns.push(column);

          // Extract tasks from column children - they're in slots.default
          const columnSlots = (
            columnProps?.slots as { default?: React.ReactNode[] }
          )?.default;
          if (columnSlots && Array.isArray(columnSlots)) {
            columnSlots.forEach((cardNode: React.ReactNode) => {
              if (React.isValidElement(cardNode)) {
                // Cards are wrapped in Suspense, need to go deeper
                const cardProps = (
                  cardNode.props as {
                    children?: { props?: Record<string, unknown> };
                  }
                )?.children?.props as Record<string, unknown>;

                // Extract task data from slots.default
                const taskSlots = (
                  cardProps?.slots as { default?: React.ReactNode[] }
                )?.default;
                let taskData: Record<string, unknown> = {};
                if (
                  taskSlots &&
                  Array.isArray(taskSlots) &&
                  taskSlots.length > 0
                ) {
                  const taskSlot = taskSlots[0];
                  if (React.isValidElement(taskSlot)) {
                    taskData = taskSlot.props as Record<string, unknown>;
                  }
                }

                // Create task from backend data
                const task: TaskWithWidgetId = {
                  id: cardProps?.cardId as string,
                  title: taskData?.title as string,
                  status: columnProps?.columnKey as string,
                  statusOrder: columnIndex + 1,
                  priority: cardProps?.priority as number,
                  description: taskData?.description as string,
                  assignee: (taskData?.assignee as string) || '',
                  widgetId: cardProps?.id as string, // Store the card widget ID
                };
                extractedTasks.push(task);
              }
            });
          }
        }
      });

      // Use columns in the order they come from the backend (already sorted by ColumnOrder)
      const sortedColumns = extractedColumns;

      return { tasks: extractedTasks, columns: sortedColumns };
    }

    return { tasks, columns };
  }, [slots, tasks, columns]);

  const handleCardMove = (
    cardId: string,
    fromColumn: string,
    toColumn: string,
    targetIndex?: number
  ) => {
    eventHandler('OnMove', id, [cardId, fromColumn, toColumn, targetIndex]);
  };

  const handleCardClick = (cardId: string) => {
    // Find the card widget ID for this task
    const task = extractedData.tasks.find(t => t.id === cardId) as
      | TaskWithWidgetId
      | undefined;
    if (task?.widgetId) {
      eventHandler('OnClick', task.widgetId, [cardId]);
    }
  };

  const handleCardDelete = (cardId: string) => {
    eventHandler('OnDelete', id, [cardId]);
  };

  if (extractedData.tasks.length === 0 && extractedData.columns.length === 0) {
    return (
      <div className="flex items-center justify-center p-8 text-gray-500">
        <div className="text-center">
          <p className="text-lg font-medium">No kanban data available</p>
          <p className="text-sm">
            The backend did not provide any kanban data to display.
          </p>
        </div>
      </div>
    );
  }

  const styles = {
    ...getWidth(width),
    ...getHeight(height),
    overflowY: 'hidden' as const,
    overflowX: 'auto' as const, // Allow horizontal scrolling when content is wider
  };

  return (
    <div style={styles}>
      <Kanban
        data={extractedData.tasks}
        columns={extractedData.columns}
        onCardMove={handleCardMove}
        onCardClick={handleCardClick}
        onCardDelete={allowDelete ? handleCardDelete : undefined}
      >
        {({
          KanbanBoard,
          KanbanColumn,
          KanbanCards,
          KanbanCard,
          KanbanHeader,
          KanbanCardContent,
        }) => (
          <KanbanBoard>
            {extractedData.columns.map(column => (
              <KanbanColumn
                key={column.id}
                id={column.id}
                name={column.name}
                color={column.color}
                width={column.width}
              >
                <KanbanCards id={column.id}>
                  {(task: Task) => (
                    <KanbanCard key={task.id} id={task.id} column={column.id}>
                      <Card>
                        <CardHeader>
                          <KanbanHeader>
                            <div className="flex items-center justify-between gap-2">
                              <div className="flex items-center gap-2">
                                <CardTitle
                                  className="text-sm cursor-pointer hover:underline hover:text-primary transition-colors"
                                  onClick={(e: React.MouseEvent) => {
                                    e.stopPropagation();
                                    handleCardClick(task.id);
                                  }}
                                >
                                  {task.title}
                                </CardTitle>
                              </div>
                              <Badge variant="secondary">
                                P{task.priority}
                              </Badge>
                            </div>
                          </KanbanHeader>
                        </CardHeader>
                        <CardContent>
                          <KanbanCardContent>
                            <p className="text-xs text-muted-foreground whitespace-pre-line">
                              {task.description}
                            </p>
                            {task.assignee && (
                              <p className="text-xs text-muted-foreground">
                                Assignee: {task.assignee}
                              </p>
                            )}
                          </KanbanCardContent>
                        </CardContent>
                      </Card>
                    </KanbanCard>
                  )}
                </KanbanCards>
              </KanbanColumn>
            ))}
          </KanbanBoard>
        )}
      </Kanban>
    </div>
  );
};
