'use client';

import * as React from 'react';
import * as DialogPrimitive from '@radix-ui/react-dialog';
import { X } from 'lucide-react';

import { cn } from '@/lib/utils';

const Dialog = DialogPrimitive.Root;

const DialogTrigger = DialogPrimitive.Trigger;

const DialogPortal = DialogPrimitive.Portal;

const DialogClose = DialogPrimitive.Close;

const DialogOverlay = React.forwardRef<
  React.ElementRef<typeof DialogPrimitive.Overlay>,
  React.ComponentPropsWithoutRef<typeof DialogPrimitive.Overlay>
>(({ className, ...props }, ref) => (
  <DialogPrimitive.Overlay
    ref={ref}
    className={cn(
      'fixed inset-0 z-50 bg-black/30  data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0',
      className
    )}
    {...props}
  />
));
DialogOverlay.displayName = DialogPrimitive.Overlay.displayName;

const DialogContent = React.forwardRef<
  React.ElementRef<typeof DialogPrimitive.Content>,
  React.ComponentPropsWithoutRef<typeof DialogPrimitive.Content>
>(({ className, children, ...props }, ref) => (
  <DialogPortal>
    <DialogOverlay />
    <DialogPrimitive.Content
      ref={ref}
      className={cn(
        'fixed left-[50%] top-[50%] z-50 flex flex-col w-full max-w-lg max-h-[85vh] translate-x-[-50%] translate-y-[-50%] border bg-background shadow-lg duration-200 data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0 data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95 data-[state=closed]:slide-out-to-left-1/2 data-[state=closed]:slide-out-to-top-[48%] data-[state=open]:slide-in-from-left-1/2 data-[state=open]:slide-in-from-top-[48%] sm:rounded-lg overflow-hidden focus:outline-none focus:ring-0',
        className
      )}
      {...props}
    >
      {children}
    </DialogPrimitive.Content>
  </DialogPortal>
));
DialogContent.displayName = DialogPrimitive.Content.displayName;

const DialogHeader = ({
  className,
  children,
  ...props
}: React.HTMLAttributes<HTMLDivElement>) => {
  // Helper function to check if children contain meaningful title content
  const hasMeaningfulTitle = React.useMemo(() => {
    let hasTitle = false;

    React.Children.forEach(children, child => {
      if (typeof child === 'string') {
        if (child.trim().length > 0) {
          hasTitle = true;
        }
      } else if (typeof child === 'number') {
        hasTitle = true;
      } else if (React.isValidElement(child)) {
        // Check if it's a DialogTitle component by checking the displayName
        if (
          child.type &&
          (child.type as { displayName?: string }).displayName === 'DialogTitle'
        ) {
          // Check if DialogTitle has meaningful content
          if (
            child.props &&
            typeof child.props === 'object' &&
            'children' in child.props
          ) {
            const titleChildren = (child.props as { children: React.ReactNode })
              .children;
            if (typeof titleChildren === 'string') {
              if (titleChildren.trim().length > 0) {
                hasTitle = true;
              }
            } else if (typeof titleChildren === 'number') {
              hasTitle = true;
            } else if (React.isValidElement(titleChildren)) {
              hasTitle = true;
            } else if (Array.isArray(titleChildren)) {
              // Check array of children
              React.Children.forEach(titleChildren, grandChild => {
                if (
                  typeof grandChild === 'string' &&
                  grandChild.trim().length > 0
                ) {
                  hasTitle = true;
                } else if (
                  typeof grandChild === 'number' ||
                  React.isValidElement(grandChild)
                ) {
                  hasTitle = true;
                }
              });
            }
          }
        }
      }
    });

    return hasTitle;
  }, [children]);

  return (
    <div
      className={cn(
        'sticky top-0 z-10 bg-background flex-shrink-0 flex flex-row items-center',
        className
      )}
      {...props}
    >
      <div className="flex-1"></div>
      <div className="flex-1 text-center">{children}</div>
      <div className="flex-1 flex justify-end">
        <DialogPrimitive.Close className="p-1 rounded hover:bg-accent focus:outline-none cursor-pointer">
          <X className="h-4 w-4 text-muted-foreground hover:text-foreground" />
          <span className="sr-only">Close</span>
        </DialogPrimitive.Close>
      </div>
      {hasMeaningfulTitle && (
        <div className="absolute bottom-0 left-0 right-0 h-px bg-border"></div>
      )}
    </div>
  );
};
DialogHeader.displayName = 'DialogHeader';

const DialogFooter = ({
  className,
  ...props
}: React.HTMLAttributes<HTMLDivElement>) => (
  <div
    className={cn(
      'flex flex-col-reverse sm:flex-row sm:justify-center sm:space-x-2 flex-shrink-0 px-6 pb-6',
      className
    )}
    {...props}
  />
);
DialogFooter.displayName = 'DialogFooter';

const DialogTitle = React.forwardRef<
  React.ElementRef<typeof DialogPrimitive.Title>,
  React.ComponentPropsWithoutRef<typeof DialogPrimitive.Title>
>(({ className, ...props }, ref) => (
  <DialogPrimitive.Title
    ref={ref}
    className={cn(
      'text-base font-semibold leading-none tracking-tight',
      className
    )}
    {...props}
  />
));
DialogTitle.displayName = DialogPrimitive.Title.displayName;

const DialogDescription = React.forwardRef<
  React.ElementRef<typeof DialogPrimitive.Description>,
  React.ComponentPropsWithoutRef<typeof DialogPrimitive.Description>
>(({ className, ...props }, ref) => (
  <DialogPrimitive.Description
    ref={ref}
    className={cn('text-sm text-muted-foreground', className)}
    {...props}
  />
));
DialogDescription.displayName = DialogPrimitive.Description.displayName;

export {
  Dialog,
  DialogPortal,
  DialogOverlay,
  DialogTrigger,
  DialogClose,
  DialogContent,
  DialogHeader,
  DialogFooter,
  DialogTitle,
  DialogDescription,
};
