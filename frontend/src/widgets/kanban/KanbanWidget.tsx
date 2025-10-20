import React, { useCallback, useState } from 'react';
import {
  DndContext,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
  DragEndEvent,
  DragStartEvent,
  DragOverlay,
  DragOverEvent,
} from '@dnd-kit/core';
import { sortableKeyboardCoordinates } from '@dnd-kit/sortable';
import { useEventHandler } from '@/components/event-handler';

interface KanbanColumnProps {
  id: string;
  events?: string[];
  title?: string;
  columnKey?: string;
  showCounts?: boolean;
  allowAdd?: boolean;
  allowMove?: boolean;
  allowDelete?: boolean;
  onCardDelete?: (cardId: string) => void;
  children?: React.ReactElement[];
}

interface KanbanCardProps {
  cardId?: string;
  [key: string]: unknown;
}

interface KanbanWidgetProps {
  id: string;
  events: string[];
  showCounts?: boolean;
  allowAdd?: boolean;
  allowMove?: boolean;
  allowDelete?: boolean;
  children: React.ReactElement<KanbanColumnProps>[];
}

export const KanbanWidget: React.FC<KanbanWidgetProps> = ({
  id,
  events,
  showCounts = true,
  allowAdd = false,
  allowMove = false,
  allowDelete = false,
  children,
}) => {
  const eventHandler = useEventHandler();
  const [activeId, setActiveId] = useState<string | null>(null);
  const [activeCard, setActiveCard] = useState<React.ReactElement | null>(null);

  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: {
        distance: 10,
      },
    }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  const handleDragStart = useCallback(
    (event: DragStartEvent) => {
      const { active } = event;
      setActiveId(active.id as string);

      // Find the card being dragged
      const column = children.find(col => {
        const cards = col.props.children as
          | React.ReactElement<KanbanCardProps>[]
          | undefined;
        return cards?.some(card => card.props.cardId === active.id);
      });
      if (column) {
        const cards = column.props.children as
          | React.ReactElement<KanbanCardProps>[]
          | undefined;
        const card = cards?.find(card => card.props.cardId === active.id);
        setActiveCard(card || null);
      }
    },
    [children]
  );

  const handleDragOver = useCallback((event: DragOverEvent) => {
    const { active, over } = event;

    if (!over) return;

    const activeContainer = active.data.current?.sortable?.containerId;
    const overContainer = over.data.current?.sortable?.containerId || over.id;

    if (activeContainer !== overContainer) {
      // Moving between columns - we'll handle this in dragEnd
    }
  }, []);

  const handleDragEnd = useCallback(
    (event: DragEndEvent) => {
      const { active, over } = event;

      setActiveId(null);
      setActiveCard(null);

      if (!over) return;

      const activeContainer = active.data.current?.sortable?.containerId;
      const overContainer = over.data.current?.sortable?.containerId || over.id;

      if (activeContainer !== overContainer && allowMove) {
        // Card moved to a different column
        const fromColumn = children.find(
          col => col.props.columnKey === activeContainer
        );
        const toColumn = children.find(
          col => col.props.columnKey === overContainer
        );

        if (fromColumn && toColumn && events.includes('OnMove')) {
          eventHandler('OnMove', id, [
            {
              cardId: active.id,
              fromColumn: fromColumn.props.columnKey,
              toColumn: toColumn.props.columnKey,
            },
          ]);
        }
      }
    },
    [allowMove, children, eventHandler, events, id]
  );

  const handleCardDelete = useCallback(
    (cardId: string) => {
      if (allowDelete && events.includes('OnDelete')) {
        eventHandler('OnDelete', id, [cardId]);
      }
    },
    [allowDelete, eventHandler, events, id]
  );

  // Clone children to pass down props
  const enhancedChildren = React.Children.map(children, child => {
    if (React.isValidElement<KanbanColumnProps>(child)) {
      return React.cloneElement(child, {
        showCounts,
        allowAdd,
        allowMove,
        allowDelete,
        onCardDelete: handleCardDelete,
      });
    }
    return child;
  });

  return (
    <DndContext
      sensors={sensors}
      collisionDetection={closestCenter}
      onDragStart={handleDragStart}
      onDragOver={handleDragOver}
      onDragEnd={handleDragEnd}
    >
      <div className="flex gap-4 h-full overflow-x-auto p-4">
        {enhancedChildren}
        <DragOverlay>
          {activeId && activeCard ? (
            <div className="cursor-grabbing opacity-80">{activeCard}</div>
          ) : null}
        </DragOverlay>
      </div>
    </DndContext>
  );
};
