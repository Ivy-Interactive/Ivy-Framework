import React from "react";
import { TableRow } from "@/components/ui/table";
import { Densities } from "@/types/density";

interface TableRowWidgetProps {
  id: string;
  isHeader?: boolean;
  isFooter?: boolean;
  density?: Densities;
  children?: React.ReactNode;
}

export const TableRowWidget: React.FC<TableRowWidgetProps> & { skipWidgetWrapper?: boolean } = ({
  children,
  isHeader = false,
  id,
}) => (
  <TableRow
    className={`border border-border ${isHeader ? "font-medium bg-background" : ""}`}
    data-ivy-widget-id={id}
    data-ivy-widget-type="Ivy.TableRow"
  >
    {children}
  </TableRow>
);
TableRowWidget.skipWidgetWrapper = true;
