import React from "react";
import { useEventHandler } from "@/components/event-handler";
import Icon from "@/components/Icon";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import { Densities } from "@/types/density";

interface ListItemWidgetProps {
  id: string;
  title?: string;
  subtitle?: string;
  icon?: string;
  badge?: string;
  disabled?: boolean;
  children?: React.ReactNode;
  density?: Densities;
  events?: string[];
}

const paddingMap: Record<Densities, string> = {
  [Densities.Small]: "pl-3 pr-3 py-1",
  [Densities.Medium]: "pl-4 pr-4 py-2",
  [Densities.Large]: "pl-5 pr-5 py-3",
};

const minHeightMap: Record<Densities, string> = {
  [Densities.Small]: "min-h-[44px]",
  [Densities.Medium]: "min-h-[60px]",
  [Densities.Large]: "min-h-[76px]",
};

const gapMap: Record<Densities, string> = {
  [Densities.Small]: "gap-2",
  [Densities.Medium]: "gap-3",
  [Densities.Large]: "gap-4",
};

const iconSizeMap: Record<Densities, string> = {
  [Densities.Small]: "h-5 w-5",
  [Densities.Medium]: "h-6 w-6",
  [Densities.Large]: "h-7 w-7",
};

const subtitleSizeMap: Record<Densities, string> = {
  [Densities.Small]: "text-xs",
  [Densities.Medium]: "text-sm",
  [Densities.Large]: "text-base",
};

export const ListItemWidget: React.FC<ListItemWidgetProps> = ({
  id,
  title,
  subtitle,
  icon,
  badge,
  disabled,
  children,
  density = Densities.Medium,
  events = [],
}) => {
  const eventHandler = useEventHandler();

  return (
    <button
      onClick={() => {
        if (events.includes("OnClick")) eventHandler("OnClick", id, []);
      }}
      disabled={disabled}
      className={cn(
        paddingMap[density],
        minHeightMap[density],
        "w-full h-full flex-left flex items-center rounded-none text-left min-w-0 transition-colors",
        children ? gapMap[density] : "",
        disabled
          ? "opacity-50 cursor-not-allowed"
          : "hover:bg-accent focus:bg-accent focus:outline-none cursor-pointer",
      )}
    >
      <div className="flex flex-col items-start text-body w-full flex-1 min-w-0 text-left">
        <span className="block w-full truncate text-left">{title}</span>
        {subtitle && (
          <span
            className={cn(
              "block text-muted-foreground w-full truncate text-left",
              subtitleSizeMap[density],
            )}
          >
            {subtitle}
          </span>
        )}
        {children && <div className="w-full py-1">{children}</div>}
      </div>
      {icon && icon != "None" && (
        <Icon
          className={cn(iconSizeMap[density], "text-muted-foreground ml-auto flex-none")}
          name={icon}
        />
      )}
      {badge && (
        <Badge variant="primary" className="ml-auto flex-none">
          {badge}
        </Badge>
      )}
    </button>
  );
};
