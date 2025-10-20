import React, { useCallback } from 'react';
import { useDroppable } from '@dnd-kit/core';
import {
  SortableContext,
  verticalListSortingStrategy,
} from '@dnd-kit/sortable';
import { cn } from '@/lib/utils';
import { Plus } from 'lucide-react';
import { useEventHandler } from '@/components/event-handler';
import { Button } from '@/components/ui/button';
import { ScrollArea } from '@/components/ui/scroll-area';

interface KanbanCardProps {
  id: string;
  cardId?: string;
  allowMove?: boolean;
  allowDelete?: boolean;
  onDelete?: (cardId: string) => void;
  columnKey?: string;
  children?: React.ReactNode;
}

interface KanbanColumnWidgetProps {
  id: string;
  events?: string[];
  title?: string;
  columnKey?: string;
  showCounts?: boolean;
  allowAdd?: boolean;
  allowMove?: boolean;
  allowDelete?: boolean;
  onCardDelete?: (cardId: string) => void;
  children?: React.ReactElement<KanbanCardProps>[];
}

export const KanbanColumnWidget: React.FC<KanbanColumnWidgetProps> = ({
  id,
  events = [],
  title,
  columnKey,
  showCounts = true,
  allowAdd = false,
  allowMove = false,
  allowDelete = false,
  onCardDelete,
  children = [],
}) => {
  const eventHandler = useEventHandler();
  const { setNodeRef, isOver } = useDroppable({
    id: columnKey || id,
  });

  const handleAddCard = useCallback(() => {
    if (allowAdd && events.includes('OnAdd')) {
      eventHandler('OnAdd', id, [columnKey]);
    }
  }, [allowAdd, columnKey, eventHandler, events, id]);

  // Extract card IDs for sortable context
  const cardIds = React.Children.map(children, child => {
    if (React.isValidElement<KanbanCardProps>(child) && child.props.cardId) {
      return child.props.cardId.toString();
    }
    return '';
  }).filter(Boolean);

  // Clone children to pass down props
  const enhancedChildren = React.Children.map(children, child => {
    if (React.isValidElement<KanbanCardProps>(child)) {
      return React.cloneElement(child, {
        allowMove,
        allowDelete,
        onDelete: onCardDelete,
        columnKey,
      });
    }
    return child;
  });

  return (
    <div
      ref={setNodeRef}
      className={cn(
        'flex flex-col bg-muted/50 rounded-lg p-4 min-w-[300px] max-w-[400px] h-full transition-colors',
        isOver && 'bg-muted/80 ring-2 ring-primary/50'
      )}
    >
      {/* Column Header */}
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center gap-2">
          <h3 className="font-semibold text-sm">{title || 'Untitled'}</h3>
          {showCounts && (
            <span className="text-xs text-muted-foreground bg-background px-2 py-1 rounded-full">
              {children.length}
            </span>
          )}
        </div>
        {allowAdd && (
          <Button
            variant="ghost"
            size="icon"
            className="h-6 w-6"
            onClick={handleAddCard}
          >
            <Plus className="h-4 w-4" />
          </Button>
        )}
      </div>

      {/* Cards Container */}
      <ScrollArea className="flex-1 -mx-2 px-2">
        <SortableContext
          items={cardIds}
          strategy={verticalListSortingStrategy}
          id={columnKey || id}
        >
          <div className="space-y-2 min-h-[50px]">{enhancedChildren}</div>
        </SortableContext>
      </ScrollArea>
    </div>
  );
};
