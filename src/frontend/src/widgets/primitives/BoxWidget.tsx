import {
  Align,
  BorderRadius,
  BorderStyle,
  getAlign,
  getAspectRatio,
  getBoxRadius,
  getBorderStyle,
  getBorderThickness,
  getColor,
  getHeight,
  getMargin,
  getPadding,
  getWidth,
} from "@/lib/styles";
import { cn } from "@/lib/utils";
import React, { useCallback } from "react";
import { useEventHandler } from "@/components/event-handler";

const EMPTY_ARRAY: never[] = [];

export type HoverEffect = "None" | "Pointer" | "PointerAndTranslate" | "Shadow";

interface BoxWidgetProps {
  id: string;
  children?: React.ReactNode;
  background?: string | undefined;
  borderRadius: BorderRadius;
  borderThickness: string;
  borderStyle: BorderStyle;
  borderColor?: string;
  padding?: string;
  margin?: string;
  width?: string;
  height?: string;
  contentAlign: Align;
  opacity?: number;
  borderOpacity?: number;
  className?: string;
  events?: string[];
  aspectRatio?: number;
  hoverVariant?: HoverEffect;
}

export const BoxWidget: React.FC<BoxWidgetProps> = ({
  id,
  children,
  width,
  height,
  borderStyle = "Solid",
  borderRadius = "Rounded",
  borderThickness = "1",
  background,
  borderColor,
  padding = "2",
  margin = "0",
  contentAlign = "TopLeft",
  opacity,
  borderOpacity,
  className,
  aspectRatio,
  events = EMPTY_ARRAY,
  hoverVariant = "None",
}) => {
  const eventHandler = useEventHandler();
  const isClickable = events.includes("OnClick");

  // Use semantic box radius for 'Rounded', explicit values for 'None'/'Full'
  const borderRadiusStyle: React.CSSProperties =
    borderRadius === "Rounded"
      ? getBoxRadius()
      : borderRadius === "Full"
        ? { borderRadius: "9999px" }
        : { borderRadius: "0" };

  const styles: React.CSSProperties = {
    // Layout and spacing should always apply
    ...getPadding(padding),
    ...getMargin(margin),
    ...getAlign("Vertical", contentAlign),
    ...getWidth(width),
    ...getHeight(height),
    ...getAspectRatio(aspectRatio),
    ...getBorderStyle(borderStyle),
    ...getBorderThickness(borderThickness),
    ...borderRadiusStyle,
    ...getColor(background, "backgroundColor", "background", opacity),
    ...getColor(background, "color", "foreground"),
    ...getColor(borderColor, "borderColor", "background", borderOpacity),
  };

  const handleClick = useCallback(
    (e: React.MouseEvent) => {
      // Prevent event from bubbling up if not strictly necessary,
      // but only fire if interactive.
      if (isClickable) {
        e.stopPropagation();
        eventHandler("OnClick", id, []);
      }
    },
    [id, isClickable, eventHandler],
  );

  const hoverClass =
    hoverVariant === "None"
      ? null
      : hoverVariant === "Pointer"
        ? "cursor-pointer"
        : hoverVariant === "Shadow"
          ? "cursor-pointer hover:shadow-lg active:shadow-md transition-shadow"
          : "cursor-pointer transform hover:-translate-x-[4px] hover:-translate-y-[4px] active:translate-x-[-2px] active:translate-y-[-2px] transition";
  if (isClickable) {
    return (
      <div
        style={styles}
        className={cn(className, hoverClass)}
        onClick={handleClick}
        onKeyDown={(e) => {
          if (e.key === "Enter" || e.key === " ") {
            e.preventDefault();
            handleClick(e as unknown as React.MouseEvent);
          }
        }}
        role="button"
        aria-label="Interactive region"
        tabIndex={0}
      >
        {children}
      </div>
    );
  }

  return (
    <div style={styles} className={cn(className, hoverClass)} role="presentation">
      {children}
    </div>
  );
};
