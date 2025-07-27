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
import React, { useEffect, useState } from 'react';

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
  const [isOpen, setIsOpen] = useState(false);

  useEffect(() => {
    // Set sheet to open state when component mounts
    setIsOpen(true);
  }, []);

  const handleClose = () => {
    setIsOpen(false);
    // Delay the event handler to allow animation to complete
    setTimeout(() => eventHandler('OnClose', id, []), 300);
  };

  if (!slots?.Content) {
    return (
      <div className="text-destructive">
        Error: Sheet requires both Trigger and Content slots.
      </div>
    );
  }

  const styles: React.CSSProperties = {
    ...getWidth(width),
  };

  return (
    <Sheet open={isOpen} onOpenChange={handleClose}>
      <SheetContent
        style={styles}
        className={cn(
          'h-full flex flex-col p-0 gap-0',
          isOpen ? 'sheet-animate-enter' : 'sheet-animate-exit'
        )}
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
