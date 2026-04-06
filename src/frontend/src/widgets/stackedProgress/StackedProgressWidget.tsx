import React from "react";
import { useEventHandler } from "@/components/event-handler";
import { getWidth } from "@/lib/styles";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";
import { cn } from "@/lib/utils";

const EMPTY_ARRAY: never[] = [];

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
  selected?: number;
  width?: string;
  events?: string[];
}

export const StackedProgressWidget: React.FC<StackedProgressWidgetProps> = ({
  id,
  segments = [],
  barHeight = 8,
  showLabels = false,
  rounded = true,
  selected,
  width = "Full",
  events = EMPTY_ARRAY,
}) => {
  const eventHandler = useEventHandler();
  const hasSelectHandler = events.includes("OnSelect");
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

  const handleSelect = (index: number) => {
    if (hasSelectHandler) {
      eventHandler("OnSelect", id, [index]);
    }
  };

  // Map from filtered label index back to the original segment index
  const labelSegments = segments
    .map((segment, index) => ({ segment, originalIndex: index }))
    .filter(({ segment }) => segment.label);

  return (
    <TooltipProvider>
      <div className="flex flex-col gap-1" style={getWidth(width)}>
        <div className="bg-neutral/10" style={containerStyles}>
          {segments.map((segment, index) => {
            const percentage = (segment.value / total) * 100;
            const color = segment.color
              ? `var(--${segment.color.toLowerCase()})`
              : "var(--primary)";
            const isSelected = selected === index;

            const segmentEl = (
              <div
                key={index}
                role={hasSelectHandler ? "button" : undefined}
                aria-label={hasSelectHandler ? `Select segment ${index + 1}` : undefined}
                tabIndex={hasSelectHandler ? 0 : undefined}
                onClick={hasSelectHandler ? () => handleSelect(index) : undefined}
                onKeyDown={
                  hasSelectHandler
                    ? (e) => {
                        if (e.key === "Enter" || e.key === " ") {
                          e.preventDefault();
                          handleSelect(index);
                        }
                      }
                    : undefined
                }
                className={cn(
                  "transition-all duration-300 ease-in-out",
                  hasSelectHandler && "cursor-pointer hover:opacity-80",
                  isSelected && "ring-2 ring-foreground ring-offset-1",
                )}
                style={{
                  width: `${percentage}%`,
                  height: "100%",
                  backgroundColor: color,
                  minWidth: segment.value > 0 ? "2px" : 0,
                  transform: isSelected ? "scaleY(1.2)" : undefined,
                }}
              />
            );

            if (segment.label || segment.value > 0) {
              return (
                <Tooltip key={index}>
                  <TooltipTrigger asChild>{segmentEl}</TooltipTrigger>
                  <TooltipContent>
                    <span>{segment.label || segment.value}</span>
                  </TooltipContent>
                </Tooltip>
              );
            }

            return segmentEl;
          })}
        </div>
        {showLabels && (
          <div className="flex gap-3 flex-wrap">
            {labelSegments.map(({ segment, originalIndex }) => {
              const color = segment.color
                ? `var(--${segment.color.toLowerCase()})`
                : "var(--primary)";
              const isSelected = selected === originalIndex;
              return (
                <div
                  key={originalIndex}
                  role={hasSelectHandler ? "button" : undefined}
                  aria-label={hasSelectHandler ? `Select segment ${originalIndex + 1}` : undefined}
                  tabIndex={hasSelectHandler ? 0 : undefined}
                  onClick={hasSelectHandler ? () => handleSelect(originalIndex) : undefined}
                  onKeyDown={
                    hasSelectHandler
                      ? (e) => {
                          if (e.key === "Enter" || e.key === " ") {
                            e.preventDefault();
                            handleSelect(originalIndex);
                          }
                        }
                      : undefined
                  }
                  className={cn(
                    "flex items-center gap-1.5 text-xs text-muted-foreground",
                    hasSelectHandler && "cursor-pointer hover:text-foreground",
                    isSelected && "text-foreground font-semibold",
                  )}
                >
                  <div
                    className={cn("rounded-full", isSelected && "ring-2 ring-foreground")}
                    style={{
                      width: "8px",
                      height: "8px",
                      backgroundColor: color,
                    }}
                  />
                  <span>{segment.label || segment.value}</span>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </TooltipProvider>
  );
};
