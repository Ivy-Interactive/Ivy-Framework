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

/**
 * Structured width config detected from the width string.
 * Prevents fragile string.includes() checks.
 */
interface WidthConfig {
  type: 'full' | 'units' | 'px' | 'rem' | 'unknown';
  value?: number;
}

/**
 * Parse width definition into structured config.
 * Matches Ivy's patterns like:
 * - Full()
 * - Units:100
 * - Px:300
 * - Rem:20
 */
const parseWidth = (width: string | undefined): WidthConfig => {
  if (!width) return { type: 'unknown' };

  if (width.includes('Full')) {
    return { type: 'full' };
  }
  if (width.includes('Units:')) {
    const num = Number(width.split('Units:')[1]);
    return { type: 'units', value: num };
  }
  if (width.includes('Px:')) {
    const num = Number(width.split('Px:')[1]);
    return { type: 'px', value: num };
  }
  if (width.includes('Rem:')) {
    const num = Number(width.split('Rem:')[1]);
    return { type: 'rem', value: num };
  }

  return { type: 'unknown' };
};

/** Whether width is fixed pixel/rem/units. */
const isFixedWidth = (cfg: WidthConfig) =>
  cfg.type === 'units' || cfg.type === 'px' || cfg.type === 'rem';

/** Base width styles from Ivy’s getWidth() */
const getBaseStyles = (width: string | undefined): React.CSSProperties => ({
  ...getWidth(width),
});

/** Remove maxWidth (for fixed-width tables) */
const omitMaxWidth = (styles: React.CSSProperties): React.CSSProperties => {
  const clone = { ...styles };
  delete clone.maxWidth;
  return clone;
};

/** Ensure maxWidth:100% for Full() width */
const applyMaxWidthConstraint = (
  styles: React.CSSProperties
): React.CSSProperties => ({
  ...styles,
  maxWidth: '100%',
});

/** Final table style builder */
const buildTableStyles = (
  width: string | undefined
): React.CSSProperties => {
  const cfg = parseWidth(width);
  const baseStyles = getBaseStyles(width);

  if (isFixedWidth(cfg)) {
    return omitMaxWidth(baseStyles);
  }

  if (cfg.type === 'full') {
    return applyMaxWidthConstraint(baseStyles);
  }

  return baseStyles;
};

export const TableWidget: React.FC<TableWidgetProps> = ({
  children,
  width,
  scale = Scales.Medium,
}) => {
  const cfg = parseWidth(width);
  const tableStyles = buildTableStyles(width);

  return (
    <Table
      scale={scale}
      className={cn('w-full caption-bottom')}
      style={{
        ...tableStyles,
        tableLayout: cfg.type === 'full' ? 'fixed' : 'auto',
      }}
    >
      <TableBody>{children}</TableBody>
    </Table>
  );
};
