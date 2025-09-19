import { getGap, getHeight, getPadding, getWidth } from '@/lib/styles';
import React from 'react';

interface GridLayoutWidgetProps {
  columns?: number;
  rows?: number;
  gap?: number;
  padding?: string;
  autoFlow?: 'Row' | 'Column' | 'RowDense' | 'ColumnDense';
  width?: string;
  height?: string;
  children: React.ReactNode[];
  childColumn?: (number | undefined)[];
  childColumnSpan?: (number | undefined)[];
  childRow?: (number | undefined)[];
  childRowSpan?: (number | undefined)[];
  className?: string;
  /** Optional minimum column width in pixels used to auto-resize columns responsively. Defaults to 280. */
  minColumnWidthPx?: number;
}

interface GridLayoutCellProps {
  children: React.ReactNode;
  column?: number;
  row?: number;
  columnSpan?: number;
  rowSpan?: number;
  className?: string;
}

const GridLayoutCell: React.FC<GridLayoutCellProps> = ({
  children,
  column,
  row,
  columnSpan,
  rowSpan,
  className,
}) => {
  const styles: React.CSSProperties = {
    ...{
      gridColumn: columnSpan ? `span ${columnSpan}` : undefined,
      gridRow: rowSpan ? `span ${rowSpan}` : undefined,
      gridColumnStart: column,
      gridRowStart: row,
    },
  };

  return (
    <div
      style={styles}
      className={`flex items-center h-full w-full ${className}`}
    >
      {children}
    </div>
  );
};

export const GridLayoutWidget: React.FC<GridLayoutWidgetProps> = ({
  children,
  columns = 1,
  rows = 1,
  autoFlow = 'Row',
  width,
  height,
  gap = 16,
  padding,
  childColumn = [],
  childColumnSpan = [],
  childRow = [],
  childRowSpan = [],
  className = '',
  minColumnWidthPx = 280,
}) => {
  const containerRef = React.useRef<HTMLDivElement>(null);
  const [calculatedColumns, setCalculatedColumns] = React.useState(columns);

  React.useEffect(() => {
    // Recalculate when props change
    setCalculatedColumns(columns);
  }, [columns]);

  React.useEffect(() => {
    const el = containerRef.current;
    if (!el) return;

    const observer = new ResizeObserver(entries => {
      const entry = entries[0];
      if (!entry) return;
      const width = entry.contentRect.width;
      if (!width || width <= 0) return;

      // Account for padding and gaps roughly: assume horizontal padding from computed style
      const styles = getComputedStyle(el);
      const paddingLeft = parseFloat(styles.paddingLeft || '0');
      const paddingRight = parseFloat(styles.paddingRight || '0');
      const gapX = gap; // gap is symmetrical for grid
      const available = Math.max(0, width - paddingLeft - paddingRight);
      const minWidth = Math.max(120, minColumnWidthPx); // sanity lower bound
      const maxCols = Math.max(
        1,
        Math.floor((available + gapX) / (minWidth + gapX))
      );
      const next = Math.min(columns, maxCols);
      if (next !== calculatedColumns) {
        setCalculatedColumns(next);
      }
    });

    observer.observe(el);
    return () => observer.disconnect();
  }, [columns, gap, minColumnWidthPx, calculatedColumns]);

  const styles: React.CSSProperties = {
    ...{
      display: 'grid',
      gridTemplateColumns: `repeat(${calculatedColumns}, minmax(0, 1fr))`,
      gridTemplateRows: `repeat(${rows}, minmax(0, 1fr))`,
      gridAutoFlow: autoFlow?.toLowerCase(),
    },
    ...getPadding(padding),
    ...getGap(gap),
    ...getWidth(width),
    ...getHeight(height),
  };

  return (
    <div
      ref={containerRef}
      style={styles}
      className={`place-items-center ${className}`}
    >
      {React.Children.map(children, (child, index) => (
        <GridLayoutCell
          column={childColumn[index]}
          columnSpan={childColumnSpan[index]}
          row={childRow[index]}
          rowSpan={childRowSpan[index]}
          className={
            React.isValidElement(child)
              ? (child.props as { className?: string }).className
              : ''
          }
        >
          {child}
        </GridLayoutCell>
      ))}
    </div>
  );
};

export default GridLayoutWidget;
