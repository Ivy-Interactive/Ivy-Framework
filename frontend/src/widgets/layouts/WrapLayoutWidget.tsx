import React from 'react';
import { cn } from '@/lib/utils';
import {
  getColor,
  getGap,
  getHeight,
  getPadding,
  getWidth,
  getMargin,
} from '@/lib/styles';

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
    ...getColor(background, 'backgroundColor', 'background'),
  };

  return (
    <div className={cn('flex flex-wrap')} style={styles}>
      {children}
    </div>
  );
};
