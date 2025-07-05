import Icon from "@/components/Icon";
import { getHeight, getWidth } from "@/lib/styles";
import { cn } from "@/lib/utils";
import React from "react";

interface CalloutWidgetProps {
  title?: string;
  children?: React.ReactNode;
  variant?: "Info" | "Success" | "Warning" | "Error";
  width?: string;
  height?: string;
  icon?: string;
}

const backgroundColors = {
  Info: "cyan",
  Success: "primary",
  Warning: "yellow",
  Error: "destructive",
};

const defaultIcons = {
  Info: "Info",
  Success: "CircleCheck",
  Warning: "CircleAlert",
  Error: "CircleAlert",
};

export const CalloutWidget: React.FC<CalloutWidgetProps> = ({
  title,
  children,
  variant,
  icon,
  width,
  height,
}) => {
  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
    backgroundColor: "var(--" + backgroundColors[variant || "Info"] + "-light)",
  };

  if (!icon) {
    icon = defaultIcons[variant || "Info"];
  }

  return (
    <div
      style={styles}
      className={cn("flex items-center p-4 text-sm")}
      role="alert"
    >
      {icon && <Icon size="30" name={icon} className="mr-4 opacity-50" />}
      <span className="sr-only">{variant}</span>
      <div className="flex flex-col">
        {title && <div className="font-medium">{title}</div>}
        {children && <div className="opacity-50">{children}</div>}
      </div>
    </div>
  );
};
