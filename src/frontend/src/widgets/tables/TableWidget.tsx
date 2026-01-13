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
  const widthStyles = getWidth(width);

  // For Full() width, use fixed layout and ensure maxWidth to prevent horizontal scroll
  // For fixed widths (Units, Px, Rem), remove maxWidth to allow expansion if needed
  const isFullWidth = width?.includes('Full');
  const isFixedWidth =
    width &&
    (width.includes('Units:') ||
      width.includes('Px:') ||
      width.includes('Rem:'));

  // For fixed widths, create new object without maxWidth property
  const tableStyles = isFixedWidth
    ? (Object.fromEntries(
        Object.entries(widthStyles).filter(([key]) => key !== 'maxWidth')
      ) as React.CSSProperties)
    : {
        ...widthStyles,
        // Ensure Full() width tables don't exceed container
        maxWidth: isFullWidth ? '100%' : widthStyles.maxWidth,
      };

  return (
    <Table
      scale={scale}
      className={cn('w-full caption-bottom')}
      style={{
        ...tableStyles,
        // Use fixed layout for Full() width to respect width constraints and prevent overflow
        // Use auto layout for fixed widths to allow natural sizing
        tableLayout: isFullWidth ? 'fixed' : 'auto',
      }}
    >
      <TableBody>{children}</TableBody>
    </Table>
  );
};
