import React, { useRef } from "react";
import { useVirtualizer } from "@tanstack/react-virtual";
import { cn } from "@/lib/utils";
import { Densities } from "@/types/density";

type ListWidgetProps = {
  children: React.ReactNode;
  density?: Densities;
};

const estimateSizeMap: Record<Densities, number> = {
  [Densities.Small]: 44,
  [Densities.Medium]: 60,
  [Densities.Large]: 76,
};

export const ListWidget = ({ children, density = Densities.Medium }: ListWidgetProps) => {
  const parentRef = useRef<HTMLDivElement | null>(null);
  const childArray = React.Children.toArray(children);

  // eslint-disable-next-line react-hooks/incompatible-library
  const rowVirtualizer = useVirtualizer({
    count: childArray.length,
    getScrollElement: () => parentRef.current,
    estimateSize: () => estimateSizeMap[density],
    overscan: 6,
  });

  return (
    <div
      ref={parentRef}
      className={cn("relative h-full w-full overflow-y-auto remove-parent-padding")}
    >
      <div
        style={{
          height: rowVirtualizer.getTotalSize(),
          position: "relative",
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
                "absolute top-0 left-0 w-full flex items-center min-w-0",
                !isLast && "border-b border-border",
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
