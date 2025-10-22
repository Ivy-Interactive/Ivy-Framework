import Icon from '@/components/Icon';
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
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
import { Sizes } from '@/types/sizes';

interface CardWidgetProps {
  id: string;
  events: string[];
  title?: string;
  description?: string;
  icon?: string;
  width?: string;
  height?: string;
  borderThickness?: string;
  borderRadius?: BorderRadius;
  borderStyle?: BorderStyle;
  borderColor?: string;
  hoverVariant?: 'None' | 'Pointer' | 'PointerAndTranslate';
  size?: Sizes;
  'data-testid'?: string;
  slots?: {
    Content?: React.ReactNode[];
    Footer?: React.ReactNode[];
  };
}

export const CardWidget: React.FC<CardWidgetProps> = ({
  id,
  events,
  title,
  description,
  icon,
  width,
  height,
  borderThickness,
  borderRadius,
  borderStyle,
  borderColor,
  hoverVariant,
  size = Sizes.Medium,
  slots,
  'data-testid': testId,
}) => {
  const eventHandler = useEventHandler();

  const getSizeClasses = (size: Sizes) => {
    switch (size) {
      case Sizes.Small:
        return {
          header: 'p-3',
          content: 'p-3 pt-0 [&_*]:text-xs',
          footer: 'p-3 pt-0',
          title: 'text-sm',
          description: 'text-xs mt-1',
          icon: 'h-4 w-4',
        };
      case Sizes.Large:
        return {
          header: 'p-8',
          content: 'p-8 pt-0 [&_*]:text-base',
          footer: 'p-8 pt-0',
          title: 'text-lg',
          description: 'text-base mt-3',
          icon: 'h-6 w-6',
        };
      default:
        return {
          header: 'p-6',
          content: 'p-6 pt-0 [&_*]:text-sm',
          footer: 'p-6 pt-0',
          title: 'text-base',
          description: 'text-sm mt-2',
          icon: 'h-5 w-5',
        };
    }
  };

  const sizeClasses = getSizeClasses(size);

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

  const headerIsEmpty = !title && !description && !icon;

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
        <CardHeader
          className={cn(
            'flex flex-row items-center justify-between gap-4',
            sizeClasses.header
          )}
        >
          <div className="flex flex-col">
            {title && (
              <CardTitle className={sizeClasses.title}>{title}</CardTitle>
            )}
            {description && (
              <CardDescription className={sizeClasses.description}>
                {description}
              </CardDescription>
            )}
          </div>
          {icon && (
            <Icon
              name={icon}
              className={cn(sizeClasses.icon, 'text-muted-foreground')}
            />
          )}
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
