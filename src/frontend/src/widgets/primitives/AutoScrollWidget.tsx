import { useAutoScroll } from "@/hooks/use-auto-scroll";
import { getHeight, getWidth } from "@/lib/styles";
import React from "react";

interface AutoScrollWidgetProps {
  id: string;
  children?: React.ReactNode;
  enabled?: boolean;
  width?: string;
  height?: string;
}

export const AutoScrollWidget: React.FC<AutoScrollWidgetProps> = ({
  id,
  children,
  enabled = true,
  width,
  height,
}) => {
  const { scrollRef, disableAutoScroll } = useAutoScroll({
    content: children,
    enabled,
    smooth: false,
  });

  return (
    <div
      id={id}
      className="min-h-0 flex min-w-0 flex-col"
      style={{ ...getWidth(width), ...getHeight(height) }}
    >
      <div
        ref={scrollRef}
        className="min-h-0 min-w-0 flex-1 overflow-x-hidden overflow-y-auto"
        onWheel={enabled ? disableAutoScroll : undefined}
        onTouchMove={enabled ? disableAutoScroll : undefined}
      >
        <div className="flex min-w-0 flex-col">{children}</div>
      </div>
    </div>
  );
};

AutoScrollWidget.displayName = "AutoScrollWidget";
