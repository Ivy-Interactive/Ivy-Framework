import React, { useRef } from 'react';
import { useVirtualizer } from '@tanstack/react-virtual';
import { cn } from '@/lib/utils';

type ListWidgetProps = {
  children: React.ReactNode;
};

export const ListWidget = ({ children }: ListWidgetProps) => {
  const parentRef = useRef<HTMLDivElement | null>(null);
  const childArray = React.Children.toArray(children);

  const rowVirtualizer = useVirtualizer({
    count: childArray.length,
    getScrollElement: () => parentRef.current,
    estimateSize: () => 60,
  });

  return (
    <div
      ref={parentRef}
      className="relative h-full w-full overflow-y-auto remove-parent-padding"
    >
      <div
        style={{
          height: rowVirtualizer.getTotalSize(),
          position: 'relative',
        }}
      >
        {rowVirtualizer.getVirtualItems().map((virtualRow, index) => {
          const child = childArray[virtualRow.index];
          return (
            <div
              key={virtualRow.key}
              className={cn(
                'absolute top-0 left-0 w-full flex items-center',
                index !== rowVirtualizer.getVirtualItems().length - 1 &&
                  'border-b border-border'
              )}
              style={{
                height: `${virtualRow.size}px`,
                transform: `translateY(${virtualRow.start}px)`,
              }}
            >
              {child}
            </div>
          );
        })}
      </div>
    </div>
  );
};
