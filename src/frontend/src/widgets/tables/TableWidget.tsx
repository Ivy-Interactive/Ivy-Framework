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
  const resolvedWidth = width || "Full";
  const widthStyles = getWidth(resolvedWidth);
  const isFitContent = resolvedWidth.toLowerCase().startsWith("fit");
  const widthClass = isFitContent ? "w-fit" : "w-full";

  return (
    <div style={widthStyles} className={widthClass}>
      <Table
        density={density}
        className={cn(widthClass, "caption-bottom border-collapse border border-border")}
        style={{
          tableLayout: layout.toLowerCase() === "fixed" ? "fixed" : "auto",
        }}
      >
        <TableBody>{children}</TableBody>
      </Table>
    </div>
  );
};
