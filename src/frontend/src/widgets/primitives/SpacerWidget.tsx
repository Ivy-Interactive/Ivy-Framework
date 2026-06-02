import React from "react";
import { getWidth, getHeight } from "@/lib/styles";
interface SpacerWidgetProps {
  width?: string;
  height?: string;
}

export const SpacerWidget: React.FC<SpacerWidgetProps> = ({ width, height }) => {
  const widthStyle = getWidth(width);
  const heightStyle = getHeight(height);

  if (heightStyle.height) {
    heightStyle.maxHeight = heightStyle.height;
  }

  const defaultStyle: React.CSSProperties = {};

  if (!width) {
    defaultStyle.flexGrow = 1;
    defaultStyle.minWidth = 0;
  }

  if (!height) {
    defaultStyle.flexGrow = 1;
  }

  return (
    <div
      style={{
        ...defaultStyle,
        ...widthStyle,
        ...heightStyle,
      }}
    />
  );
};
