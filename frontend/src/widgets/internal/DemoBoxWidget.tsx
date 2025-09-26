import React from 'react';
import { cn } from '@/lib/utils';
import {
  getBorderRadius,
  getBorderStyle,
  getBorderThickness,
  getMargin,
  getPadding,
  getWidth,
  getHeight,
  getAlign,
  type BorderRadius,
  type BorderStyle,
  type Align,
} from '@/lib/styles';

interface DemoBoxWidgetProps {
  borderThickness?: string;
  borderRadius?: BorderRadius;
  borderStyle?: BorderStyle;
  padding?: string;
  margin?: string;
  width?: string;
  height?: string;
  contentAlign?: Align;
  visible?: boolean;
  children?: React.ReactNode;
}

const DemoBoxWidget: React.FC<DemoBoxWidgetProps> = ({
  borderThickness = '1',
  borderRadius = 'Rounded',
  borderStyle = 'Solid',
  padding = '4',
  margin = '0',
  width,
  height,
  contentAlign = 'TopLeft',
  visible = true,
  children,
}) => {
  if (!visible) {
    return null;
  }

  const styles: React.CSSProperties = {
    ...getBorderThickness(borderThickness),
    ...getBorderRadius(borderRadius),
    ...getBorderStyle(borderStyle),
    ...getPadding(padding),
    ...getMargin(margin),
    ...getWidth(width),
    ...getHeight(height),
    ...getAlign('Vertical', contentAlign),
    borderColor: 'var(--border)',
  };

  return (
    <div className={cn('border')} style={styles}>
      {children}
    </div>
  );
};

DemoBoxWidget.displayName = 'DemoBoxWidget';

export default DemoBoxWidget;
