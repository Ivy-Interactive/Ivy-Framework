import React, { useCallback } from "react";
import { useEventHandler } from "@/components/event-handler";
import Icon from "@/components/Icon";
import { cn } from "@/lib/utils";

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
}

export const BreadcrumbsWidget: React.FC<BreadcrumbsWidgetProps> = ({
  id,
  items = EMPTY_ARRAY,
  separator = "/",
  disabled = false,
  events = EMPTY_ARRAY,
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

  return (
    <nav aria-label="Breadcrumb">
      <ol className="flex items-center gap-1.5 text-sm">
        {items.map((item, index) => {
          const isLast = index === items.length - 1;
          const isClickable = item.hasOnClick && !item.disabled && !disabled && !isLast;

          return (
            <React.Fragment key={item.label}>
              <li className="flex items-center gap-1.5">
                {isClickable ? (
                  <button
                    type="button"
                    onClick={() => handleItemClick(index)}
                    className="text-muted-foreground hover:text-foreground transition-colors"
                    title={item.tooltip}
                  >
                    <span className="flex items-center gap-1">
                      {item.icon && item.icon !== "None" && <Icon name={item.icon} size={14} />}
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
                    {item.icon && item.icon !== "None" && <Icon name={item.icon} size={14} />}
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
