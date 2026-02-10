import {
  Align,
  BorderRadius,
  BorderStyle,
  getAlign,
  getBoxRadius,
  getBorderStyle,
  getBorderThickness,
  getColor,
  getHeight,
  getMargin,
  getPadding,
  getWidth,
} from '@/lib/styles';
import { cn } from '@/lib/utils';
import React from 'react';

interface BoxWidgetProps {
  id: string;
  children?: React.ReactNode;
  color?: string | undefined;
  borderRadius: BorderRadius;
  borderThickness: string;
  borderStyle: BorderStyle;
  padding?: string;
  margin?: string;
  width?: string;
  height?: string;
  contentAlign: Align;
  opacity?: number;
  className?: string;
}

export const BoxWidget: React.FC<BoxWidgetProps> = ({
  children,
  width,
  height,
  borderStyle = 'Solid',
  borderRadius = 'Rounded',
  borderThickness = '1',
  color,
  padding = '2',
  margin = '0',
  contentAlign = 'TopLeft',
  opacity,
  className,
}) => {
  // Use semantic box radius for 'Rounded', explicit values for 'None'/'Full'
  const borderRadiusStyle: React.CSSProperties =
    borderRadius === 'Rounded'
      ? getBoxRadius()
      : borderRadius === 'Full'
        ? { borderRadius: '9999px' }
        : { borderRadius: '0' };

  const styles: React.CSSProperties = {
    // Layout and spacing should always apply
    ...getPadding(padding),
    ...getMargin(margin),
    ...getAlign('Vertical', contentAlign),
    ...getWidth(width),
    ...getHeight(height),
    ...getBorderStyle(borderStyle),
    ...getBorderThickness(borderThickness),
    ...borderRadiusStyle,
    ...getColor(color, 'backgroundColor', 'background', opacity),
    ...getColor(color, 'borderColor', 'background'),
    ...getColor(color, 'color', 'foreground'),
  };

  return (
    <>
      <div style={styles} className={cn(className)}>
        {children}
      </div>
    </>
  );
};
