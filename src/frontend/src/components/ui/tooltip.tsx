import * as React from 'react';
import * as TooltipPrimitive from '@radix-ui/react-tooltip';

import { cn } from '@/lib/utils';

const TooltipProvider = TooltipPrimitive.Provider;

const Tooltip = TooltipPrimitive.Root;

const TooltipTrigger = TooltipPrimitive.Trigger;

const hasNoSpaces = (str: string): boolean => {
  const trimmed = str.trim();
  return trimmed.length > 0 && !trimmed.includes(' ');
};

const needsBreakAll = (content: React.ReactNode): boolean => {
  const checkString = (str: string): boolean => {
    return hasNoSpaces(str);
  };

  if (typeof content === 'string') {
    return checkString(content);
  }

  if (React.isValidElement(content)) {
    const props = content.props as { children?: React.ReactNode };
    const children = props?.children;
    if (typeof children === 'string') {
      return checkString(children);
    }
    if (Array.isArray(children)) {
      return children.some(child => needsBreakAll(child));
    }
    if (children) {
      return needsBreakAll(children);
    }
  }

  return false;
};

export type TooltipBreakType = 'auto' | 'normal' | 'all' | 'words';

interface TooltipContentProps extends React.ComponentPropsWithoutRef<
  typeof TooltipPrimitive.Content
> {
  breakType?: TooltipBreakType;
}

const TooltipContent = React.forwardRef<
  React.ElementRef<typeof TooltipPrimitive.Content>,
  TooltipContentProps
>(
  (
    { className, sideOffset = 4, breakType = 'auto', children, ...props },
    ref
  ) => {
    const getBreakClass = React.useMemo(() => {
      if (breakType !== 'auto') {
        switch (breakType) {
          case 'normal':
            return 'break-normal';
          case 'all':
            return 'break-all';
          case 'words':
            return 'break-words';
          default:
            return 'break-normal';
        }
      }

      return needsBreakAll(children) ? 'break-all' : 'break-normal';
    }, [breakType, children]);

    return (
      <TooltipPrimitive.Portal>
        <TooltipPrimitive.Content
          ref={ref}
          sideOffset={sideOffset}
          className={cn(
            'z-50 overflow-hidden rounded-selector bg-primary px-3 py-1.5 text-xs text-primary-foreground animate-in fade-in-0 zoom-in-95 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:zoom-out-95 data-[side=bottom]:slide-in-from-top-2 data-[side=left]:slide-in-from-right-2 data-[side=right]:slide-in-from-left-2 data-[side=top]:slide-in-from-bottom-2 max-w-sm',
            className
          )}
          {...props}
        >
          <div className={cn('max-h-[20vh] overflow-hidden', getBreakClass)}>
            {children}
          </div>
        </TooltipPrimitive.Content>
      </TooltipPrimitive.Portal>
    );
  }
);
TooltipContent.displayName = TooltipPrimitive.Content.displayName;

export { Tooltip, TooltipTrigger, TooltipContent, TooltipProvider };
export type { TooltipContentProps };
