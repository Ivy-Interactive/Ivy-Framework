import { getGap, getHeight, getPadding, getWidth } from "@/lib/styles";
import React from "react";

interface GridLayoutWidgetProps {
  columns?: number;
  rows?: number;
  gap?: number;
  padding?: string;
  autoFlow?: "Row" | "Column" | "RowDense" | "ColumnDense";
  width?: string;
  height?: string;
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
    <div style={styles} className={className}>
      {children}
    </div>
  );
};

export const GridLayoutWidget: React.FC<GridLayoutWidgetProps> = ({
  children,
  columns = 1,
  rows = 1,
  autoFlow = "Row",
  width,
  height,
  gap = 16,
  padding,
  childColumn = [],
  childColumnSpan = [],
  childRow = [],
  childRowSpan = [],
  className = "",
}) => {
  const styles: React.CSSProperties = {
    ...{
      display: "grid",
      gridTemplateColumns: `repeat(${columns}, minmax(0, 1fr))`,
      gridTemplateRows: `repeat(${rows}, minmax(0, 1fr))`,
      gridAutoFlow: autoFlow?.toLowerCase(),
    },
    ...getPadding(padding),
    ...getGap(gap),
    ...getWidth(width),
    ...getHeight(height),
  };

  return (
    <div style={styles} className={className}>
      {React.Children.map(children, (child, index) => (
        <GridLayoutCell
          column={childColumn[index]}
          columnSpan={childColumnSpan[index]}
          row={childRow[index]}
          rowSpan={childRowSpan[index]}
          className={React.isValidElement(child) ? child.props.className : ""}
        >
          {child}
        </GridLayoutCell>
      ))}
    </div>
  );
};

export default GridLayoutWidget;
