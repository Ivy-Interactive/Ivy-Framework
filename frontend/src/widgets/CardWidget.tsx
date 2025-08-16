import Icon from '@/components/Icon';
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { getHeight, getWidth } from '@/lib/styles';
import { cn } from '@/lib/utils';
import React from 'react';
import { EmptyWidget } from './primitives/EmptyWidget';

interface CardWidgetProps {
  id: string;
  title?: string;
  description?: string;
  icon?: string;
  width?: string;
  height?: string;
  slots?: {
    Content?: React.ReactNode[];
    Footer?: React.ReactNode[];
  };
}

export const CardWidget: React.FC<CardWidgetProps> = ({
  title,
  description,
  icon,
  width,
  height,
  slots,
}) => {
  const styles = {
    ...getWidth(width),
    ...getHeight(height),
  };

  const footerIsEmpty =
    slots?.Footer?.length === 0 ||
    slots?.Footer?.some(
      node => React.isValidElement(node) && node.type === EmptyWidget
    );

  const headerIsEmpty = !title && !description && !icon;

  return (
    <Card style={styles} className={cn('flex', 'flex-col', 'overflow-hidden')}>
      {!headerIsEmpty ? (
        <CardHeader className="flex flex-row items-center justify-between gap-4">
          <div className="flex flex-col">
            {title && <CardTitle>{title}</CardTitle>}
            {description && (
              <CardDescription className="mt-2">{description}</CardDescription>
            )}
          </div>
          {icon && (
            <Icon name={icon} className="h-5 w-5 text-muted-foreground" />
          )}
        </CardHeader>
      ) : (
        <></>
      )}
      <CardContent className={cn('flex-1', headerIsEmpty && 'pt-6')}>
        {slots?.Content}
      </CardContent>
      {!footerIsEmpty && (
        <CardFooter className="flex-none">{slots?.Footer}</CardFooter>
      )}
    </Card>
  );
};
