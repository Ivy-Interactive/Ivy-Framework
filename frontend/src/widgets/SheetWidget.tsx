import { useEventHandler } from '@/components/EventHandlerContext';
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet';
import { getWidth } from '@/lib/styles';
import { cn } from '@/lib/utils';
import React from 'react';

interface SheetWidgetProps {
  id: string;
  title?: string;
  description?: string;
  width?: string;
  slots?: {
    Content?: React.ReactNode[];
  };
}

export const SheetWidget: React.FC<SheetWidgetProps> = ({
  slots,
  title,
  description,
  id,
  width,
}) => {
  const eventHandler = useEventHandler();

  if (!slots?.Content) {
    return (
      <div className="text-red-500">
        Error: Sheet requires both Trigger and Content slots.
      </div>
    );
  }

  const styles: React.CSSProperties = {
    ...getWidth(width),
  };

  return (
    <Sheet open={true} onOpenChange={() => eventHandler('OnClose', id, [])}>
      <SheetContent
        style={styles}
        className={cn('h-full flex flex-col p-0 gap-0')}
        onOpenAutoFocus={e => {
          e.preventDefault();
        }}
      >
        {(title || description) && true && (
          <SheetHeader className="p-4 pb-0">
            {title && <SheetTitle>{title}</SheetTitle>}
            {description && <SheetDescription>{description}</SheetDescription>}
          </SheetHeader>
        )}
        <div className="flex-1 pb-0 pt-0 pl-4 pr-4 mt-4">{slots.Content}</div>
      </SheetContent>
    </Sheet>
  );
};
