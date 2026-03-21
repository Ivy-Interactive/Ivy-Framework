import React from 'react';
import { Table, TableBody } from '@/components/ui/table';
import { getWidth } from '@/lib/styles';
import { Densities } from '@/types/density';
import { cn } from '@/lib/utils';

interface TableWidgetProps {
  id: string;
  children?: React.ReactNode;
  width?: string;
  density?: Densities;
}

export const TableWidget: React.FC<TableWidgetProps> = ({
  children,
  width,
  density = Densities.Medium,
}) => {
  const widthStyles = getWidth(width || 'Full');

  return (
    <Table
      density={density}
      className={cn('w-full caption-bottom')}
      style={{
        ...widthStyles,
        ...(widthStyles.width === '100%'
          ? { maxWidth: '100%', tableLayout: 'fixed' as const }
          : { tableLayout: 'auto' as const }),
      }}
    >
      <TableBody>{children}</TableBody>
    </Table>
  );
};
