import React from "react";
import {
  Align,
  BorderRadius,
  BorderStyle,
  getAspectRatio,
  getRowGap,
  getColumnGap,
  getHeight,
  getAlign,
  getPadding,
  getWidth,
  Orientation,
  getColor,
  getMargin,
  getAlignSelf,
  getBorderStyle,
  getBorderThickness,
  getBoxRadius,
} from "@/lib/styles";
import { ScrollArea } from "@/components/ui/scroll-area";

const EMPTY_ARRAY: never[] = [];

interface StackLayoutWidgetProps {
  children: React.ReactNode;
  orientation: Orientation;
  rowGap?: number;
  columnGap?: number;
  padding?: string;
  margin?: string;
  width?: string;
  height?: string;
  background?: string;
  align?: Align;
  scroll?: "None" | "Auto" | "Vertical" | "Horizontal" | "Both";
  removeParentPadding?: boolean;
  visible?: boolean;
  wrap?: boolean;
  childAlignSelf?: (Align | undefined)[];
  borderColor?: string;
  borderRadius?: BorderRadius;
  borderStyle?: BorderStyle;
  borderThickness?: string;
  aspectRatio?: number;
}

export const StackLayoutWidget: React.FC<StackLayoutWidgetProps> = ({
  orientation = "Vertical",
  children,
  rowGap = 4,
  columnGap = 4,
  padding,
  margin,
  width,
  height,
  background,
  align,
  scroll,
  removeParentPadding = false,
  visible = true,
  wrap = false,
  childAlignSelf = EMPTY_ARRAY,
  borderColor,
  borderRadius = "None",
  borderStyle = "None",
  borderThickness,
  aspectRatio,
}) => {
  const baseStyles: React.CSSProperties = {
    ...getPadding(padding),
    ...getMargin(margin),
    ...getRowGap(rowGap),
    ...getColumnGap(columnGap),
    ...getAlign(orientation, align),
    ...getWidth(width),
    ...getHeight(height),
    ...getAspectRatio(aspectRatio),
    ...getColor(background, "backgroundColor", "background"),
    ...(borderStyle !== "None" ? getBorderStyle(borderStyle) : {}),
    ...(borderThickness ? getBorderThickness(borderThickness) : {}),
    ...(borderColor ? getColor(borderColor, "borderColor", "background") : {}),
    ...(borderRadius === "Rounded"
      ? getBoxRadius()
      : borderRadius === "Full"
        ? { borderRadius: "9999px" }
        : {}),
  };

  // Override flexWrap if wrap is enabled
  if (wrap) {
    baseStyles.flexWrap = "wrap";
  }

  if (!visible) {
    return null;
  }

  // Wrap children with alignSelf styles if needed
  const wrappedChildren = React.Children.map(children, (child, index) => {
    const alignSelf = childAlignSelf[index];
    if (alignSelf && React.isValidElement(child)) {
      const alignSelfStyles = getAlignSelf(alignSelf);
      return <div style={alignSelfStyles}>{child}</div>;
    }
    return child;
  });

  const hasScroll = scroll && scroll !== "None";

  if (hasScroll) {
    const flexStyles = { ...baseStyles };
    delete flexStyles.width;
    delete flexStyles.height;
    const outerStyles: React.CSSProperties = {
      ...getWidth(width),
      ...getHeight(height),
    };

    return (
      <ScrollArea
        className={removeParentPadding ? "remove-parent-padding" : ""}
        style={outerStyles}
        type="scroll"
        scrollHideDelay={600}
      >
        <div style={flexStyles}>{wrappedChildren}</div>
      </ScrollArea>
    );
  }

  return (
    <div style={baseStyles} className={removeParentPadding ? "remove-parent-padding" : ""}>
      {wrappedChildren}
    </div>
  );
};
