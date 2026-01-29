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
  width = 'Full',
  scale = Scales.Medium,
}) => {
  const widthStyles = getWidth(width);

  const isFullWidth = widthStyles.width === '100%';
  const isFixedSize = Boolean(width && widthStyles.width) && !isFullWidth;

  const tableStyles: React.CSSProperties = isFixedSize
    ? (Object.fromEntries(
        Object.entries(widthStyles).filter(([key]) => key !== 'maxWidth')
      ) as React.CSSProperties)
    : {
        ...widthStyles,
        maxWidth: isFullWidth ? '100%' : widthStyles.maxWidth,
      };

  return (
    <Table
      scale={scale}
      className={cn('w-full caption-bottom')}
      style={{
        ...tableStyles,
        tableLayout: isFullWidth ? 'fixed' : 'auto',
      }}
    >
      <TableBody>{children}</TableBody>
    </Table>
  );
};
