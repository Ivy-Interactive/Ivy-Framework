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
import { ImageOverlayProvider } from "@/components/markdown/ImageOverlayContext";
import { type Responsive, resolveResponsive } from "@/hooks/use-responsive";
import { useCurrentBreakpoint } from "@/hooks/use-breakpoint-context";

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
  alignContent?: Align;
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
  responsiveOrientation?: Responsive<Orientation>;
  responsiveRowGap?: Responsive<number>;
  responsiveColumnGap?: Responsive<number>;
  responsivePadding?: Responsive<string>;
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
  alignContent,
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
  responsiveOrientation,
  responsiveRowGap,
  responsiveColumnGap,
  responsivePadding,
}) => {
  const bp = useCurrentBreakpoint();

  // Resolve layout-specific responsive values with mobile-first cascading
  const resolvedOrientation = responsiveOrientation
    ? resolveResponsive(responsiveOrientation, bp, orientation)
    : orientation;
  const resolvedRowGap = responsiveRowGap
    ? resolveResponsive(responsiveRowGap, bp, rowGap)
    : rowGap;
  const resolvedColumnGap = responsiveColumnGap
    ? resolveResponsive(responsiveColumnGap, bp, columnGap)
    : columnGap;
  const resolvedPadding = responsivePadding
    ? resolveResponsive(responsivePadding, bp, padding as string)
    : padding;

  const baseStyles: React.CSSProperties = {
    ...getPadding(resolvedPadding),
    ...getMargin(margin),
    ...getRowGap(resolvedRowGap),
    ...getColumnGap(resolvedColumnGap),
    ...getAlign(resolvedOrientation, alignContent),
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
      <ImageOverlayProvider>
        <ScrollArea
          className={removeParentPadding ? "remove-parent-padding" : ""}
          style={outerStyles}
          type="scroll"
          scrollHideDelay={600}
        >
          <div style={flexStyles}>{wrappedChildren}</div>
        </ScrollArea>
      </ImageOverlayProvider>
    );
  }

  return (
    <ImageOverlayProvider>
      <div style={baseStyles} className={removeParentPadding ? "remove-parent-padding" : ""}>
        {wrappedChildren}
      </div>
    </ImageOverlayProvider>
  );
};
