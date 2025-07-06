import React from 'react';
import { TableRow } from '@/components/ui/table';

interface TableRowWidgetProps {
  id: string;
  isHeader?: boolean;
  isFooter?: boolean;
  children?: React.ReactNode;
}

export const TableRowWidget: React.FC<TableRowWidgetProps> = ({
  children,
  isHeader = false,
}) => (
  <TableRow className={`${isHeader ? 'font-medium bg-white' : ''}`}>
    {children}
  </TableRow>
);
