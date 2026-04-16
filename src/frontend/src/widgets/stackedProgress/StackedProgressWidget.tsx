import React from "react";
import { useEventHandler } from "@/components/event-handler";
import { getWidth } from "@/lib/styles";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";
import { cn } from "@/lib/utils";
import { Densities } from "@/types/density";

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
  density?: Densities;
}

export const StackedProgressWidget: React.FC<StackedProgressWidgetProps> = ({
  id,
  segments = EMPTY_ARRAY,
  barHeight = 8,
  showLabels = false,
  rounded = true,
  selected,
  width = "Full",
  events = EMPTY_ARRAY,
  density = Densities.Medium,
}) => {
  const eventHandler = useEventHandler();
  const hasSelectHandler = events.includes("OnSelect");
  const total = segments.reduce((sum, s) => sum + s.value, 0);

  const labelGapClass =
    density === Densities.Small ? "gap-2" : density === Densities.Large ? "gap-4" : "gap-3";
  const dotSize =
    density === Densities.Small ? "6px" : density === Densities.Large ? "10px" : "8px";
  const labelTextClass =
    density === Densities.Small
      ? "text-[10px]"
      : density === Densities.Large
        ? "text-sm"
        : "text-xs";

  if (total === 0) {
    return (
      <div
        className="bg-neutral/10"
        style={{
          minWidth: 0,
          maxWidth: "100%",
          ...getWidth(width),
          height: `${barHeight}px`,
          borderRadius: rounded ? `${barHeight / 2}px` : undefined,
          overflow: "hidden",
        }}
      />
    );
  }

  const containerStyles: React.CSSProperties = {
    minWidth: 0,
    maxWidth: "100%",
    ...getWidth(width),
    height: `${barHeight}px`,
    display: "flex",
    overflow: "hidden",
    borderRadius: rounded ? `${barHeight / 2}px` : undefined,
  };

  const handleSelect = (index: number) => {
    if (hasSelectHandler) {
      if (events.includes("OnSelect")) eventHandler("OnSelect", id, [index]);
    }
  };

  // Map from filtered label index back to the original segment index
  const labelSegments = segments
    .map((segment, index) => ({ segment, originalIndex: index }))
    .filter(({ segment }) => segment.label);

  return (
    <TooltipProvider>
      <div
        className="flex flex-col gap-1"
        style={{
          minWidth: 0,
          maxWidth: "100%",
          ...getWidth(width),
          height: "fit-content",
          overflow: "hidden",
        }}
      >
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
                  flex: `${percentage} 1 0%`,
                  height: "100%",
                  backgroundColor: color,
                  minWidth: 0,
                  transform: isSelected ? "scaleY(1.2)" : undefined,
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
          <div className={cn("flex flex-wrap min-w-0", labelGapClass)}>
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
                    "flex items-center gap-1.5 text-muted-foreground",
                    labelTextClass,
                    hasSelectHandler && "cursor-pointer hover:text-foreground",
                    isSelected && "text-foreground font-semibold",
                  )}
                >
                  <div
                    className={cn("rounded-full", isSelected && "ring-2 ring-foreground")}
                    style={{
                      width: dotSize,
                      height: dotSize,
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
