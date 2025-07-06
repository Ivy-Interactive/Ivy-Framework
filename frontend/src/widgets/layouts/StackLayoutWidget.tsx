import React from 'react';
import {
  Align,
  getGap,
  getHeight,
  getAlign,
  getPadding,
  getWidth,
  Orientation,
  getColor,
  getMargin,
} from '@/lib/styles';
import { ScrollArea } from '@/components/ui/scroll-area';

interface StackLayoutWidgetProps {
  children: React.ReactNode;
  orientation: Orientation;
  gap?: number;
  padding?: string;
  margin?: string;
  width?: string;
  height?: string;
  background?: string;
  align?: Align;
  scroll?: 'None' | 'Auto';
  removeParentPadding?: boolean;
  visible?: boolean;
}

export const StackLayoutWidget: React.FC<StackLayoutWidgetProps> = ({
  orientation,
  children,
  gap,
  padding,
  margin,
  width,
  height,
  background,
  align,
  scroll,
  removeParentPadding,
  visible,
}) => {
  const styles = {
    ...getPadding(padding),
    ...getMargin(margin),
    ...getGap(gap),
    ...getWidth(width),
    ...getHeight(height),
    ...getAlign(orientation, align),
    ...getColor(background, 'backgroundColor', 'background'),
  };

  if (!visible) {
    return null;
  }

  if (scroll === 'Auto') {
    return (
      <div style={styles}>
        <ScrollArea className="h-full w-full">
          <div className="p-4">{children}</div>
        </ScrollArea>
      </div>
    );
  }

  return (
    <div
      style={styles}
      className={removeParentPadding ? 'remove-parent-padding' : ''}
    >
      {children}
    </div>
  );
};
