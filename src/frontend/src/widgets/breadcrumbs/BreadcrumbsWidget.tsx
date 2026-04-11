import React, { useCallback } from "react";
import { useEventHandler } from "@/components/event-handler";
import Icon from "@/components/Icon";
import { cn } from "@/lib/utils";
import { Densities } from "@/types/density";

const EMPTY_ARRAY: never[] = [];

interface BreadcrumbItemProps {
  label: string;
  hasOnClick?: boolean;
  icon?: string;
  tooltip?: string;
  disabled?: boolean;
}

interface BreadcrumbsWidgetProps {
  id: string;
  items?: BreadcrumbItemProps[];
  separator?: string;
  disabled?: boolean;
  events?: string[];
  density?: Densities;
}

export const BreadcrumbsWidget: React.FC<BreadcrumbsWidgetProps> = ({
  id,
  items = EMPTY_ARRAY,
  separator = "/",
  disabled = false,
  events = EMPTY_ARRAY,
  density = Densities.Medium,
}) => {
  const eventHandler = useEventHandler();
  const hasItemClickHandler = events.includes("OnItemClick");

  const handleItemClick = useCallback(
    (index: number) => {
      if (hasItemClickHandler && !disabled) {
        eventHandler("OnItemClick", id, [index]);
      }
    },
    [id, disabled, hasItemClickHandler, eventHandler],
  );

  const olGapClass =
    density === Densities.Small ? "gap-1" : density === Densities.Large ? "gap-2" : "gap-1.5";
  const textSizeClass =
    density === Densities.Small ? "text-xs" : density === Densities.Large ? "text-base" : "text-sm";
  const liGapClass =
    density === Densities.Small ? "gap-1" : density === Densities.Large ? "gap-2" : "gap-1.5";
  const iconSize = density === Densities.Small ? 12 : density === Densities.Large ? 16 : 14;

  return (
    <nav aria-label="Breadcrumb">
      <ol className={cn("flex items-center", olGapClass, textSizeClass)}>
        {items.map((item, index) => {
          const isLast = index === items.length - 1;
          const isClickable = item.hasOnClick && !item.disabled && !disabled && !isLast;

          return (
            <React.Fragment key={item.label}>
              <li className={cn("flex items-center", liGapClass)}>
                {isClickable ? (
                  <button
                    type="button"
                    onClick={() => handleItemClick(index)}
                    className="text-muted-foreground hover:text-foreground transition-colors"
                    title={item.tooltip}
                  >
                    <span className="flex items-center gap-1">
                      {item.icon && item.icon !== "None" && (
                        <Icon name={item.icon} size={iconSize} />
                      )}
                      {item.label}
                    </span>
                  </button>
                ) : (
                  <span
                    className={cn(
                      "flex items-center gap-1",
                      isLast ? "text-foreground font-medium" : "text-muted-foreground",
                      (item.disabled || disabled) && "opacity-50",
                    )}
                    title={item.tooltip}
                  >
                    {item.icon && item.icon !== "None" && <Icon name={item.icon} size={iconSize} />}
                    {item.label}
                  </span>
                )}
              </li>
              {!isLast && (
                <li role="presentation" className="text-muted-foreground/50 select-none">
                  {separator}
                </li>
              )}
            </React.Fragment>
          );
        })}
      </ol>
    </nav>
  );
};
