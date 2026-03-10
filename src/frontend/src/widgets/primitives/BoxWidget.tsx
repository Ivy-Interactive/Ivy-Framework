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
import React, { useCallback } from 'react';
import { useEventHandler } from '@/components/event-handler';

export type BoxHoverVariant = 'None' | 'Pointer' | 'PointerAndTranslate';

interface BoxWidgetProps {
  id: string;
  children?: React.ReactNode;
  color?: string | undefined;
  borderRadius: BorderRadius;
  borderThickness: string;
  borderStyle: BorderStyle;
  borderColor?: string;
  padding?: string;
  margin?: string;
  width?: string;
  height?: string;
  contentAlign: Align;
  opacity?: number;
  borderOpacity?: number;
  className?: string;
  events?: string[];
  hoverVariant?: BoxHoverVariant;
}

export const BoxWidget: React.FC<BoxWidgetProps> = ({
  id,
  children,
  width,
  height,
  borderStyle = 'Solid',
  borderRadius = 'Rounded',
  borderThickness = '1',
  color,
  borderColor,
  padding = '2',
  margin = '0',
  contentAlign = 'TopLeft',
  opacity,
  borderOpacity,
  className,
  events = [],
  hoverVariant = 'None',
}) => {
  const eventHandler = useEventHandler();
  const isClickable = events.includes('OnClick');

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
    ...getColor(color, 'color', 'foreground'),
    ...getColor(borderColor, 'borderColor', 'background', borderOpacity),
  };

  const handleClick = useCallback(
    (e: React.MouseEvent) => {
      // Prevent event from bubbling up if not strictly necessary,
      // but only fire if interactive.
      if (isClickable) {
        e.stopPropagation();
        eventHandler('OnClick', id, []);
      }
    },
    [id, isClickable, eventHandler]
  );

  const hoverClass =
    hoverVariant === 'None'
      ? null
      : hoverVariant === 'Pointer'
        ? 'cursor-pointer'
        : 'cursor-pointer transform hover:-translate-x-[4px] hover:-translate-y-[4px] active:translate-x-[-2px] active:translate-y-[-2px] transition';
  return (
    <>
      <div
        style={styles}
        className={cn(className, hoverClass)}
        onClick={isClickable ? handleClick : undefined}
        role={isClickable ? 'button' : undefined}
        tabIndex={isClickable ? 0 : undefined}
      >
        {children}
      </div>
    </>
  );
};
