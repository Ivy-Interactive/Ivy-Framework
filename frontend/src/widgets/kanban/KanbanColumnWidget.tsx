import React from 'react';
import { useEventHandler } from '@/components/event-handler';
import { Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';

interface KanbanColumnWidgetProps {
  id: string;
  events: string[];
  title?: string;
  columnKey?: string;
  'data-testid'?: string;
  slots?: {
    children?: React.ReactNode[];
  };
}

export const KanbanColumnWidget: React.FC<KanbanColumnWidgetProps> = ({
  id,
  events,
  title,
  columnKey,
  slots,
  'data-testid': testId = 'kanban-column',
}) => {
  const eventHandler = useEventHandler();
  const cardCount = slots?.children?.length || 0;

  const handleAddCard = () => {
    if (events.includes('OnAdd') && columnKey) {
      eventHandler('OnAdd', id, [columnKey]);
    }
  };

  const handleDrop = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
  };

  const handleDragOver = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
  };

  return (
    <div
      data-testid={testId}
      data-column-key={columnKey}
      className="bg-muted/50 rounded-lg p-4 min-h-[400px] flex flex-col"
      onDrop={handleDrop}
      onDragOver={handleDragOver}
    >
      {/* Column Header */}
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center gap-2">
          {title && (
            <h3 className="font-semibold text-sm text-muted-foreground uppercase tracking-wide">
              {title}
            </h3>
          )}
          <Badge variant="secondary" className="text-xs">
            {cardCount}
          </Badge>
        </div>
        {events.includes('OnAdd') && (
          <Button
            size="sm"
            variant="ghost"
            onClick={handleAddCard}
            className="h-6 w-6 p-0"
          >
            <Plus className="h-4 w-4" />
          </Button>
        )}
      </div>

      {/* Cards Container */}
      <div className="flex-1 space-y-3">{slots?.children}</div>
    </div>
  );
};
