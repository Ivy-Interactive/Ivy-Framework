import React from 'react';
import { TableRow } from '@/components/ui/table';
import { Scales } from '@/types/scale';

interface TableRowWidgetProps {
  id: string;
  isHeader?: boolean;
  isFooter?: boolean;
  scale?: Scales;
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
