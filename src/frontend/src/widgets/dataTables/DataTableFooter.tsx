import React, { ReactNode, useState, useRef, useEffect, useLayoutEffect, useCallback } from "react";
import { createPortal } from "react-dom";
import { cn } from "@/lib/utils";
import { tableStyles } from "./styles/style";
import { DataColumn } from "./types/types";
import { ChevronDown } from "lucide-react";
import type { FooterColumnLayout } from "./hooks/useFooterColumnLayout";

const footerStyles = {
  row: {
    display: "flex",
    gap: "8px",
    alignItems: "flex-start",
  } as React.CSSProperties,
  rowGrid: {
    display: "grid",
    minWidth: "min-content",
  } as React.CSSProperties,
  rowGridScroll: {
    overflowX: "auto",
    overflowY: "hidden",
    scrollbarWidth: "none",
  } as React.CSSProperties,
  cell: {
    flex: 1,
    minWidth: 0,
    fontSize: "12px",
    fontWeight: 600,
    color: "var(--foreground)",
  } as React.CSSProperties,
  cellGrid: {
    minWidth: 0,
    fontSize: "12px",
    fontWeight: 600,
    color: "var(--foreground)",
  } as React.CSSProperties,
  value: {
    lineHeight: "1.4",
    whiteSpace: "nowrap",
    overflow: "hidden",
    textOverflow: "ellipsis",
  } as React.CSSProperties,
  dropdownMenu: {
    background: "var(--popover)",
    border: "1px solid var(--border)",
    borderRadius: "6px",
    boxShadow: "0 2px 8px rgba(0, 0, 0, 0.12)",
    minWidth: "80px",
    overflow: "hidden",
  } as React.CSSProperties,
};

/** Aggregate footer row below the grid (flex layout in GridContainer — not overlaid). */
export interface DataTableFooterProps {
  children?: ReactNode;
  className?: string;
}

export const DataTableFooter: React.FC<DataTableFooterProps> = ({ children, className }) => {
  return (
    <div className={cn(className)} style={tableStyles.tableEditor.footer}>
      {children}
    </div>
  );
};

/**
 * A single footer cell that shows a dropdown when multiple values exist
 */
const FooterCell: React.FC<{
  values: string[];
  align?: string;
  cellStyle: React.CSSProperties;
  /** When footer column widths / marker change (e.g. grid column resize), menu anchor must update. */
  layoutSyncKey?: string;
}> = ({ values, align, cellStyle, layoutSyncKey = "" }) => {
  const [selectedIndex, setSelectedIndex] = useState(0);
  const [open, setOpen] = useState(false);
  const [menuPos, setMenuPos] = useState<{
    left?: number;
    right?: number;
    top: number;
    minWidth: number;
  } | null>(null);
  const triggerRef = useRef<HTMLDivElement>(null);
  const menuRef = useRef<HTMLDivElement>(null);

  const textAlign = align === "Right" ? "right" : align === "Center" ? "center" : "left";

  const updateMenuPosition = useCallback(() => {
    const el = triggerRef.current;
    if (!el) return;
    const rect = el.getBoundingClientRect();
    const minWidth = Math.max(80, rect.width);
    if (textAlign === "right") {
      setMenuPos({
        right: window.innerWidth - rect.right,
        top: rect.top,
        minWidth,
      });
    } else {
      setMenuPos({
        left: rect.left,
        top: rect.top,
        minWidth,
      });
    }
  }, [textAlign]);

  useLayoutEffect(() => {
    if (!open) {
      setMenuPos(null);
      return;
    }
    updateMenuPosition();
  }, [open, updateMenuPosition, selectedIndex, values, layoutSyncKey]);

  useEffect(() => {
    if (!open) return;
    const onScrollOrResize = () => updateMenuPosition();
    window.addEventListener("resize", onScrollOrResize);
    window.addEventListener("scroll", onScrollOrResize, true);
    return () => {
      window.removeEventListener("resize", onScrollOrResize);
      window.removeEventListener("scroll", onScrollOrResize, true);
    };
  }, [open, updateMenuPosition]);

  /** Footer strip scroll (synced to grid) moves cells without window scroll — reposition menu. */
  useEffect(() => {
    if (!open) return;
    const scrollRoot = triggerRef.current?.closest("[data-footer-scroll]");
    if (!scrollRoot) return;
    const onScroll = () => updateMenuPosition();
    scrollRoot.addEventListener("scroll", onScroll, { passive: true });
    return () => scrollRoot.removeEventListener("scroll", onScroll);
  }, [open, updateMenuPosition, layoutSyncKey]);

  useEffect(() => {
    if (!open) return;
    const handler = (e: PointerEvent) => {
      const t = e.target as Node;
      if (triggerRef.current?.contains(t) || menuRef.current?.contains(t)) return;
      setOpen(false);
    };
    document.addEventListener("pointerdown", handler);
    return () => document.removeEventListener("pointerdown", handler);
  }, [open]);

  // Single value — no dropdown
  if (values.length === 1) {
    return (
      <div style={{ ...cellStyle, textAlign }}>
        <div style={footerStyles.value}>{values[0]}</div>
      </div>
    );
  }

  const menuPortal =
    open &&
    menuPos &&
    typeof document !== "undefined" &&
    createPortal(
      <div
        ref={menuRef}
        style={{
          ...footerStyles.dropdownMenu,
          position: "fixed",
          zIndex: 10,
          top: menuPos.top,
          left: menuPos.left,
          right: menuPos.right,
          minWidth: menuPos.minWidth,
          transform: "translateY(calc(-100% - 2px))",
        }}
      >
        {values.map((value, i) => (
          <div
            key={i}
            role="option"
            aria-selected={i === selectedIndex}
            onMouseDown={(e) => {
              e.preventDefault();
              setSelectedIndex(i);
              setOpen(false);
            }}
            className={cn(
              "px-2 py-1 text-xs cursor-pointer whitespace-nowrap transition-colors",
              i === selectedIndex ? "bg-accent font-bold" : "font-normal hover:bg-accent",
            )}
          >
            {value}
          </div>
        ))}
      </div>,
      document.body,
    );

  // Multiple values — dropdown selector (menu portaled so it is not clipped by overflow:hidden)
  return (
    <div style={{ ...cellStyle, textAlign, position: "relative" }}>
      <div
        ref={triggerRef}
        className={cn(
          "inline-flex max-w-full min-w-0 items-center gap-0.5 rounded px-1 -mx-1 cursor-pointer transition-colors",
          "hover:bg-accent hover:text-accent-foreground",
          open && "bg-accent text-accent-foreground",
        )}
        role="button"
        tabIndex={0}
        onPointerDown={() => {
          // Ensure focus is on the trigger so `onBlur` can close the dropdown.
          triggerRef.current?.focus();
        }}
        onBlur={(e) => {
          const next = e.relatedTarget as Node | null;
          if (!next) {
            setOpen(false);
            return;
          }
          if (triggerRef.current?.contains(next) || menuRef.current?.contains(next)) return;
          setOpen(false);
        }}
        onKeyDown={(e) => {
          if (e.key === "Escape") setOpen(false);
          if (e.key === "Enter" || e.key === " ") {
            e.preventDefault();
            setOpen((v) => !v);
          }
        }}
        onClick={() => setOpen((v) => !v)}
      >
        <span className="min-w-0 truncate" style={footerStyles.value}>
          {values[selectedIndex]}
        </span>
        <ChevronDown
          size={10}
          className={cn(
            "shrink-0 opacity-50 transition-transform duration-150",
            open && "rotate-180",
          )}
        />
      </div>
      {menuPortal}
    </div>
  );
};

/**
 * Renders aggregate footer values from column footer data
 */
export interface AggregateFooterProps {
  columns: DataColumn[];
  layout?: FooterColumnLayout;
  footerScrollRef?: React.RefObject<HTMLDivElement | null>;
}

export const AggregateFooter: React.FC<AggregateFooterProps> = ({
  columns,
  layout,
  footerScrollRef,
}) => {
  const hasFooter = columns.some((col) => col.footer && col.footer.length > 0);
  if (!hasFooter) return null;

  const useGridLayout =
    layout != null &&
    layout.columnWidths.length === columns.length &&
    layout.columnWidths.length > 0;

  const gridTemplateColumns = useGridLayout
    ? `${layout.markerWidth}px ${layout.columnWidths.map((w) => `${w}px`).join(" ")}`
    : undefined;

  const cellStyle = useGridLayout ? footerStyles.cellGrid : footerStyles.cell;

  const layoutSyncKey =
    useGridLayout && layout ? `${layout.markerWidth}:${layout.columnWidths.join(",")}` : "";

  if (useGridLayout && gridTemplateColumns && footerScrollRef) {
    return (
      <DataTableFooter>
        <div
          ref={footerScrollRef}
          data-footer-scroll
          className="[scrollbar-width:none] [&::-webkit-scrollbar]:hidden"
          style={{ ...footerStyles.rowGridScroll }}
        >
          <div style={{ ...footerStyles.rowGrid, gridTemplateColumns }}>
            <div style={{ ...footerStyles.cellGrid, minHeight: "1em" }} aria-hidden />
            {columns.map((col) => {
              const footerValues = col.footer;
              if (!footerValues || footerValues.length === 0) {
                return (
                  <div key={col.name} style={footerStyles.cellGrid}>
                    &nbsp;
                  </div>
                );
              }
              return (
                <FooterCell
                  key={col.name}
                  values={footerValues}
                  align={col.alignContent}
                  cellStyle={cellStyle}
                  layoutSyncKey={layoutSyncKey}
                />
              );
            })}
          </div>
        </div>
      </DataTableFooter>
    );
  }

  return (
    <DataTableFooter>
      <div style={footerStyles.row}>
        {columns.map((col) => {
          const footerValues = col.footer;
          if (!footerValues || footerValues.length === 0) {
            return (
              <div key={col.name} style={footerStyles.cell}>
                &nbsp;
              </div>
            );
          }
          return (
            <FooterCell
              key={col.name}
              values={footerValues}
              align={col.alignContent}
              cellStyle={cellStyle}
              layoutSyncKey={layoutSyncKey}
            />
          );
        })}
      </div>
    </DataTableFooter>
  );
};

export type { FooterColumnLayout };
