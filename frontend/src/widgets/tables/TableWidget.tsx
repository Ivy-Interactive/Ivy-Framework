import React from 'react';
import { Table, TableBody } from '@/components/ui/table';
import { getWidth } from '@/lib/styles';
import { Scales } from '@/types/scale';
import { cn } from '@/lib/utils';

interface TableWidgetProps {
  id: string;
  children?: React.ReactNode;
  width?: string;
  scale?: Scales;
}

export const TableWidget: React.FC<TableWidgetProps> = ({
  children,
  width,
  scale = Scales.Medium,
}) => {
  const styles = {
    ...getWidth(width),
  };

  return (
    <Table
      scale={scale}
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
