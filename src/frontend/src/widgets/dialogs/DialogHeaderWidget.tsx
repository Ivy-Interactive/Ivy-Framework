import * as DialogPrimitive from '@radix-ui/react-dialog';
import { X } from 'lucide-react';
import React from 'react';
import { cn } from '@/lib/utils';

interface DialogHeaderWidgetProps {
  id: string;
  title: string;
  /** When true, the dialog header close control is not rendered (server: DialogHeader.HideCloseButton). */
  hideCloseButton?: boolean;
}

export const DialogHeaderWidget: React.FC<DialogHeaderWidgetProps> = ({
  title,
  hideCloseButton,
}) => (
  <div
    className={cn(
      'sticky top-0 z-10 bg-background p-4 shrink-0 flex items-center justify-between',
      'flex gap-2'
    )}
  >
    <div className="flex-1">
      <DialogPrimitive.Title
        className={cn('text-base font-semibold leading-none tracking-tight')}
      >
        {title}
      </DialogPrimitive.Title>
    </div>
    {!hideCloseButton && (
      <DialogPrimitive.Close className="p-1 rounded-selector hover:bg-accent focus:outline-none cursor-pointer">
        <X className="h-4 w-4 text-muted-foreground hover:text-foreground" />
        <span className="sr-only">Close</span>
      </DialogPrimitive.Close>
    )}
  </div>
);
