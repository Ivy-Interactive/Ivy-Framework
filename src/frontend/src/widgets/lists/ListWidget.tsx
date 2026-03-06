import React, { useRef, useState } from 'react';
import { useVirtualizer } from '@tanstack/react-virtual';
import {
  DndContext,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
  type DragEndEvent,
} from '@dnd-kit/core';
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
  useSortable,
  verticalListSortingStrategy,
} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { cn } from '@/lib/utils';
import { useEventHandler } from '@/components/event-handler';

type ListWidgetProps = {
  id: string;
  children: React.ReactNode;
  reorderable?: boolean;
};

interface SortableItemProps {
  id: string;
  children: React.ReactNode;
  isLast: boolean;
}

function SortableItem({ id, children, isLast }: SortableItemProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id });

  const style: React.CSSProperties = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1,
    zIndex: isDragging ? 100 : undefined,
  };

  return (
    <div
      ref={setNodeRef}
      style={style}
      className={cn(
        'w-full flex items-center min-w-0 cursor-grab active:cursor-grabbing',
        !isLast && 'border-b border-border'
      )}
      {...attributes}
      {...listeners}
    >
      {children}
    </div>
  );
}

export const ListWidget = ({
  id,
  children,
  reorderable = false,
}: ListWidgetProps) => {
  const parentRef = useRef<HTMLDivElement | null>(null);
  const childArray = React.Children.toArray(children);
  const eventHandler = useEventHandler();

  const getChildId = (child: React.ReactNode, index: number): string => {
    if (React.isValidElement<{ id?: string }>(child) && child.props.id) {
      return child.props.id;
    }
    return `item-${index}`;
  };

  const initialItems = childArray.map((child, index) =>
    getChildId(child, index)
  );
  const [items, setItems] = useState(initialItems);

  const sensors = useSensors(
    useSensor(PointerSensor),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;

    if (over && active.id !== over.id) {
      setItems(prevItems => {
        const oldIndex = prevItems.indexOf(active.id as string);
        const newIndex = prevItems.indexOf(over.id as string);
        const newItems = arrayMove(prevItems, oldIndex, newIndex);
        eventHandler('OnReorder', id, [newItems]);
        return newItems;
      });
    }
  };

  // eslint-disable-next-line react-hooks/incompatible-library
  const rowVirtualizer = useVirtualizer({
    count: childArray.length,
    getScrollElement: () => parentRef.current,
    estimateSize: () => 60,
    overscan: 6,
  });

  if (reorderable) {
    const sortedChildren = items.map(itemId => {
      const index = initialItems.indexOf(itemId);
      return index >= 0 ? childArray[index] : null;
    });

    return (
      <DndContext
        sensors={sensors}
        collisionDetection={closestCenter}
        onDragEnd={handleDragEnd}
      >
        <SortableContext items={items} strategy={verticalListSortingStrategy}>
          <div
            ref={parentRef}
            className={cn(
              'relative h-full w-full overflow-y-auto remove-parent-padding'
            )}
          >
            {sortedChildren.map((child, index) => {
              const itemId = items[index];
              const isLast = index === sortedChildren.length - 1;
              return (
                <SortableItem key={itemId} id={itemId} isLast={isLast}>
                  {child}
                </SortableItem>
              );
            })}
          </div>
        </SortableContext>
      </DndContext>
    );
  }

  return (
    <div
      ref={parentRef}
      className={cn(
        'relative h-full w-full overflow-y-auto remove-parent-padding'
      )}
    >
      <div
        style={{
          height: rowVirtualizer.getTotalSize(),
          position: 'relative',
        }}
      >
        {rowVirtualizer.getVirtualItems().map((virtualRow, index) => {
          const child = childArray[virtualRow.index];
          const isLast = index === rowVirtualizer.getVirtualItems().length - 1;
          return (
            <div
              key={virtualRow.key}
              data-index={virtualRow.index}
              className={cn(
                'absolute top-0 left-0 w-full flex items-center min-w-0',
                !isLast && 'border-b border-border'
              )}
              style={{
                transform: `translateY(${virtualRow.start}px)`,
              }}
              ref={rowVirtualizer.measureElement}
            >
              {child}
            </div>
          );
        })}
      </div>
    </div>
  );
};
