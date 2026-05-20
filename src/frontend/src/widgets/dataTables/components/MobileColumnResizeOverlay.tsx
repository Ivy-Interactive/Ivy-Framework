import React from "react";
import { GripVertical } from "lucide-react";
import type { ColumnResizeHandle } from "../hooks/useMobileColumnResize";

interface MobileColumnResizeOverlayProps {
  handles: ColumnResizeHandle[];
  onHandlePointerDown: (colIndex: number, e: React.PointerEvent<HTMLDivElement>) => void;
}

/**
 * Grip on the active column header right edge for touch resize (Google Sheets–style on phones).
 */
export const MobileColumnResizeOverlay: React.FC<MobileColumnResizeOverlayProps> = ({
  handles,
  onHandlePointerDown,
}) => {
  const handle = handles[0];
  if (!handle) return null;

  return (
    <div
      role="separator"
      aria-orientation="vertical"
      aria-label={`Resize ${handle.column.title ?? "column"}`}
      className="ivy-datatable-column-resize-handle ivy-datatable-column-resize-handle--active fixed z-30 flex items-center justify-center touch-none select-none"
      style={{
        left: handle.x,
        top: handle.y,
        width: handle.width,
        height: handle.height,
      }}
      onPointerDown={(e) => onHandlePointerDown(handle.colIndex, e)}
    >
      <span
        className="flex h-5 w-3.5 items-center justify-center rounded-sm border border-primary bg-background/95 text-primary shadow-sm"
        aria-hidden
      >
        <GripVertical className="h-3 w-3" strokeWidth={2} />
      </span>
    </div>
  );
};
