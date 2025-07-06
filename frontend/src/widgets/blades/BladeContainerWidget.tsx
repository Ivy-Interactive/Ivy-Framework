import { ScrollArea } from '@radix-ui/react-scroll-area'; //todo: why is it not working with the shadcn component?
import React, { useEffect, useRef } from 'react';

interface BladeContainerWidgetProps {
  children: React.ReactNode;
}

export const BladeContainerWidget: React.FC<BladeContainerWidgetProps> = ({
  children,
}) => {
  const scrollRef = useRef<HTMLDivElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const container = containerRef.current;
    if (!container || !scrollRef.current) return;

    const resizeObserver = new ResizeObserver(entries => {
      for (const entry of entries) {
        const containerWidth = entry.target.clientWidth;
        const scrollPosition = containerWidth - scrollRef.current!.clientWidth;
        if (scrollPosition > 0) {
          scrollRef.current!.scrollTo({
            left: scrollPosition,
            behavior: 'smooth',
          });
        }
      }
    });
    resizeObserver.observe(container);
    return () => {
      resizeObserver.disconnect();
    };
  }, []);

  return (
    <div className="bg-gray-300 remove-ancestor-padding h-screen w-screen">
      <ScrollArea
        ref={scrollRef}
        className="h-full w-full overflow-y-hidden"
        type="hover"
      >
        <div className="flex w-max" ref={containerRef}>
          {children}
        </div>
      </ScrollArea>
    </div>
  );
};
