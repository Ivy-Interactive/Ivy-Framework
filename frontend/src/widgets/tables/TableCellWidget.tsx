import React from 'react';
import { TableCell } from '@/components/ui/table';
import { cn } from '@/lib/utils';
import { Align, getAlign, getWidth } from '@/lib/styles';
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from '@/components/ui/tooltip';
import './table.css';

interface TableCellWidgetProps {
  id: string;
  isHeader?: boolean;
  isFooter?: boolean;
  align: Align;
  width?: string;
  multiLine?: boolean;
  children?: React.ReactNode;
}

export const TableCellWidget: React.FC<TableCellWidgetProps> = ({
  children,
  isHeader,
  isFooter,
  align,
  width,
  multiLine,
}) => {
  const alignStyles = {
    ...getAlign('Horizontal', align),
    ...getAlign('Vertical', align),
  };

  const cellStyles = {
    ...getWidth(width),
  };

  const content = (
    <div
      className={cn(
        'align-middle force-text-inherit',
        multiLine && 'whitespace-normal wrap-break-word',
        !multiLine && 'min-w-0'
      )}
      style={alignStyles}
    >
      {!multiLine ? (
        <span className="block overflow-hidden text-ellipsis whitespace-nowrap w-full">
          {children}
        </span>
      ) : (
        children
      )}
    </div>
  );

  // Apply max-w-0 overflow-hidden for truncation when:
  // 1. We have an explicit width (for column width control), OR
  // 2. It's a header cell (headers should truncate)
  // Don't apply to data cells without widths - they need to size naturally
  const shouldTruncate = width || isHeader;

  // Only show tooltip for string children to avoid "[object Object]" issues
  const shouldShowTooltip = !multiLine && typeof children === 'string';

  return (
    <TableCell
      className={cn(
        isHeader && 'header-cell bg-muted font-semibold',
        isFooter && 'footer-cell bg-muted font-semibold',
        'border-border force-text-inherit',
        // Apply max-w-0 overflow-hidden for truncation
        shouldTruncate && 'max-w-0 overflow-hidden'
      )}
      style={cellStyles}
    >
      {shouldShowTooltip ? (
        <TooltipProvider>
          <Tooltip>
            <TooltipTrigger asChild>{content}</TooltipTrigger>
            <TooltipContent className="bg-popover text-popover-foreground shadow-md max-w-sm">
              <div className="whitespace-pre-wrap wrap-break-word">
                {children}
              </div>
            </TooltipContent>
          </Tooltip>
        </TooltipProvider>
      ) : (
        content
      )}
    </TableCell>
  );
};
