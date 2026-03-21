import React from "react";
import type { VariantProps } from "class-variance-authority";

import { cn } from "@/lib/utils";
import {
  tableCellSizeVariant,
  tableHeadSizeVariant,
  tableSizeVariant,
} from "./table/table-variant";
import { TableProvider } from "./table/TableContext";
import { useTableScale } from "./table/useTableSize";
import { Densities } from "@/types/density";

export interface TableProps
  extends
    Omit<React.HTMLAttributes<HTMLTableElement>, "size">,
    VariantProps<typeof tableSizeVariant> {}

const Table = React.forwardRef<HTMLTableElement, TableProps>(
  ({ className, density = Densities.Medium, children, ...props }, ref) => (
    <TableProvider density={density as Densities}>
      <div className="relative w-full overflow-auto">
        <table
          ref={ref}
          className={cn("w-full caption-bottom", tableSizeVariant({ density }), className)}
          {...props}
        >
          {children}
        </table>
      </div>
    </TableProvider>
  ),
);
Table.displayName = "Table";

const TableHeader = React.forwardRef<
  HTMLTableSectionElement,
  React.HTMLAttributes<HTMLTableSectionElement>
>(({ className, ...props }, ref) => (
  <thead ref={ref} className={cn("[&_tr]:border-b", className)} {...props} />
));
TableHeader.displayName = "TableHeader";

const TableBody = React.forwardRef<
  HTMLTableSectionElement,
  React.HTMLAttributes<HTMLTableSectionElement>
>(({ className, ...props }, ref) => (
  <tbody ref={ref} className={cn("[&_tr:last-child]:border-0", className)} {...props} />
));
TableBody.displayName = "TableBody";

const TableFooter = React.forwardRef<
  HTMLTableSectionElement,
  React.HTMLAttributes<HTMLTableSectionElement>
>(({ className, ...props }, ref) => (
  <tfoot
    ref={ref}
    className={cn("border-t bg-muted/50 font-medium [&>tr]:last:border-b-0", className)}
    {...props}
  />
));
TableFooter.displayName = "TableFooter";

const TableRow = React.forwardRef<HTMLTableRowElement, React.HTMLAttributes<HTMLTableRowElement>>(
  ({ className, ...props }, ref) => (
    <tr
      ref={ref}
      className={cn(
        "border-b transition-colors hover:bg-muted/50 data-[state=selected]:bg-muted",
        className,
      )}
      {...props}
    />
  ),
);
TableRow.displayName = "TableRow";

export interface TableHeadProps
  extends
    Omit<React.ThHTMLAttributes<HTMLTableCellElement>, "size">,
    VariantProps<typeof tableHeadSizeVariant> {}

const TableHead = React.forwardRef<HTMLTableCellElement, TableHeadProps>(
  ({ className, density: propScale, ...props }, ref) => {
    const contextScale = useTableScale();
    const density = propScale ?? contextScale;

    return (
      <th
        ref={ref}
        className={cn(
          tableHeadSizeVariant({ density }),
          "text-left align-middle font-medium text-muted-foreground [&:has([role=checkbox])]:pr-0 [&>[role=checkbox]]:translate-y-[2px]",
          className,
        )}
        {...props}
      />
    );
  },
);
TableHead.displayName = "TableHead";

export interface TableCellProps
  extends
    Omit<React.TdHTMLAttributes<HTMLTableCellElement>, "size">,
    VariantProps<typeof tableCellSizeVariant> {}

const TableCell = React.forwardRef<HTMLTableCellElement, TableCellProps>(
  ({ className, density: propScale, ...props }, ref) => {
    const contextScale = useTableScale();
    const density = propScale ?? contextScale;

    return (
      <td
        ref={ref}
        className={cn(
          tableCellSizeVariant({ density }),
          "align-middle [&:has([role=checkbox])]:pr-0 [&>[role=checkbox]]:translate-y-[2px]",
          className,
        )}
        {...props}
      />
    );
  },
);
TableCell.displayName = "TableCell";

const TableCaption = React.forwardRef<
  HTMLTableCaptionElement,
  React.HTMLAttributes<HTMLTableCaptionElement>
>(({ className, ...props }, ref) => (
  <caption ref={ref} className={cn("mt-4 text-sm text-muted-foreground", className)} {...props} />
));
TableCaption.displayName = "TableCaption";

export { Table, TableHeader, TableBody, TableFooter, TableHead, TableRow, TableCell, TableCaption };
