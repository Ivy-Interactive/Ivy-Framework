import React, { ReactNode, useState, useRef, useEffect } from "react";
import { cn } from "@/lib/utils";
import { tableStyles } from "./styles/style";
import { DataColumn } from "./types/types";
import { ChevronDown } from "lucide-react";

const footerStyles = {
  row: {
    display: "flex",
    gap: "8px",
    alignItems: "flex-start",
  } as React.CSSProperties,
  cell: {
    flex: 1,
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
    position: "absolute",
    bottom: "100%",
    marginBottom: "2px",
    background: "var(--popover)",
    border: "1px solid var(--border)",
    borderRadius: "6px",
    boxShadow: "0 2px 8px rgba(0, 0, 0, 0.12)",
    zIndex: 50,
    minWidth: "80px",
    overflow: "hidden",
  } as React.CSSProperties,
  dropdownMenuAnchorLeft: { left: 0 } as React.CSSProperties,
  dropdownMenuAnchorRight: { right: 0 } as React.CSSProperties,
};

/**
 * Footer component that overlaps the bottom of the DataTableEditor
 * Horizontal scrollbars from the editor will appear on top of this footer
 */
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
}> = ({ values, align }) => {
  const [selectedIndex, setSelectedIndex] = useState(0);
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setOpen(false);
      }
    };
    document.addEventListener("mousedown", handler);
    return () => document.removeEventListener("mousedown", handler);
  }, [open]);

  const textAlign = align === "Right" ? "right" : align === "Center" ? "center" : "left";

  // Single value — no dropdown
  if (values.length === 1) {
    return (
      <div style={{ ...footerStyles.cell, textAlign }}>
        <div style={footerStyles.value}>{values[0]}</div>
      </div>
    );
  }

  // Multiple values — dropdown selector
  return (
    <div ref={ref} style={{ ...footerStyles.cell, textAlign, position: "relative" }}>
      <div
        className={cn(
          "inline-flex max-w-full min-w-0 items-center gap-0.5 rounded px-1 -mx-1 cursor-pointer transition-colors",
          "hover:bg-accent hover:text-accent-foreground",
          open && "bg-accent text-accent-foreground",
        )}
        onClick={() => setOpen(!open)}
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
      {open && (
        <div
          style={{
            ...footerStyles.dropdownMenu,
            ...(textAlign === "right"
              ? footerStyles.dropdownMenuAnchorRight
              : footerStyles.dropdownMenuAnchorLeft),
          }}
        >
          {values.map((value, i) => (
            <div
              key={i}
              onClick={() => {
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
        </div>
      )}
    </div>
  );
};

/**
 * Renders aggregate footer values from column footer data
 */
export interface AggregateFooterProps {
  columns: DataColumn[];
}

export const AggregateFooter: React.FC<AggregateFooterProps> = ({ columns }) => {
  const hasFooter = columns.some((col) => col.footer && col.footer.length > 0);
  if (!hasFooter) return null;

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
          return <FooterCell key={col.name} values={footerValues} align={col.align} />;
        })}
      </div>
    </DataTableFooter>
  );
};
