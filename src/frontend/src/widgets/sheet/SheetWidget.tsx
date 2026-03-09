import { useEventHandler } from '@/components/event-handler';
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet';
import { getHeight, getWidth } from '@/lib/styles';
import { cn } from '@/lib/utils';
import React, { useState } from 'react';
import './sheet.css';

type SheetSide = 'left' | 'right' | 'top' | 'bottom';

const normalizeSide = (side?: string): SheetSide => {
  if (!side) return 'right';
  return side.toLowerCase() as SheetSide;
};

interface SheetWidgetProps {
  id: string;
  title?: string;
  description?: string;
  width?: string;
  side?: SheetSide;
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
  side = 'right',
}) => {
  const eventHandler = useEventHandler();
  const [isOpen, setIsOpen] = useState(true);

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

  const normalizedSide = normalizeSide(side);
  const isHorizontal = normalizedSide === 'left' || normalizedSide === 'right';

  const styles: React.CSSProperties = isHorizontal
    ? { ...getWidth(width) }
    : { ...getHeight(width) };

  return (
    <Sheet open={isOpen} onOpenChange={handleClose}>
      <SheetContent
        side={normalizedSide}
        style={styles}
        className={cn('flex flex-col p-0 gap-0', isHorizontal && 'h-full')}
        data-sheet-side={normalizedSide}
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
        <div className="flex-1 pb-4 pt-1 pl-4 pr-4 mt-4 overflow-y-auto">
          {slots.Content}
        </div>
      </SheetContent>
    </Sheet>
  );
};
