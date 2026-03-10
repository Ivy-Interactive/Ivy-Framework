import React from 'react';
import {
  Align,
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
} from '@/lib/styles';
import { ScrollArea } from '@/components/ui/scroll-area';

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
  align?: Align;
  scroll?: 'None' | 'Auto' | 'Vertical' | 'Horizontal' | 'Both';
  removeParentPadding?: boolean;
  visible?: boolean;
  wrap?: boolean;
  childAlignSelf?: (Align | undefined)[];
}

export const StackLayoutWidget: React.FC<StackLayoutWidgetProps> = ({
  orientation = 'Vertical',
  children,
  rowGap = 4,
  columnGap = 4,
  padding,
  margin,
  width,
  height,
  background,
  align,
  scroll,
  removeParentPadding = false,
  visible = true,
  wrap = false,
  childAlignSelf = EMPTY_ARRAY,
}) => {
  const baseStyles: React.CSSProperties = {
    ...getPadding(padding),
    ...getMargin(margin),
    ...getRowGap(rowGap),
    ...getColumnGap(columnGap),
    ...getAlign(orientation, align),
    ...getWidth(width),
    ...getHeight(height),
    ...getColor(background, 'backgroundColor', 'background'),
  };

  // Override flexWrap if wrap is enabled
  if (wrap) {
    baseStyles.flexWrap = 'wrap';
  }

  if (!visible) {
    return null;
  }

  // Handle scroll modes
  const getScrollStyles = (): React.CSSProperties => {
    switch (scroll) {
      case 'Auto':
      case 'Vertical':
        return { overflowY: 'auto', overflowX: 'hidden' };
      case 'Horizontal':
        return { overflowX: 'auto', overflowY: 'hidden' };
      case 'Both':
        return { overflow: 'auto' };
      default:
        return {};
    }
  };

  const styles = { ...baseStyles, ...getScrollStyles() };

  // Wrap children with alignSelf styles if needed
  const wrappedChildren = React.Children.map(children, (child, index) => {
    const alignSelf = childAlignSelf[index];
    if (alignSelf && React.isValidElement(child)) {
      const alignSelfStyles = getAlignSelf(alignSelf);
      return <div style={alignSelfStyles}>{child}</div>;
    }
    return child;
  });

  if (scroll === 'Auto' || scroll === 'Vertical') {
    return (
      <div style={styles}>
        <ScrollArea className="h-full w-full">
          <div className="p-4">{wrappedChildren}</div>
        </ScrollArea>
      </div>
    );
  }

  return (
    <div
      style={styles}
      className={removeParentPadding ? 'remove-parent-padding' : ''}
    >
      {wrappedChildren}
    </div>
  );
};
