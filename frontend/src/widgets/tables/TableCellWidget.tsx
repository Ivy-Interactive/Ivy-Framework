import React from 'react';
import { TableCell } from '@/components/ui/table';
import { cn } from '@/lib/utils';
import { Align, getAlign, getWidth } from '@/lib/styles';

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

  return (
    <TableCell
      className={cn(
        isHeader && 'header-cell bg-muted font-semibold',
        isFooter && 'footer-cell bg-muted font-semibold',
        'border-border'
      )}
      style={cellStyles}
    >
      <div
        className={cn(
          'align-middle text-body min-w-0',
          multiLine && 'break-all'
        )}
        style={alignStyles}
      >
        {children}
      </div>
    </TableCell>
  );
};
