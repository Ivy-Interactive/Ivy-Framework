import React from 'react';
import { TableRow } from '@/components/ui/table';
import { Sizes } from '@/types/sizes';

interface TableRowWidgetProps {
  id: string;
  isHeader?: boolean;
  isFooter?: boolean;
  size?: Sizes;
  children?: React.ReactNode;
}

export const TableRowWidget: React.FC<TableRowWidgetProps> = ({
  children,
  isHeader = false,
}) => (
  <TableRow className={`${isHeader ? 'font-medium bg-background' : ''}`}>
    {children}
  </TableRow>
);
