import React from "react";
import { Table, TableBody } from "@/components/ui/table";
import { getWidth } from "@/lib/styles";
import { Densities } from "@/types/density";
import { cn } from "@/lib/utils";

interface TableWidgetProps {
  id: string;
  children?: React.ReactNode;
  width?: string;
  density?: Densities;
  layout?: string;
}

export const TableWidget: React.FC<TableWidgetProps> = ({
  children,
  width,
  density = Densities.Medium,
  layout = "Auto",
}) => {
  const widthStyles = getWidth(width || "Full");

  return (
    <div style={widthStyles} className="w-full">
      <Table
        density={density}
        className={cn("w-full caption-bottom border-collapse border border-border")}
        style={{
          tableLayout: layout.toLowerCase() === "fixed" ? "fixed" : "auto",
        }}
      >
        <TableBody>{children}</TableBody>
      </Table>
    </div>
  );
};
