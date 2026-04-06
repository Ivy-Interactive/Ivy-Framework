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
import React, { useCallback, useRef, useState } from "react";
import "./sheet.css";

type SheetSide = "left" | "right" | "top" | "bottom";

const normalizeSide = (side?: string): SheetSide => {
  if (!side) return "right";
  return side.toLowerCase() as SheetSide;
};

// Helper to parse an Ivy Size token to pixels for resize math.
const parseSizeToPixels = (
  sizeStr: string | undefined,
  defaultPx: number,
  axis: "width" | "height",
): number => {
  if (!sizeStr) return defaultPx;
  const [sizeType, value] = sizeStr.split(":");
  const normalizedType = sizeType.toLowerCase();
  const numValue = parseFloat(value);
  const viewportPx =
    typeof window === "undefined"
      ? defaultPx
      : axis === "width"
        ? window.innerWidth || document.documentElement.clientWidth || defaultPx
        : window.innerHeight || document.documentElement.clientHeight || defaultPx;

  switch (normalizedType) {
    case "px":
      return isNaN(numValue) ? defaultPx : numValue;
    case "rem":
      return isNaN(numValue) ? defaultPx : numValue * 16;
    case "units":
      return isNaN(numValue) ? defaultPx : numValue * 4;
    case "fraction":
      return isNaN(numValue) ? defaultPx : numValue * viewportPx;
    case "full":
    case "screen":
      return viewportPx;
    case "fit":
    case "auto":
    case "mincontent":
    case "maxcontent":
      return defaultPx;
    default:
      return defaultPx;
  }
};

/** SignalR + MessagePack may send bool as number; props are Record<string, unknown>. */
const coerceResizable = (value: unknown): boolean => {
  if (value === true || value === 1) return true;
  if (value === false || value === 0 || value == null) return false;
  if (typeof value === "string") {
    const v = value.trim().toLowerCase();
    if (v === "true" || v === "1") return true;
    if (v === "false" || v === "0" || v === "") return false;
  }
  return false;
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
  resizable: resizableProp = false,
}) => {
  const eventHandler = useEventHandler();
  const [isOpen, setIsOpen] = useState(true);
  const isResizingRef = useRef(false);
  const canResize = coerceResizable(resizableProp);

  const normalizedSide = normalizeSide(side);
  const isHorizontal = normalizedSide === "left" || normalizedSide === "right";

  // Use width for horizontal sheets, height for vertical sheets
  const sizeStr = isHorizontal ? width : height;
  const axis: "width" | "height" = isHorizontal ? "width" : "height";

  // Parse size parts for resize constraints
  const sizeParts = (sizeStr ?? "").split(",");
  const initialPx = parseSizeToPixels(sizeParts[0], isHorizontal ? 384 : 256, axis);
  const minPx = parseSizeToPixels(sizeParts[1], isHorizontal ? 200 : 100, axis);
  const maxPx = parseSizeToPixels(sizeParts[2], isHorizontal ? 1200 : 900, axis);

  const [currentSize, setCurrentSize] = useState(initialPx);

  const handleClose = () => {
    setIsOpen(false);
    setTimeout(() => eventHandler("OnClose", id, []), 300);
  };

  const handleResizePointerDown = useCallback(
    (e: React.PointerEvent<HTMLDivElement>) => {
      if (!canResize || e.button !== 0) return;

      e.preventDefault();
      e.stopPropagation();
      isResizingRef.current = true;

      const el = e.currentTarget;
      el.setPointerCapture(e.pointerId);

      const startPos = isHorizontal ? e.clientX : e.clientY;
      const startSize = currentSize;
      const invertDelta = normalizedSide === "right" || normalizedSide === "bottom";

      const handleMove = (moveEvent: PointerEvent) => {
        moveEvent.preventDefault();
        const clientPos = isHorizontal ? moveEvent.clientX : moveEvent.clientY;
        const delta = clientPos - startPos;
        const adjustedDelta = invertDelta ? -delta : delta;
        const newSize = Math.min(maxPx, Math.max(minPx, startSize + adjustedDelta));
        setCurrentSize(newSize);
      };

      const cleanup = (endEvent: PointerEvent) => {
        try {
          if (el.hasPointerCapture(endEvent.pointerId)) {
            el.releasePointerCapture(endEvent.pointerId);
          }
        } catch {
          /* ignore */
        }
        isResizingRef.current = false;
        el.removeEventListener("pointermove", handleMove);
        el.removeEventListener("pointerup", cleanup);
        el.removeEventListener("pointercancel", cleanup);
      };

      el.addEventListener("pointermove", handleMove, { passive: false });
      el.addEventListener("pointerup", cleanup);
      el.addEventListener("pointercancel", cleanup);
    },
    [canResize, isHorizontal, normalizedSide, currentSize, minPx, maxPx],
  );

  if (!slots?.Content) {
    return (
      <div className="text-destructive">Error: Sheet requires both Trigger and Content slots.</div>
    );
  }

  const styles: React.CSSProperties = canResize
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
    <Sheet open={isOpen} onOpenChange={handleClose} modal={!canResize}>
      <SheetContent
        side={normalizedSide}
        style={styles}
        className={cn(
          "flex flex-col p-0 gap-0",
          isHorizontal
            ? canResize
              ? "h-full min-w-0 sm:max-w-none"
              : "h-full w-auto sm:max-w-none"
            : "max-h-none",
        )}
        data-sheet-side={normalizedSide}
        onInteractOutside={(e: Event) => {
          if (isResizingRef.current) e.preventDefault();
        }}
        onPointerDownOutside={(e: Event) => {
          if (isResizingRef.current) e.preventDefault();
        }}
        onOpenAutoFocus={(e: Event) => {
          const container = e.currentTarget as HTMLElement | null;
          const target = container?.querySelector<HTMLElement>("[autofocus]");
          if (target) {
            e.preventDefault();
            target.focus();
          } else {
            e.preventDefault();
          }
        }}
      >
        {canResize && (
          <div
            className={cn(
              "sheet-resize-handle absolute z-10 touch-none select-none pointer-events-auto",
              isHorizontal
                ? "top-0 h-full w-1.5 cursor-col-resize"
                : "left-0 w-full h-1.5 cursor-row-resize",
              handleEdge === "left" && "left-0",
              handleEdge === "right" && "right-0",
              handleEdge === "top" && "top-0",
              handleEdge === "bottom" && "bottom-0",
            )}
            onPointerDown={handleResizePointerDown}
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
          />
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
