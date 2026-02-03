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
  const widthStyles = getWidth(width || 'Full');
  const isFull = widthStyles.width === '100%';

  return (
    <Table
      scale={scale}
      className={cn('w-full caption-bottom')}
      style={{
        ...widthStyles,
        tableLayout: isFull ? 'fixed' : 'auto',
        ...(isFull && { maxWidth: '100%' }),
      }}
    >
      <TableBody>{children}</TableBody>
    </Table>
  );
};
