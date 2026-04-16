import Icon from "@/components/Icon";
import { useEventHandler } from "@/components/event-handler";
import { getWidth } from "@/lib/styles";
import { cn } from "@/lib/utils";
import { Densities } from "@/types/density";
import React from "react";

const EMPTY_ARRAY: never[] = [];

export interface StepperItem {
  symbol: string;
  icon?: string;
  label?: string;
  description?: string;
  loading?: boolean;
}

interface StepperWidgetProps {
  id: string;
  selectedIndex?: number;
  items: StepperItem[];
  width?: string;
  allowSelectForward?: boolean;
  events?: string[];
  density?: Densities;
}

export const StepperWidget: React.FC<StepperWidgetProps> = ({
  id,
  selectedIndex = 0,
  items = EMPTY_ARRAY,
  width,
  allowSelectForward = false,
  events = EMPTY_ARRAY,
  density = Densities.Medium,
}) => {
  const eventHandler = useEventHandler();
  const hasSelectHandler = events.includes("OnSelect");

  const circleClass =
    density === Densities.Small
      ? "w-6 h-6 text-xs"
      : density === Densities.Large
        ? "w-10 h-10 text-base"
        : "w-8 h-8 text-sm";
  const iconSize = density === Densities.Small ? 14 : density === Densities.Large ? 18 : 16;
  const connectorClass =
    density === Densities.Small ? "mx-1" : density === Densities.Large ? "mx-3" : "mx-2";
  const labelWidthClass =
    density === Densities.Small ? "w-6" : density === Densities.Large ? "w-10" : "w-8";

  const handleSelect = (index: number) => {
    if (!hasSelectHandler) return;
    if (index === selectedIndex) return; // Current step not clickable
    if (index > selectedIndex && !allowSelectForward) return; // Upcoming only if allowed
    if (events.includes("OnSelect")) eventHandler("OnSelect", id, [index]);
  };

  const styles: React.CSSProperties = {
    ...getWidth(width),
  };

  const getStepState = (index: number) => {
    if (selectedIndex === null || selectedIndex === undefined) {
      return "upcoming";
    }
    if (index < selectedIndex) return "completed";
    if (index === selectedIndex) return "current";
    return "upcoming";
  };

  return (
    <div key={id} style={styles} className="flex flex-col w-full">
      {/* Row 1: Circles and lines */}
      <div className="flex items-center w-full">
        {items.map((item, index) => {
          const state = getStepState(index);
          const isLast = index === items.length - 1;
          const isLineCompleted = index < selectedIndex;
          const isClickable =
            hasSelectHandler &&
            (state === "completed" || (state === "upcoming" && allowSelectForward));

          return (
            <React.Fragment key={item.label || item.symbol}>
              <button
                type="button"
                aria-label={item.label || `Step ${index + 1}`}
                onClick={() => handleSelect(index)}
                disabled={!isClickable}
                className={cn(
                  "relative z-10 flex-shrink-0 flex items-center justify-center rounded-full border-2 font-medium transition-all bg-background",
                  circleClass,
                  state === "completed" &&
                    isClickable &&
                    "border-primary bg-primary text-primary-foreground cursor-pointer hover:scale-110 hover:shadow-md",
                  state === "completed" &&
                    !isClickable &&
                    "border-primary bg-primary text-primary-foreground",
                  state === "current" && "border-primary bg-primary text-primary-foreground",
                  state === "upcoming" &&
                    isClickable &&
                    "border-muted-foreground/30 text-muted-foreground/50 cursor-pointer hover:border-primary/50 hover:text-muted-foreground hover:scale-105",
                  state === "upcoming" &&
                    !isClickable &&
                    "border-muted-foreground/30 text-muted-foreground/50",
                )}
              >
                {item.icon ? <Icon name={item.icon} size={iconSize} /> : item.symbol || index + 1}
              </button>

              {/* Connector line between steps */}
              {!isLast && (
                <div
                  className={cn(
                    "flex-1 h-0.5",
                    connectorClass,
                    isLineCompleted ? "bg-primary" : "bg-muted-foreground/30",
                  )}
                />
              )}
            </React.Fragment>
          );
        })}
      </div>

      {/* Row 2: Labels and descriptions - mirrors the structure of row 1 */}
      <div className="flex items-start w-full mt-2">
        {items.map((item, index) => {
          const state = getStepState(index);
          const isFirst = index === 0;
          const isLast = index === items.length - 1;

          return (
            <React.Fragment key={item.label || item.symbol}>
              {/* Label container - same width as circle */}
              <div
                className={cn(
                  "flex flex-col flex-shrink-0",
                  labelWidthClass,
                  isFirst && "items-start",
                  isLast && "items-end",
                  !isFirst && !isLast && "items-center",
                )}
              >
                <div
                  className={cn(
                    "flex flex-col whitespace-nowrap",
                    isFirst && "items-start text-left",
                    isLast && "items-end text-right",
                    !isFirst && !isLast && "items-center text-center",
                  )}
                >
                  {item.label && (
                    <span
                      className={cn(
                        "text-sm font-medium",
                        state === "upcoming" && "text-muted-foreground/50",
                      )}
                    >
                      {item.label}
                    </span>
                  )}
                  {item.description && (
                    <span
                      className={cn(
                        "text-sm",
                        state === "upcoming" ? "text-muted-foreground/40" : "text-muted-foreground",
                      )}
                    >
                      {item.description}
                    </span>
                  )}
                </div>
              </div>

              {/* Spacer to match the line width */}
              {!isLast && <div className={cn("flex-1", connectorClass)} />}
            </React.Fragment>
          );
        })}
      </div>
    </div>
  );
};

export default StepperWidget;
