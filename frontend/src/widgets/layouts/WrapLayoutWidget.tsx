import {
  getColor,
  getGap,
  getHeight,
  getPadding,
  getWidth,
  getMargin,
} from "@/lib/styles";
import { cn } from "@/lib/utils";
import React from "react";

interface WrapLayoutWidgetProps {
  gap?: number;
  padding?: string;
  margin?: string;
  width?: string;
  height?: string;
  background?: string;
  children: React.ReactNode;
}

export const WrapLayoutWidget: React.FC<WrapLayoutWidgetProps> = ({
  children,
  gap,
  padding,
  margin,
  width,
  height,
  background,
}) => {
  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
    ...getPadding(padding),
    ...getMargin(margin),
    ...getGap(gap),
    ...getColor(background, "backgroundColor", "background"),
  };

  return (
    <div className={cn("flex flex-wrap")} style={styles}>
      {children}
    </div>
  );
};
