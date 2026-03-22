import React from "react";
import { useEventHandler } from "@/components/event-handler";
import Icon from "@/components/Icon";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";

interface ListItemWidgetProps {
  id: string;
  title?: string;
  subtitle?: string;
  icon?: string;
  badge?: string;
  disabled?: boolean;
  children?: React.ReactNode;
}

export const ListItemWidget: React.FC<ListItemWidgetProps> = ({
  id,
  title,
  subtitle,
  icon,
  badge,
  disabled,
  children,
}) => {
  const eventHandler = useEventHandler();

  return (
    <button
      onClick={() => eventHandler("OnClick", id, [])}
      disabled={disabled}
      className={cn(
        "pl-4 pr-4 py-2 w-full h-full min-h-[60px] flex-left flex items-center rounded-none text-left min-w-0 transition-colors",
        children ? "gap-3" : "",
        disabled
          ? "opacity-50 cursor-not-allowed"
          : "hover:bg-accent focus:bg-accent focus:outline-none cursor-pointer",
      )}
    >
      <div className="flex flex-col items-start text-body w-full flex-1 min-w-0 text-left">
        <span className="block w-full truncate text-left">{title}</span>
        {subtitle && (
          <span className="block text-sm text-muted-foreground w-full truncate text-left">
            {subtitle}
          </span>
        )}
        {children && <div className="w-full py-1">{children}</div>}
      </div>
      {icon && icon != "None" && (
        <Icon className="h-6 w-6 text-muted-foreground ml-auto flex-none" name={icon} />
      )}
      {badge && (
        <Badge variant="primary" className="ml-auto flex-none">
          {badge}
        </Badge>
      )}
    </button>
  );
};
