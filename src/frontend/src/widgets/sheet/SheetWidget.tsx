import { useEventHandler } from "@/components/event-handler";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
import { getHeight, getWidth } from "@/lib/styles";
import { cn } from "@/lib/utils";
import React, { useCallback, useState } from "react";
import "./sheet.css";

type SheetSide = "left" | "right" | "top" | "bottom";

const normalizeSide = (side?: string): SheetSide => {
  if (!side) return "right";
  return side.toLowerCase() as SheetSide;
};

// Helper to parse a Size string to pixels
const parseSizeToPixels = (sizeStr: string | undefined, defaultPx: number): number => {
  if (!sizeStr) return defaultPx;
  const [sizeType, value] = sizeStr.split(":");
  const numValue = parseFloat(value);
  if (isNaN(numValue)) return defaultPx;

  switch (sizeType.toLowerCase()) {
    case "px":
      return numValue;
    case "rem":
      return numValue * 16;
    case "units":
      return numValue * 4;
    default:
      return defaultPx;
  }
};

interface SheetWidgetProps {
  id: string;
  title?: string;
  description?: string;
  width?: string;
  height?: string;
  side?: SheetSide;
  resizable?: boolean;
  slots?: {
    Content?: React.ReactNode[];
  };
}

export const SheetWidget: React.FC<SheetWidgetProps> = ({
  slots,
  title,
  description,
  id,
  width,
  height,
  side = "right",
  resizable = false,
}) => {
  const eventHandler = useEventHandler();
  const [isOpen, setIsOpen] = useState(true);

  const normalizedSide = normalizeSide(side);
  const isHorizontal = normalizedSide === "left" || normalizedSide === "right";

  // Use width for horizontal sheets, height for vertical sheets
  const sizeStr = isHorizontal ? width : height;

  // Parse size parts for resize constraints
  const sizeParts = (sizeStr ?? "").split(",");
  const initialPx = parseSizeToPixels(sizeParts[0], isHorizontal ? 384 : 256);
  const minPx = parseSizeToPixels(sizeParts[1], isHorizontal ? 200 : 100);
  const maxPx = parseSizeToPixels(sizeParts[2], isHorizontal ? 1200 : 900);

  const [currentSize, setCurrentSize] = useState(initialPx);
  const [isResizing, setIsResizing] = useState(false);

  const handleClose = () => {
    setIsOpen(false);
    setTimeout(() => eventHandler("OnClose", id, []), 300);
  };

  const handleResizeStart = useCallback(
    (e: React.MouseEvent | React.TouchEvent) => {
      if (!resizable) return;

      e.preventDefault();
      e.stopPropagation();
      setIsResizing(true);

      const startPos =
        "touches" in e
          ? isHorizontal
            ? e.touches[0].clientX
            : e.touches[0].clientY
          : isHorizontal
            ? e.clientX
            : e.clientY;
      const startSize = currentSize;

      // For right/bottom sheets, dragging toward viewport edge shrinks; for left/top it grows
      const invertDelta = normalizedSide === "right" || normalizedSide === "bottom";

      const handleMove = (moveEvent: MouseEvent | TouchEvent) => {
        const clientPos =
          "touches" in moveEvent
            ? isHorizontal
              ? moveEvent.touches[0].clientX
              : moveEvent.touches[0].clientY
            : isHorizontal
              ? moveEvent.clientX
              : moveEvent.clientY;
        const delta = clientPos - startPos;
        const adjustedDelta = invertDelta ? -delta : delta;
        const newSize = Math.min(maxPx, Math.max(minPx, startSize + adjustedDelta));
        setCurrentSize(newSize);
      };

      const handleEnd = () => {
        setIsResizing(false);
        document.removeEventListener("mousemove", handleMove);
        document.removeEventListener("mouseup", handleEnd);
        document.removeEventListener("touchmove", handleMove);
        document.removeEventListener("touchend", handleEnd);
      };

      document.addEventListener("mousemove", handleMove);
      document.addEventListener("mouseup", handleEnd);
      document.addEventListener("touchmove", handleMove, { passive: true });
      document.addEventListener("touchend", handleEnd, { passive: true });
    },
    [resizable, isHorizontal, normalizedSide, currentSize, minPx, maxPx],
  );

  if (!slots?.Content) {
    return (
      <div className="text-destructive">Error: Sheet requires both Trigger and Content slots.</div>
    );
  }

  const styles: React.CSSProperties = resizable
    ? isHorizontal
      ? { width: `${currentSize}px`, maxWidth: `${maxPx}px`, minWidth: `${minPx}px` }
      : { height: `${currentSize}px`, maxHeight: `${maxPx}px`, minHeight: `${minPx}px` }
    : isHorizontal
      ? { ...getWidth(width) }
      : { ...getHeight(height) };

  // Determine which edge the resize handle sits on (inner edge of the sheet)
  const handleEdge =
    normalizedSide === "right"
      ? "left"
      : normalizedSide === "left"
        ? "right"
        : normalizedSide === "bottom"
          ? "top"
          : "bottom";

  return (
    <Sheet open={isOpen} onOpenChange={handleClose}>
      <SheetContent
        side={normalizedSide}
        style={styles}
        className={cn("flex flex-col p-0 gap-0", isHorizontal && "h-full")}
        data-sheet-side={normalizedSide}
        onOpenAutoFocus={(e) => {
          e.preventDefault();
        }}
      >
        {resizable && (
          <div
            className={cn(
              "sheet-resize-handle absolute z-10 group",
              isHorizontal ? "top-0 h-full w-1.5" : "left-0 w-full h-1.5",
              isHorizontal ? "cursor-col-resize" : "cursor-row-resize",
              handleEdge === "left" && "left-0",
              handleEdge === "right" && "right-0",
              handleEdge === "top" && "top-0",
              handleEdge === "bottom" && "bottom-0",
            )}
            onMouseDown={handleResizeStart}
            onTouchStart={handleResizeStart}
            role="separator"
            aria-orientation={isHorizontal ? "vertical" : "horizontal"}
            aria-label="Resize sheet"
            tabIndex={0}
            onKeyDown={(e) => {
              const step = 10;
              if (isHorizontal) {
                if (e.key === "ArrowLeft") {
                  setCurrentSize((prev) =>
                    normalizedSide === "right"
                      ? Math.min(maxPx, prev + step)
                      : Math.max(minPx, prev - step),
                  );
                } else if (e.key === "ArrowRight") {
                  setCurrentSize((prev) =>
                    normalizedSide === "right"
                      ? Math.max(minPx, prev - step)
                      : Math.min(maxPx, prev + step),
                  );
                }
              } else {
                if (e.key === "ArrowUp") {
                  setCurrentSize((prev) =>
                    normalizedSide === "bottom"
                      ? Math.min(maxPx, prev + step)
                      : Math.max(minPx, prev - step),
                  );
                } else if (e.key === "ArrowDown") {
                  setCurrentSize((prev) =>
                    normalizedSide === "bottom"
                      ? Math.max(minPx, prev - step)
                      : Math.min(maxPx, prev + step),
                  );
                }
              }
            }}
          >
            <div
              className={cn(
                "absolute bg-border rounded-full transition-opacity",
                "opacity-0 group-hover:opacity-100",
                isResizing && "opacity-100",
                isHorizontal
                  ? "top-1/2 -translate-y-1/2 w-1 h-8"
                  : "left-1/2 -translate-x-1/2 h-1 w-8",
                handleEdge === "left" && "left-0",
                handleEdge === "right" && "right-0",
                handleEdge === "top" && "top-0",
                handleEdge === "bottom" && "bottom-0",
              )}
            />
          </div>
        )}
        <SheetHeader className={cn("p-4 pb-0", !title && !description && "sr-only")}>
          <SheetTitle className={cn(!title && "sr-only")}>{title || "Sheet"}</SheetTitle>
          <SheetDescription className={cn(!description && "sr-only")}>
            {description || "Sheet content"}
          </SheetDescription>
        </SheetHeader>
        <div className="flex-1 pb-4 pt-1 pl-4 pr-4 mt-4 overflow-y-auto">{slots.Content}</div>
      </SheetContent>
    </Sheet>
  );
};
