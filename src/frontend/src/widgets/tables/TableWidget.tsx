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
  const styles = getWidth(width);

  const isFullWidth = styles.width === '100%';
  const isFixedWidth = Boolean(width && styles.width && !isFullWidth);

  const tableStyles: React.CSSProperties = isFixedWidth
    ? (Object.fromEntries(
        Object.entries(styles).filter(([k]) => k !== 'maxWidth')
      ) as React.CSSProperties)
    : { ...styles, maxWidth: isFullWidth ? '100%' : styles.maxWidth };

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
