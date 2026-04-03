import React from "react";
import { getWidth } from "@/lib/styles";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";

interface ProgressSegment {
  value: number;
  color?: string;
  label?: string;
}

interface StackedProgressWidgetProps {
  id: string;
  segments?: ProgressSegment[];
  barHeight?: number;
  showLabels?: boolean;
  rounded?: boolean;
  width?: string;
}

export const StackedProgressWidget: React.FC<StackedProgressWidgetProps> = ({
  segments = [],
  barHeight = 8,
  showLabels = false,
  rounded = true,
  width = "Full",
}) => {
  const total = segments.reduce((sum, s) => sum + s.value, 0);

  if (total === 0) {
    return (
      <div
        className="bg-neutral/10"
        style={{
          ...getWidth(width),
          height: `${barHeight}px`,
          borderRadius: rounded ? `${barHeight / 2}px` : undefined,
        }}
      />
    );
  }

  const containerStyles: React.CSSProperties = {
    ...getWidth(width),
    height: `${barHeight}px`,
    display: "flex",
    overflow: "hidden",
    borderRadius: rounded ? `${barHeight / 2}px` : undefined,
  };

  return (
    <TooltipProvider>
      <div className="flex flex-col gap-1" style={getWidth(width)}>
        <div className="bg-neutral/10" style={containerStyles}>
          {segments.map((segment, index) => {
            const percentage = (segment.value / total) * 100;
            const color = segment.color
              ? `var(--${segment.color.toLowerCase()})`
              : "var(--primary)";

            const segmentEl = (
              <div
                key={index}
                className="transition-all duration-300 ease-in-out"
                style={{
                  width: `${percentage}%`,
                  height: "100%",
                  backgroundColor: color,
                  minWidth: segment.value > 0 ? "2px" : 0,
                }}
              />
            );

            if (segment.label || segment.value > 0) {
              return (
                <Tooltip key={index}>
                  <TooltipTrigger asChild>{segmentEl}</TooltipTrigger>
                  <TooltipContent>
                    <span>
                      {segment.label ? `${segment.label}: ${segment.value}` : `${segment.value}`}
                    </span>
                  </TooltipContent>
                </Tooltip>
              );
            }

            return segmentEl;
          })}
        </div>
        {showLabels && (
          <div className="flex gap-3 flex-wrap">
            {segments
              .filter((s) => s.label)
              .map((segment, index) => {
                const color = segment.color
                  ? `var(--${segment.color.toLowerCase()})`
                  : "var(--primary)";
                return (
                  <div
                    key={index}
                    className="flex items-center gap-1.5 text-xs text-muted-foreground"
                  >
                    <div
                      className="rounded-full"
                      style={{
                        width: "8px",
                        height: "8px",
                        backgroundColor: color,
                      }}
                    />
                    <span>{segment.label}</span>
                    <span className="font-medium text-foreground">{segment.value}</span>
                  </div>
                );
              })}
          </div>
        )}
      </div>
    </TooltipProvider>
  );
};
