import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
} from '@/components/ui/card';
import {
  getHeight,
  getWidth,
  getBorderRadius,
  getBorderStyle,
  getBorderThickness,
  getColor,
  BorderRadius,
  BorderStyle,
} from '@/lib/styles';
import { cn } from '@/lib/utils';
import { useEventHandler } from '@/components/event-handler';
import React, { useCallback } from 'react';
import { EmptyWidget } from '../primitives/EmptyWidget';
import { Scales } from '@/types/scale';
import { cardStyles, getSizeClasses } from './styles';

interface CardWidgetProps {
  id: string;
  events: string[];
  width?: string;
  height?: string;
  borderThickness?: string;
  borderRadius?: BorderRadius;
  borderStyle?: BorderStyle;
  borderColor?: string;
  hoverVariant?: 'None' | 'Pointer' | 'PointerAndTranslate';
  scale?: Scales;
  'data-testid'?: string;
  slots?: {
    Header?: React.ReactNode[];
    Content?: React.ReactNode[];
    Footer?: React.ReactNode[];
  };
}

export const CardWidget: React.FC<CardWidgetProps> = ({
  id,
  events,
  width,
  height,
  borderThickness,
  borderRadius,
  borderStyle,
  borderColor,
  hoverVariant,
  scale = Scales.Medium,
  slots,
  'data-testid': testId,
}) => {
  const eventHandler = useEventHandler();
  const sizeClasses = getSizeClasses(scale);

  const styles = {
    ...getWidth(width),
    ...getHeight(height),
    ...(borderStyle && getBorderStyle(borderStyle)),
    ...(borderThickness && getBorderThickness(borderThickness)),
    ...(borderRadius && getBorderRadius(borderRadius)),
    ...(borderColor && getColor(borderColor, 'borderColor', 'background')),
  };

  const footerIsEmpty =
    slots?.Footer?.length === 0 ||
    slots?.Footer?.some(
      node => React.isValidElement(node) && node.type === EmptyWidget
    );

  const headerIsEmpty =
    slots?.Header?.length === 0 ||
    slots?.Header?.some(
      node => React.isValidElement(node) && node.type === EmptyWidget
    );

  const handleClick = useCallback(() => {
    if (events.includes('OnClick')) eventHandler('OnClick', id, []);
  }, [id, eventHandler, events]);

  const hoverClass =
    hoverVariant === 'None'
      ? cardStyles.hover.none
      : hoverVariant === 'Pointer'
        ? cardStyles.hover.pointer
        : cardStyles.hover.pointerAndTranslate;

  return (
    <Card
      role="region"
      data-testid={testId}
      style={styles}
      className={cn(cardStyles.container, hoverClass)}
      onClick={handleClick}
    >
      {!headerIsEmpty ? (
        <CardHeader className={cn(cardStyles.header.base, sizeClasses.header)}>
          {slots?.Header}
        </CardHeader>
      ) : (
        <></>
      )}
      <CardContent
        className={cn(
          cardStyles.content.base,
          sizeClasses.content,
          headerIsEmpty && cardStyles.content.noHeader
        )}
      >
        {slots?.Content}
      </CardContent>
      {!footerIsEmpty && (
        <CardFooter className={cn(cardStyles.footer.base, sizeClasses.footer)}>
          {slots?.Footer}
        </CardFooter>
      )}
    </Card>
  );
};
