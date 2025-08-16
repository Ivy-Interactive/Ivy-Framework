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
  children?: React.ReactNode;
}

export const TableCellWidget: React.FC<TableCellWidgetProps> = ({
  children,
  isHeader,
  isFooter,
  align,
  width,
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
        'border-border',
        'min-w-0'
      )}
      style={cellStyles}
    >
      <div style={alignStyles}>{children}</div>
    </TableCell>
  );
};
