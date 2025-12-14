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
import { EmptyWidget } from './primitives/EmptyWidget';
import { Scales } from '@/types/scale';

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

  const getSizeClasses = (scale?: Scales) => {
    switch (scale) {
      case Scales.Small:
        return {
          header:
            'px-3 py-1 items-center [&_:is(p,span)]:!pt-1 [&_:is(h1,h2,h3,h4,h5,h6)]:!text-sm [&_:is(h1,h2,h3,h4,h5,h6)]:!my-0 [&_:is(h1,h2,h3,h4,h5,h6)]:!font-normal [&_:is(h1,h2,h3,h4,h5,h6)]:!h-9 [&_:is(h1,h2,h3,h4,h5,h6)]:!flex [&_:is(h1,h2,h3,h4,h5,h6)]:!items-center',
          content: 'p-3 pt-0 [&_*]:!text-sm',
          footer: 'p-3 pt-0',
          title: 'text-sm',
          description: 'text-xs mt-1',
          icon: 'h-4 w-4',
        };
      case Scales.Large:
        return {
          header:
            'px-8 py-5 items-center [&_:is(p,span)]:!pt-1 [&_:is(h1,h2,h3,h4,h5,h6)]:!text-lg [&_:is(h1,h2,h3,h4,h5,h6)]:!my-0 [&_:is(h1,h2,h3,h4,h5,h6)]:!font-normal [&_:is(h1,h2,h3,h4,h5,h6)]:!h-9 [&_:is(h1,h2,h3,h4,h5,h6)]:!flex [&_:is(h1,h2,h3,h4,h5,h6)]:!items-center',
          content: 'p-8 pt-0',
          footer: 'p-8 pt-0',
          title: 'text-lg',
          description: 'text-base mt-3',
          icon: 'h-6 w-6',
        };
      default:
        return {
          header:
            'px-6 py-4 items-center [&_:is(p,span)]:!pt-1 [&_:is(h1,h2,h3,h4,h5,h6,p)]:!text-base [&_:is(h1,h2,h3,h4,h5,h6)]:!my-0 [&_:is(h1,h2,h3,h4,h5,h6)]:!font-normal [&_:is(h1,h2,h3,h4,h5,h6)]:!h-9 [&_:is(h1,h2,h3,h4,h5,h6)]:!flex [&_:is(h1,h2,h3,h4,h5,h6)]:!items-center',
          content: 'p-6 pt-0',
          footer: 'p-6 pt-0',
          title: 'text-base',
          description: 'text-sm',
          icon: 'h-5 w-5',
        };
    }
  };

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
      ? null
      : hoverVariant === 'Pointer'
        ? 'cursor-pointer'
        : 'cursor-pointer transform hover:-translate-x-[4px] hover:-translate-y-[4px] active:translate-x-[-2px] active:translate-y-[-2px] transition';

  return (
    <Card
      role="region"
      data-testid={testId}
      style={styles}
      className={cn('flex', 'flex-col', 'overflow-hidden', hoverClass)}
      onClick={handleClick}
    >
      {!headerIsEmpty ? (
        <CardHeader className={cn('flex-none', sizeClasses.header)}>
          {slots?.Header}
        </CardHeader>
      ) : (
        <></>
      )}
      <CardContent
        className={cn('flex-1', sizeClasses.content, headerIsEmpty && 'pt-6')}
      >
        {slots?.Content}
      </CardContent>
      {!footerIsEmpty && (
        <CardFooter className={cn('flex-none', sizeClasses.footer)}>
          {slots?.Footer}
        </CardFooter>
      )}
    </Card>
  );
};
