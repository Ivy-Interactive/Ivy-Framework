import React from 'react';
import { Table, TableBody } from '@/components/ui/table';
import { getWidth } from '@/lib/styles';
import { Sizes } from '@/types/sizes';
import { cn } from '@/lib/utils';

interface TableWidgetProps {
  id: string;
  children?: React.ReactNode;
  width?: string;
  size?: Sizes;
}

export const TableWidget: React.FC<TableWidgetProps> = ({
  children,
  width,
  size,
}) => {
  const styles = {
    ...getWidth(width),
  };

  return (
    <Table
      size={size}
      className={cn('w-full')}
      style={{
        ...styles,
        tableLayout: 'fixed',
      }}
    >
      <TableBody>{children}</TableBody>
    </Table>
  );
};
