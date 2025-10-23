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
}

interface KanbanWidgetProps {
  id: string;
  columns?: Column[];
  tasks?: Task[];
  events?: Record<string, unknown>;
  width?: string;
  height?: string;
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
  slots,
}) => {
  const eventHandler = useEventHandler();
  // Extract data from backend kanban structure
  const extractedData = React.useMemo(() => {
    if (slots?.default && slots.default.length > 0) {
      const extractedTasks: Task[] = [];
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
                const task: Task = {
                  id: cardProps?.cardId as string,
                  title: taskData?.title as string,
                  status: columnProps?.columnKey as string,
                  statusOrder: columnIndex + 1,
                  priority: cardProps?.priority as number,
                  description: taskData?.description as string,
                  assignee: (taskData?.assignee as string) || '',
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
    // Send move event to backend with column keys
    eventHandler('OnMove', id, [cardId, fromColumn, toColumn, targetIndex]);
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
    overflow: 'hidden', // Prevent the container from scrolling
  };

  return (
    <div style={styles}>
      <Kanban
        data={extractedData.tasks}
        columns={extractedData.columns}
        onCardMove={handleCardMove}
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
              >
                <KanbanCards id={column.id}>
                  {(task: Task) => (
                    <KanbanCard key={task.id} id={task.id} column={column.id}>
                      <Card>
                        <CardHeader>
                          <KanbanHeader>
                            <div className="flex items-center justify-between gap-2">
                              <div className="flex items-center gap-2">
                                <CardTitle className="text-sm">
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
                            <p className="text-xs text-muted-foreground">
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
