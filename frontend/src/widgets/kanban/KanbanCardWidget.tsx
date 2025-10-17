import React, { useState } from 'react';
import { useEventHandler } from '@/components/event-handler';
import { cn } from '@/lib/utils';
import { GripVertical, X } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';

interface KanbanCardWidgetProps {
  id: string;
  events: string[];
  cardId?: string;
  allowDelete?: boolean;
  'data-testid'?: string;
  slots?: {
    children?: React.ReactNode[];
  };
}

export const KanbanCardWidget: React.FC<KanbanCardWidgetProps> = ({
  id,
  events,
  cardId,
  allowDelete = false,
  slots,
  'data-testid': testId = 'kanban-card',
}) => {
  const eventHandler = useEventHandler();
  const [isDragging, setIsDragging] = useState(false);

  const handleDragStart = (e: React.DragEvent<HTMLDivElement>) => {
    setIsDragging(true);
    e.dataTransfer.setData('cardId', cardId || '');

    // Find the parent column to get the column key
    const columnElement = e.currentTarget.closest('[data-column-key]');
    if (columnElement) {
      const columnKey = columnElement.getAttribute('data-column-key');
      if (columnKey) {
        e.dataTransfer.setData('fromColumn', columnKey);
      }
    }

    e.dataTransfer.effectAllowed = 'move';
  };

  const handleDragEnd = () => {
    setIsDragging(false);
  };

  const handleDelete = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (events.includes('OnDelete') && cardId) {
      eventHandler('OnDelete', id, [cardId]);
    }
  };

  return (
    <Card
      data-testid={testId}
      data-card-id={cardId}
      draggable
      onDragStart={handleDragStart}
      onDragEnd={handleDragEnd}
      className={cn(
        'group cursor-grab active:cursor-grabbing transition-all duration-200',
        'hover:shadow-md border-border/50',
        isDragging && 'opacity-50 rotate-3 scale-105'
      )}
    >
      <CardContent className="p-4">
        <div className="flex items-start justify-between gap-2">
          <div className="flex-1 min-w-0">{slots?.children}</div>
          <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
            <GripVertical className="h-4 w-4 text-muted-foreground cursor-grab" />
            {allowDelete && (
              <Button
                size="sm"
                variant="ghost"
                onClick={handleDelete}
                className="h-6 w-6 p-0 hover:bg-destructive hover:text-destructive-foreground"
              >
                <X className="h-3 w-3" />
              </Button>
            )}
          </div>
        </div>
      </CardContent>
    </Card>
  );
};
