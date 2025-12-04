import React, { useRef, useLayoutEffect } from 'react';
import { useVirtualizer } from '@tanstack/react-virtual';
import { cn } from '@/lib/utils';

type ListWidgetProps = {
  children: React.ReactNode;
};

export const ListWidget = ({ children }: ListWidgetProps) => {
  const parentRef = useRef<HTMLDivElement | null>(null);
  const childArray = React.Children.toArray(children);

  // Add remove-parent-padding class to the immediate parent (flex container)
  useLayoutEffect(() => {
    const parentElement = parentRef.current?.parentElement;
    if (parentElement) {
      parentElement.classList.add('remove-parent-padding');
      return () => {
        parentElement.classList.remove('remove-parent-padding');
      };
    }
  }, []);

  const rowVirtualizer = useVirtualizer({
    count: childArray.length,
    getScrollElement: () => parentRef.current,
    estimateSize: () => 60,
    // Allow dynamic measurement so rows expand to fit multi-line content
    measureElement: el => el.getBoundingClientRect().height,
    overscan: 6,
  });

  return (
    <div
      ref={parentRef}
      className="remove-parent-padding relative h-full w-full overflow-y-auto"
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
              className={cn(
                'absolute top-0 left-0 w-full flex items-center min-w-0',
                !isLast && 'border-b border-border'
              )}
              style={{
                height: `${virtualRow.size}px`,
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
