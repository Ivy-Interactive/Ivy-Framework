import React from 'react';
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from '@/components/ui/tooltip';
import { cn } from '@/lib/utils';

export type TooltipOptions = {
  open?: boolean;
  contentClassName?: string;
  triggerAsChild?: boolean;
};

export class TooltipHelper {
  static wrap(
    node: React.ReactNode,
    content?: React.ReactNode,
    options?: TooltipOptions
  ): React.ReactElement {
    const { open, contentClassName, triggerAsChild = true } = options || {};
    return (
      <TooltipProvider>
        <Tooltip open={open}>
          <TooltipTrigger asChild={triggerAsChild}>{node}</TooltipTrigger>
          {content ? (
            <TooltipContent
              className={cn(
                'bg-popover text-popover-foreground shadow-md',
                contentClassName
              )}
            >
              {content}
            </TooltipContent>
          ) : null}
        </Tooltip>
      </TooltipProvider>
    );
  }
}

export function wrapWithTooltip(
  node: React.ReactNode,
  content?: React.ReactNode,
  options?: TooltipOptions
): React.ReactElement {
  return TooltipHelper.wrap(node, content, options);
}
