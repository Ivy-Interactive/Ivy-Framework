import React from "react";
import { TableCell } from "@/components/ui/table";
import { cn } from "@/lib/utils";
import { Align, getWidth } from "@/lib/styles";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";
import "./table.css";

interface TableCellWidgetProps {
  id: string;
  isHeader?: boolean;
  isFooter?: boolean;
  alignContent: Align;
  width?: string;
  multiline?: boolean;
  children?: React.ReactNode;
}

// Convert Align enum to text-align CSS property for table cells
const getTextAlign = (align: Align): React.CSSProperties => {
  // Extract horizontal alignment from the Align enum
  switch (align) {
    case "TopLeft":
    case "Left":
    case "BottomLeft":
      return { textAlign: "left" };
    case "TopRight":
    case "Right":
    case "BottomRight":
      return { textAlign: "right" };
    case "TopCenter":
    case "Center":
    case "BottomCenter":
      return { textAlign: "center" };
    default:
      return { textAlign: "left" };
  }
};

export const TableCellWidget: React.FC<TableCellWidgetProps> = ({
  children,
  isHeader,
  isFooter,
  alignContent,
  width,
  multiline,
}) => {
  const cellStyles = {
    ...getWidth(width),
  };

  const textAlignStyle = getTextAlign(alignContent);

  const content = (
    <div
      className={cn(
        "align-middle force-text-inherit",
        multiline && "whitespace-normal wrap-break-word",
        !multiline && "min-w-0",
      )}
      style={textAlignStyle}
    >
      {!multiline ? (
        <div
          className="overflow-hidden text-ellipsis whitespace-nowrap max-w-full"
          style={textAlignStyle}
        >
          {children}
        </div>
      ) : (
        children
      )}
    </div>
  );

  // Apply max-w-0 overflow-hidden for truncation when:
  // 1. We have an explicit width (for column width control), OR
  // 2. It's a header cell (headers should truncate)
  // Don't apply to data cells without widths - they need to size naturally
  const shouldTruncate = width || isHeader;

  // Only show tooltip for string/number children to avoid "[object Object]" issues
  const shouldShowTooltip =
    !multiline && (typeof children === "string" || typeof children === "number");

  const cellClasses = cn("border border-border force-text-inherit", {
    "header-cell bg-muted font-bold": isHeader,
    "footer-cell bg-muted font-semibold": isFooter,
    "max-w-0 overflow-hidden": shouldTruncate,
  });

  return (
    <TableCell className={cellClasses} style={cellStyles}>
      {shouldShowTooltip ? (
        <TooltipProvider>
          <Tooltip>
            <TooltipTrigger asChild>{content}</TooltipTrigger>
            <TooltipContent className="bg-popover text-popover-foreground shadow-md">
              <div className="whitespace-pre-wrap">{children}</div>
            </TooltipContent>
          </Tooltip>
        </TooltipProvider>
      ) : (
        content
      )}
    </TableCell>
  );
};
