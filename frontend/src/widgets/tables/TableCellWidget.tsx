import { TableCell } from "@/components/ui/table";
import { Align, getAlign, getWidth } from "@/lib/styles";
import { cn } from "@/lib/utils";
import React from "react";

interface TableCellWidgetProps {
  id: string;
  isHeader?: boolean;
  isFooter?: boolean;
  align: Align;
  width?: string;
  children?: React.ReactNode;
}

export const TableCellWidget: React.FC<TableCellWidgetProps> = ({
  children,
  isHeader,
  isFooter,
  align,
  width,
}) => {
  const alignStyles = {
    ...getAlign("Horizontal", align),
    ...getAlign("Vertical", align),
  };

  const cellStyles = {
    ...getWidth(width),
  };

  return (
    <TableCell
      className={cn(
        isHeader && "header-cell bg-gray-100 font-semibold",
        isFooter && "footer-cell bg-gray-100 font-semibold",
        "text-nowrap",
        "border",
        "max-w-[400px] overflow-ellipsis overflow-clip"
      )}
      style={cellStyles}
    >
      <div style={alignStyles}>{children}</div>
    </TableCell>
  );
};
