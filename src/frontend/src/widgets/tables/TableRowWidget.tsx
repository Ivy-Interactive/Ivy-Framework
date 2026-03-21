import React from 'react';
import { TableRow } from '@/components/ui/table';
import { Densities } from '@/types/density';

interface TableRowWidgetProps {
  id: string;
  isHeader?: boolean;
  isFooter?: boolean;
  density?: Densities;
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
