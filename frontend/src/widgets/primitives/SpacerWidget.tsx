import React from 'react';
import { getWidth, getHeight } from '@/lib/styles';
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
