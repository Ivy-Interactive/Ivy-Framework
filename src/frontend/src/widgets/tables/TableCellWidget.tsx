import React from "react";
import { TableCell } from "@/components/ui/table";
import { cn } from "@/lib/utils";
import { Align, getWidth } from "@/lib/styles";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";
import { Densities } from "@/types/density";
import "./table.css";

interface TableCellWidgetProps {
  id: string;
  isHeader?: boolean;
  isFooter?: boolean;
  alignContent: Align;
  width?: string;
  multiline?: boolean;
  density?: Densities;
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

export const TableCellWidget: React.FC<TableCellWidgetProps> & { skipWidgetWrapper?: boolean } = ({
  id,
  children,
  isHeader,
  isFooter,
  alignContent,
  width,
  multiline,
  density: _density = Densities.Medium,
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

  // Check if width is a content-based sizing type (Fit, MinContent, MaxContent, Auto)
  // or undefined. Content-sized columns should size naturally and not be truncated.
  const isContentSized =
    !width ||
    width.toLowerCase().startsWith("fit") ||
    width.toLowerCase().startsWith("mincontent") ||
    width.toLowerCase().startsWith("maxcontent") ||
    width.toLowerCase().startsWith("auto");

  // Apply max-w-0 overflow-hidden for truncation when:
  // 1. We have an explicit fixed width (not content-sized), OR
  // 2. It's a header cell of a non-content-sized column
  const shouldTruncate = width ? !isContentSized : isHeader && !isContentSized;

  // Only show tooltip for string/number children to avoid "[object Object]" issues
  const shouldShowTooltip =
    !multiline && (typeof children === "string" || typeof children === "number");

  const isRightAligned =
    alignContent === "Right" || alignContent === "TopRight" || alignContent === "BottomRight";

  const isCenterAligned =
    alignContent === "Center" || alignContent === "TopCenter" || alignContent === "BottomCenter";

  const isLeftAligned =
    alignContent === "Left" || alignContent === "TopLeft" || alignContent === "BottomLeft";

  const cellClasses = cn("border border-border force-text-inherit", {
    "header-cell bg-muted font-bold": isHeader,
    "footer-cell bg-muted font-semibold": isFooter,
    "max-w-0 overflow-hidden": shouldTruncate,
    "text-right": isRightAligned,
    "text-center": isCenterAligned,
    "text-left": isLeftAligned,
  });

  return (
    <TableCell
      className={cellClasses}
      style={cellStyles}
      data-ivy-widget-id={id}
      data-ivy-widget-type="Ivy.TableCell"
    >
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
TableCellWidget.skipWidgetWrapper = true;
