import React from "react";
import { getColor, getHeight, getPadding, getWidth } from "@/lib/styles";

const EMPTY_ARRAY: never[] = [];

interface CanvasLayoutWidgetProps {
  children: React.ReactNode[];
  padding?: string;
  width?: string;
  height?: string;
  background?: string;
  childLeft?: (string | undefined)[];
  childTop?: (string | undefined)[];
}

function sizeToCss(size?: string): string | undefined {
  if (!size) return undefined;
  const [sizeType, value] = size.split(":");
  switch (sizeType.toLowerCase()) {
    case "units":
      return `${parseFloat(value) * 0.25}rem`;
    case "px":
      return `${value}px`;
    case "rem":
      return `${value}rem`;
    case "fraction":
      return `${parseFloat(value) * 100}%`;
    case "full":
      return "100%";
    default:
      return undefined;
  }
}

export const CanvasLayoutWidget: React.FC<CanvasLayoutWidgetProps> = ({
  children,
  padding,
  width,
  height,
  background,
  childLeft = EMPTY_ARRAY,
  childTop = EMPTY_ARRAY,
}) => {
  const containerStyles: React.CSSProperties = {
    position: "relative",
    ...getPadding(padding),
    ...getWidth(width),
    ...getHeight(height),
    ...getColor(background, "backgroundColor", "background"),
  };

  return (
    <div style={containerStyles}>
      {React.Children.map(children, (child, index) => {
        const left = sizeToCss(childLeft[index]);
        const top = sizeToCss(childTop[index]);

        const positionStyles: React.CSSProperties = {
          position: "absolute",
          left,
          top,
        };

        return <div style={positionStyles}>{child}</div>;
      })}
    </div>
  );
};

export default CanvasLayoutWidget;
