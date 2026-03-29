import { useAutoScroll } from "@/hooks/use-auto-scroll";
import { getHeight, getWidth } from "@/lib/styles";
import React from "react";

interface AutoScrollWidgetProps {
  id: string;
  children?: React.ReactNode;
  /** When true, auto-follow is off and the user scrolls manually (default false). */
  disabled?: boolean;
  width?: string;
  height?: string;
}

export const AutoScrollWidget: React.FC<AutoScrollWidgetProps> = ({
  id,
  children,
  disabled = false,
  width,
  height,
}) => {
  const autoFollow = !disabled;
  const { scrollRef, disableAutoScroll } = useAutoScroll({
    content: children,
    enabled: autoFollow,
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
        onWheel={autoFollow ? disableAutoScroll : undefined}
        onTouchMove={autoFollow ? disableAutoScroll : undefined}
      >
        <div className="flex min-w-0 flex-col">{children}</div>
      </div>
    </div>
  );
};

AutoScrollWidget.displayName = "AutoScrollWidget";
