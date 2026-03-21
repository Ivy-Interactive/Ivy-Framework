import Icon from "@/components/Icon";
import { useEventHandler } from "@/components/event-handler";
import { getHeight, getWidth } from "@/lib/styles";
import { cn } from "@/lib/utils";
import { X } from "lucide-react";
import React from "react";

const EMPTY_ARRAY: never[] = [];

interface CalloutWidgetProps {
  id: string;
  title?: string;
  children?: React.ReactNode;
  variant?: "Info" | "Success" | "Warning" | "Error";
  width?: string;
  height?: string;
  icon?: string;
  events?: string[];
}

const calloutVariant = {
  Info: {
    container: "border-cyan/20 bg-cyan/5 text-foreground dark:border-cyan/30 dark:bg-cyan/10",
    icon: "",
  },
  Success: {
    container:
      "border-emerald/20 bg-emerald/5 text-foreground dark:border-emerald/30 dark:bg-emerald/10",
    icon: "text-emerald dark:text-emerald-light",
  },
  Warning: {
    container: "border-amber/20 bg-amber/5 text-foreground dark:border-amber/30 dark:bg-amber/10",
    icon: "text-amber dark:text-amber-light",
  },
  Error: {
    container:
      "border-destructive/20 bg-destructive/5 text-foreground dark:border-destructive/30 dark:bg-destructive/10",
    icon: "text-destructive dark:text-destructive-light",
  },
};

const defaultIcons = {
  Info: "Info",
  Success: "CircleCheck",
  Warning: "CircleAlert",
  Error: "CircleAlert",
};

export const CalloutWidget: React.FC<CalloutWidgetProps> = ({
  id,
  title,
  children,
  variant = "Info",
  icon,
  width,
  height,
  events = EMPTY_ARRAY,
}) => {
  const eventHandler = useEventHandler();
  const showCloseButton = events.includes("OnClose");

  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  if (!icon) {
    icon = defaultIcons[variant || "Info"];
  }

  const variantKey = variant || "Info";
  const variantStyles = calloutVariant[variantKey];

  return (
    <div
      style={styles}
      className={cn(
        "flex items-center px-4 text-large-body rounded-box border transition-colors relative",
        variantStyles.container,
      )}
      role="alert"
    >
      {icon && <Icon size="30" name={icon} className={cn("mr-4 shrink-0", variantStyles.icon)} />}
      <span className="sr-only">{variant}</span>
      <div className="flex flex-col min-w-0 flex-1 py-4">
        {title && <div className="font-medium leading-none mb-1">{title}</div>}
        {children && <div className="text-sm opacity-90 [&_p]:text-sm [&_p]:mb-0">{children}</div>}
      </div>
      {showCloseButton && (
        <button
          type="button"
          onClick={() => eventHandler("OnClose", id, [])}
          className="absolute top-3 right-3 p-1 rounded-md opacity-70 hover:opacity-100"
          aria-label="Dismiss"
        >
          <X className="h-4 w-4" />
        </button>
      )}
    </div>
  );
};
