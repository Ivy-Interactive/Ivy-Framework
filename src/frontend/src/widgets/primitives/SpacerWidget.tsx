import React from "react";
import { getWidth, getHeight } from "@/lib/styles";
interface SpacerWidgetProps {
  width?: string;
  height?: string;
}

export const SpacerWidget: React.FC<SpacerWidgetProps> = ({ width, height }) => {
  return (
    <div
      style={{
        flexGrow: 1,
        flexShrink: 1,
        flexBasis: "0%",
        ...getWidth(width),
        ...getHeight(height),
      }}
    />
  );
};
