import { getWidth, getHeight } from "@/lib/styles";
import React from "react";

interface SpacerWidgetProps {
  width?: string;
  height?: string;
}

export const SpacerWidget: React.FC<SpacerWidgetProps> = ({
  width,
  height,
}) => {
  return <div style={{ ...getWidth(width), ...getHeight(height) }} />;
};
