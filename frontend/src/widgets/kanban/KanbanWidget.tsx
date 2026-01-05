import React from 'react';
import { Kanban, type Task } from '@/components/ui/shadcn-io/kanban';
import { getWidth, getHeight } from '@/lib/styles';
import { useKanbanData } from './useKanbanData';
import { useKanbanHandlers } from './useKanbanHandlers';
import { KanbanEmptyState } from './KanbanEmptyState';
import { KanbanCardRenderer } from './KanbanCardRenderer';
import type { KanbanWidgetProps } from './types';

export const KanbanWidget: React.FC<KanbanWidgetProps> = ({
  id,
  columns = [],
  tasks = [],
  width,
  height,
  columnWidth,
  showCounts = true,
  slots,
  widgetNodeChildren,
}) => {
  const extractedData = useKanbanData(
    slots,
    tasks,
    columns,
    widgetNodeChildren
  );
  const { handleCardMove } = useKanbanHandlers(id);

  const sortedColumns = React.useMemo(() => {
    return [...extractedData.columns].sort((a, b) => {
      const orderA = a.order ?? Number.MAX_SAFE_INTEGER;
      const orderB = b.order ?? Number.MAX_SAFE_INTEGER;
      return orderA - orderB;
    });
  }, [extractedData.columns]);

  if (extractedData.tasks.length === 0 && sortedColumns.length === 0) {
    return <KanbanEmptyState />;
  }

  const styles = {
    ...getWidth(width),
    ...getHeight(height),
    overflowY: 'hidden' as const,
    overflowX: 'auto' as const,
    maxWidth: '100%',
    boxSizing: 'border-box' as const,
  };

  return (
    <div style={styles}>
      <Kanban
        data={extractedData.tasks}
        columns={extractedData.columns}
        onCardMove={handleCardMove}
        showCounts={showCounts}
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
            {sortedColumns.map(column => (
              <KanbanColumn
                key={column.id}
                id={column.id}
                name={column.name}
                color={column.color}
                width={columnWidth}
              >
                <KanbanCards id={column.id}>
                  {(task: Task, index: number) => {
                    const card = extractedData.cards.find(
                      c => c.cardId === task.id
                    );

                    return (
                      <KanbanCardRenderer
                        key={task.id}
                        task={task}
                        index={index}
                        card={card}
                        KanbanCard={KanbanCard}
                        KanbanHeader={KanbanHeader}
                        KanbanCardContent={KanbanCardContent}
                      />
                    );
                  }}
                </KanbanCards>
              </KanbanColumn>
            ))}
          </KanbanBoard>
        )}
      </Kanban>
    </div>
  );
};
