import React, { useCallback } from 'react';
import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { cn } from '@/lib/utils';
import { GripVertical, X } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';

interface KanbanCardWidgetProps {
  id: string;
  cardId?: string;
  allowMove?: boolean;
  allowDelete?: boolean;
  onDelete?: (cardId: string) => void;
  columnKey?: string;
  children?: React.ReactNode;
}

export const KanbanCardWidget: React.FC<KanbanCardWidgetProps> = ({
  id,
  cardId,
  allowMove = false,
  allowDelete = false,
  onDelete,
  columnKey,
  children,
}) => {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({
    id: cardId || id,
    data: {
      sortable: {
        containerId: columnKey,
      },
    },
  });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
  };

  const handleDelete = useCallback(
    (e: React.MouseEvent) => {
      e.stopPropagation();
      if (onDelete && cardId) {
        onDelete(cardId);
      }
    },
    [onDelete, cardId]
  );

  return (
    <div
      ref={setNodeRef}
      style={style}
      className={cn('relative group', isDragging && 'opacity-50 z-50')}
    >
      <Card
        className={cn(
          'p-3 cursor-pointer hover:shadow-md transition-shadow',
          allowMove && 'cursor-grab active:cursor-grabbing',
          isDragging && 'shadow-lg'
        )}
        {...(allowMove ? { ...attributes, ...listeners } : {})}
      >
        <div className="flex items-start gap-2">
          {allowMove && (
            <GripVertical className="h-4 w-4 text-muted-foreground opacity-0 group-hover:opacity-100 transition-opacity flex-shrink-0 mt-0.5" />
          )}
          <div className="flex-1 min-w-0">{children}</div>
          {allowDelete && (
            <Button
              variant="ghost"
              size="icon"
              className="h-6 w-6 opacity-0 group-hover:opacity-100 transition-opacity flex-shrink-0"
              onClick={handleDelete}
            >
              <X className="h-3 w-3" />
            </Button>
          )}
        </div>
      </Card>
    </div>
  );
};
