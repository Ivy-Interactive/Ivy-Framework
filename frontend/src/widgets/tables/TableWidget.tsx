import React from 'react';
import { Table, TableBody } from '@/components/ui/table';
import { getWidth } from '@/lib/styles';

interface TableWidgetProps {
  id: string;
  children?: React.ReactNode;
  width?: string;
}

export const TableWidget: React.FC<TableWidgetProps> = ({
  children,
  width,
}) => {
  const styles = {
    ...getWidth(width),
  };

  return (
    <Table className="text-sm w-fit" style={styles}>
      <TableBody>{children}</TableBody>
    </Table>
  );
};
