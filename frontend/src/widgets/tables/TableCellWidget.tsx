import React from 'react';
import { TableCell } from '@/components/ui/table';
import { cn } from '@/lib/utils';
import { Align, getWidth } from '@/lib/styles';
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

/**
 * Safe type guard for textual content.
 * More robust than typeof children === 'string'.
 */
const isStringContent = (
  content: React.ReactNode
): content is string | number =>
  typeof content === 'string' || typeof content === 'number';

// Convert Align enum to CSS
const getTextAlign = (align: Align): React.CSSProperties => {
  switch (align) {
    case 'TopLeft':
    case 'Left':
    case 'BottomLeft':
      return { textAlign: 'left' };
    case 'TopRight':
    case 'Right':
    case 'BottomRight':
      return { textAlign: 'right' };
    case 'TopCenter':
    case 'Center':
    case 'BottomCenter':
      return { textAlign: 'center' };
    default:
      return { textAlign: 'left' };
  }
};

export const TableCellWidget: React.FC<TableCellWidgetProps> = ({
  children,
  isHeader = false,
  isFooter = false,
  align,
  width,
  multiLine = false,
}) => {
  const cellStyles = {
    ...getWidth(width),
  };

  const textAlignStyle = getTextAlign(align);

  /**
   * Build inner content
   * - Wrapping
   * - Truncation
   * - Alignment
   * - Text safety
   */
  const content = (
    <div
      className={cn(
        'align-middle force-text-inherit',
        multiLine ? 'whitespace-normal break-words' : 'min-w-0'
      )}
      style={textAlignStyle}
    >
      {!multiLine ? (
        <span
          className="inline-block overflow-hidden text-ellipsis whitespace-nowrap max-w-full"
          style={textAlignStyle}
        >
          {children}
        </span>
      ) : (
        children
      )}
    </div>
  );

  /**
   * Truncation logic:
   * - Always truncate headers
   * - Truncate when explicit width is set
   */
  const shouldTruncate = isHeader || Boolean(width);

  /**
   * Tooltip logic:
   * Only show tooltip for string | number (safe type check)
   */
  const shouldShowTooltip = !multiLine && isStringContent(children);

  /**
   * Cell classes (cleaned using cn)
   */
  const cellClasses = cn(
    'border-border force-text-inherit',
    {
      'header-cell bg-muted font-semibold': isHeader,
      'footer-cell bg-muted font-semibold': isFooter,
      'max-w-0 overflow-hidden': shouldTruncate,
    }
  );

  return (
    <TableCell className={cellClasses} style={cellStyles}>
      {shouldShowTooltip ? (
        <TooltipProvider>
          <Tooltip>
            <TooltipTrigger asChild>{content}</TooltipTrigger>
            <TooltipContent className="bg-popover text-popover-foreground shadow-md max-w-sm">
              <div className="whitespace-pre-wrap break-words">{children}</div>
            </TooltipContent>
          </Tooltip>
        </TooltipProvider>
      ) : (
        content
      )}
    </TableCell>
  );
};

export default TableCellWidget;
