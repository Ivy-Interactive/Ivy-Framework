import { Densities } from "@/types/density";
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
  variant?: "Info" | "Success" | "Warning" | "Error" | "Destructive";
  density?: Densities;
  width?: string;
  height?: string;
  icon?: string;
  events?: string[];
}

const calloutVariant = {
  Info: {
    container: "border-cyan/20 bg-cyan/10 text-foreground dark:border-cyan/30 dark:bg-cyan/10",
    icon: "",
  },
  Success: {
    container:
      "border-emerald/20 bg-emerald/10 text-foreground dark:border-emerald/30 dark:bg-emerald/10",
    icon: "text-emerald dark:text-emerald-light",
  },
  Warning: {
    container: "border-amber/20 bg-amber/10 text-foreground dark:border-amber/30 dark:bg-amber/10",
    icon: "text-amber dark:text-amber-light",
  },
  Error: {
    container:
      "border-destructive/20 bg-destructive/10 text-foreground dark:border-destructive/30 dark:bg-destructive/10",
    icon: "text-destructive dark:text-destructive-light",
  },
  Destructive: {
    container:
      "border-destructive/20 bg-destructive/10 text-foreground dark:border-destructive/30 dark:bg-destructive/10",
    icon: "text-destructive dark:text-destructive-light",
  },
};

const defaultIcons = {
  Info: "Info",
  Success: "CircleCheck",
  Warning: "CircleAlert",
  Error: "CircleAlert",
  Destructive: "CircleAlert",
};

export const CalloutWidget: React.FC<CalloutWidgetProps> = ({
  id,
  title,
  children,
  variant = "Info",
  density = Densities.Medium,
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

  const isSmall = density === Densities.Small;
  const isLarge = density === Densities.Large;

  const iconSize = isSmall ? "20" : isLarge ? "28" : "24";
  const paddingClass = isSmall ? "py-2.5 px-3" : isLarge ? "py-6 px-6" : "py-4 px-4";

  // Title line heights match icon sizes for perfect titled alignment
  const titleLeadingClass = isSmall ? "leading-5" : isLarge ? "leading-7" : "leading-6";

  // text-sm leading-relaxed has a visual midline around 11.5px from the top.
  // Small icon center is 10px. We use mt-0.5 on the text to push it down visually to match the SVG circle's center.
  // Medium icon center is 12px. No offset needed.
  // Large icon center is 14px. We use mt-0.5 on the text.
  const iconAlignmentClass = "";
  const textAlignmentClass = !title ? (isLarge || isSmall ? "mt-0.5" : "") : "";

  return (
    <div
      style={styles}
      className={cn(
        "flex items-start text-large-body rounded-box border transition-colors relative",
        paddingClass,
        variantStyles.container,
      )}
      role="alert"
    >
      {icon && (
        <Icon
          size={iconSize}
          name={icon}
          className={cn("mr-3.5 shrink-0 opacity-90", iconAlignmentClass, variantStyles.icon)}
        />
      )}
      <span className="sr-only">{variant}</span>
      <div className={cn("flex flex-col min-w-0 flex-1", textAlignmentClass)}>
        {title && <div className={cn("font-medium mb-1", titleLeadingClass)}>{title}</div>}
        {children && (
          <div className="text-sm opacity-90 leading-relaxed [&_p]:text-sm [&_p]:mb-0">
            {children}
          </div>
        )}
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
