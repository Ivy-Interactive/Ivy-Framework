import React from "react";
import {
  Align,
  getRowGap,
  getColumnGap,
  getPadding,
  getWidth,
  getHeight,
  convertSizeToGridValue,
  gridCellOverflow,
  getAlignSelf,
} from "../../lib/styles";

const EMPTY_ARRAY: never[] = [];

interface GridLayoutWidgetProps {
  columns?: number;
  rows?: number;
  rowGap?: number;
  columnGap?: number;
  padding: string;
  autoFlow?: "Row" | "Column" | "RowDense" | "ColumnDense";
  width?: string;
  height?: string;
  columnWidths?: string[];
  rowHeights?: string[];
  children: React.ReactNode[];
  childColumn?: (number | undefined)[];
  childColumnSpan?: (number | undefined)[];
  childRow?: (number | undefined)[];
  childRowSpan?: (number | undefined)[];
  childAlignSelf?: (Align | undefined)[];
  className?: string;
}

interface GridLayoutCellProps {
  children: React.ReactNode;
  column?: number;
  row?: number;
  columnSpan?: number;
  rowSpan?: number;
  alignSelf?: Align;
  className?: string;
}

const GridLayoutCell: React.FC<GridLayoutCellProps> = ({
  children,
  column,
  row,
  columnSpan,
  rowSpan,
  alignSelf,
  className,
}) => {
  const styles: React.CSSProperties = {
    gridColumn: columnSpan ? `span ${columnSpan}` : undefined,
    gridRow: rowSpan ? `span ${rowSpan}` : undefined,
    gridColumnStart: column,
    gridRowStart: row,
    ...getAlignSelf(alignSelf),
  };

  return (
    <div
      style={styles}
      className={`relative flex items-center h-full w-full min-w-0 ${gridCellOverflow.ellipsis} ${className}`}
    >
      {children}
    </div>
  );
};

export const GridLayoutWidget: React.FC<GridLayoutWidgetProps> = ({
  children,
  columns = 1,
  rows = 1,
  autoFlow,
  width,
  height,
  rowGap = 4,
  columnGap = 4,
  padding = "0,0,0,0",
  columnWidths,
  rowHeights,
  childColumn = EMPTY_ARRAY,
  childColumnSpan = EMPTY_ARRAY,
  childRow = EMPTY_ARRAY,
  childRowSpan = EMPTY_ARRAY,
  childAlignSelf = EMPTY_ARRAY,
  className = "",
}) => {
  const styles: React.CSSProperties = {
    display: "grid",
    gridTemplateColumns: columnWidths
      ? columnWidths.map(convertSizeToGridValue).join(" ")
      : `repeat(${columns}, minmax(0, 1fr))`,
    gridTemplateRows: rowHeights
      ? rowHeights.map(convertSizeToGridValue).join(" ")
      : `repeat(${rows}, minmax(0, 1fr))`,
    gridAutoFlow: autoFlow?.toLowerCase() || "row",
    ...getPadding(padding),
    ...getRowGap(rowGap),
    ...getColumnGap(columnGap),
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
          alignSelf={childAlignSelf[index]}
          className={
            React.isValidElement(child) ? (child.props as { className?: string }).className : ""
          }
        >
          {child}
        </GridLayoutCell>
      ))}
    </div>
  );
};

export default GridLayoutWidget;
