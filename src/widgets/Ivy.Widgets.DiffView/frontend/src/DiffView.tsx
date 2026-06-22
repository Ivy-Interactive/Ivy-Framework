import React, { useMemo, useState, useEffect } from "react";
import { parseDiff, Diff, Hunk, type ChangeData } from "react-diff-view";
import "react-diff-view/style/index.css";
import "./custom-diff.css";
import type { EventHandler } from "./types";
import { getWidth, getHeight } from "./styles";

interface DiffViewProps {
  id: string;
  width?: string;
  height?: string;
  onIvyEvent: EventHandler;
  events?: string[];
  diff?: string;
  viewType?: "Unified" | "Split";
  language?: string;
  oldRevision?: string;
  newRevision?: string;
  wordWrap?: boolean;
  collapsible?: boolean;
  defaultCollapsed?: boolean;
}

function getLineNumber(change: ChangeData | null): number {
  if (!change) return 0;
  if (change.type === "normal") return change.newLineNumber;
  return change.lineNumber;
}

export function useIsMobile(): boolean {
  const [isMobile, setIsMobile] = useState(() => {
    if (typeof window === "undefined") return false;
    return window.matchMedia("(max-width: 767px)").matches;
  });

  useEffect(() => {
    if (typeof window === "undefined") return;

    const mediaQuery = window.matchMedia("(max-width: 767px)");
    const handleChange = (e: MediaQueryListEvent) => {
      setIsMobile(e.matches);
    };

    mediaQuery.addEventListener("change", handleChange);
    return () => mediaQuery.removeEventListener("change", handleChange);
  }, []);

  return isMobile;
}

export const DiffView: React.FC<DiffViewProps> = ({
  id,
  width,
  height,
  onIvyEvent,
  events = [],
  diff,
  viewType = "Unified",
  oldRevision,
  newRevision,
  wordWrap,
  collapsible = false,
  defaultCollapsed = false,
}) => {
  const files = useMemo(() => {
    if (!diff) return [];
    try {
      return parseDiff(diff);
    } catch {
      return [];
    }
  }, [diff]);

  const [collapsedState, setCollapsedState] = useState<Record<number, boolean>>({});

  const isMobile = useIsMobile();
  const diffViewType = viewType === "Split" ? "split" : "unified";
  const effectiveViewType = isMobile ? "unified" : diffViewType;
  const effectiveWordWrap = isMobile || wordWrap;

  const style: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
    overflow: "auto",
  };

  if (!diff || files.length === 0) {
    return (
      <div style={style} className="text-[var(--muted-foreground)] p-4 text-sm">
        No diff to display
      </div>
    );
  }

  return (
    <div style={style} className={`ivy-diff-view text-xs${effectiveWordWrap ? " diff-wrap" : ""}`}>
      {files.map((file, fileIndex) => {
        const rawOld = oldRevision || file.oldPath || "";
        const rawNew = newRevision || file.newPath || "";
        const oldName = rawOld === "/dev/null" ? "" : rawOld;
        const newName = rawNew === "/dev/null" ? "" : rawNew;
        const isRename = oldName !== newName && oldName !== "" && newName !== "";
        const hasHeader = oldName || newName;
        const fileId = file.newPath || file.oldPath || `diff-${fileIndex}`;

        const getBasename = (path: string) => {
          const parts = path.split("/");
          return parts[parts.length - 1] || path;
        };

        const isCollapsed = collapsible
          ? (collapsedState[fileIndex] ?? defaultCollapsed)
          : false;

        const toggleCollapsed = () => {
          if (!collapsible) return;
          setCollapsedState((prev) => ({
            ...prev,
            [fileIndex]: !isCollapsed,
          }));
        };

        return (
          <div key={fileIndex} id={fileId}>
            {hasHeader && (
              <div
                className={`flex items-center gap-2 px-3 py-1.5 text-[11px] bg-[var(--muted)] text-[var(--muted-foreground)] border-b border-[var(--border)] sticky top-0 z-10${collapsible ? " cursor-pointer select-none" : ""}`}
                style={{ fontFamily: 'var(--font-sans, sans-serif)' }}
                onClick={collapsible ? toggleCollapsed : undefined}
              >
                {collapsible && (
                  <svg
                    width="12"
                    height="12"
                    viewBox="0 0 12 12"
                    className="shrink-0 transition-transform duration-150"
                    style={{ transform: isCollapsed ? "rotate(-90deg)" : "rotate(0deg)" }}
                  >
                    <path d="M3 4.5L6 7.5L9 4.5" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
                  </svg>
                )}
                {isRename ? (
                  <>
                    <span className="font-semibold">{getBasename(oldName)}</span>
                    <span className="opacity-40">&rarr;</span>
                    <span className="font-semibold">{getBasename(newName)}</span>
                  </>
                ) : (
                  <span className="font-semibold">{getBasename(newName || oldName)}</span>
                )}
              </div>
            )}
            {!isCollapsed && (
              <Diff
                viewType={effectiveViewType}
                diffType={file.type}
                hunks={file.hunks}
                gutterEvents={{
                  onClick: ({ change }) => {
                    if (events.includes("OnLineClick")) {
                      onIvyEvent("OnLineClick", id, [getLineNumber(change)]);
                    }
                  },
                }}
              >
                {(hunks) =>
                  hunks.map((hunk) => (
                    <Hunk key={hunk.content} hunk={hunk} />
                  ))
                }
              </Diff>
            )}
          </div>
        );
      })}
    </div>
  );
};
