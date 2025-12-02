import React from 'react';
import {
  getGap,
  getPadding,
  getWidth,
  getHeight,
  convertSizeToGridValue,
} from '../../lib/styles';

interface GridLayoutWidgetProps {
  columns?: number;
  rows?: number;
  gap?: number;
  padding?: string;
  autoFlow?: 'Row' | 'Column' | 'RowDense' | 'ColumnDense';
  width?: string;
  height?: string;
  columnWidths?: string[];
  rowHeights?: string[];
  children: React.ReactNode[];
  childColumn?: (number | undefined)[];
  childColumnSpan?: (number | undefined)[];
  childRow?: (number | undefined)[];
  childRowSpan?: (number | undefined)[];
  className?: string;
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
  columnWidths,
  rowHeights,
  childColumn = [],
  childColumnSpan = [],
  childRow = [],
  childRowSpan = [],
  className = '',
}) => {
  const styles: React.CSSProperties = {
    display: 'grid',
    gridTemplateColumns: columnWidths
      ? columnWidths.map(convertSizeToGridValue).join(' ')
      : `repeat(${columns}, minmax(0, 1fr))`,
    gridTemplateRows: rowHeights
      ? rowHeights.map(convertSizeToGridValue).join(' ')
      : `repeat(${rows}, minmax(0, 1fr))`,
    gridAutoFlow: autoFlow?.toLowerCase() || 'row',
    ...getPadding(padding),
    ...getGap(gap),
    ...getWidth(width),
    ...getHeight(height),
  };

  return (
    <div style={styles} className={`place-items-center ${className}`}>
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
