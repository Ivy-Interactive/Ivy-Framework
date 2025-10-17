import React from 'react';
import { useEventHandler } from '@/components/event-handler';

interface KanbanWidgetProps {
  id: string;
  events: string[];
  showCounts?: boolean;
  allowAdd?: boolean;
  allowMove?: boolean;
  allowDelete?: boolean;
  'data-testid'?: string;
  slots?: {
    children?: React.ReactNode[];
  };
}

export const KanbanWidget: React.FC<KanbanWidgetProps> = ({
  id,
  events,
  allowMove = false,
  slots,
  'data-testid': testId = 'kanban-board',
}) => {
  const eventHandler = useEventHandler();

  const handleDrop = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    const cardId = e.dataTransfer.getData('cardId');
    const fromColumn = e.dataTransfer.getData('fromColumn');
    const toColumn = e.currentTarget.getAttribute('data-column-key');

    if (
      cardId &&
      fromColumn &&
      toColumn &&
      fromColumn !== toColumn &&
      allowMove
    ) {
      if (events.includes('OnMove')) {
        eventHandler('OnMove', id, [
          { CardId: cardId, FromColumn: fromColumn, ToColumn: toColumn },
        ]);
      }
    }
  };

  const handleDragOver = (e: React.DragEvent<HTMLDivElement>) => {
    if (allowMove) {
      e.preventDefault();
    }
  };

  return (
    <div
      data-testid={testId}
      className="flex gap-6 overflow-x-auto p-6 min-h-[400px]"
    >
      {slots?.children?.map((column, index) => (
        <div
          key={index}
          className="flex-shrink-0 w-80"
          onDrop={handleDrop}
          onDragOver={handleDragOver}
        >
          {column}
        </div>
      ))}
    </div>
  );
};
