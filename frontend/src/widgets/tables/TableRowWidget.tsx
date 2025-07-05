import { TableRow } from "@/components/ui/table";
import React from "react";

interface TableRowWidgetProps {
  id: string;
  isHeader?: boolean;
  isFooter?: boolean;
  children?: React.ReactNode;
}

export const TableRowWidget: React.FC<TableRowWidgetProps> = ({
  children,
  isHeader = false,
}) => (
  <TableRow className={`${isHeader ? "font-medium bg-white" : ""}`}>
    {children}
  </TableRow>
);
