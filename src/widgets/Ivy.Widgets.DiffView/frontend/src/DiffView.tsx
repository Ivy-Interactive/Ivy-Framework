import React, { useMemo, useState, useEffect, useRef, useCallback } from "react";
import { parseDiff, Diff, Hunk, type ChangeData } from "react-diff-view";
import "react-diff-view/style/index.css";
import "./custom-diff.css";
import type { EventHandler } from "./types";
import { getWidth, getHeight } from "./styles";

/** Container width (px) below which the diff is too cramped for a side-by-side (split) view. */
export const NARROW_BREAKPOINT = 768;

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

function getBasename(path: string): string {
  const parts = path.split("/");
  return parts[parts.length - 1] || path;
}

/**
 * Tracks whether a container is narrower than {@link NARROW_BREAKPOINT}, measured
 * against the element's own width (via ResizeObserver) rather than the viewport.
 *
 * This matters because the DiffView is frequently embedded in a panel that is
 * narrower than the browser window, so a viewport-level media query would report
 * "wide" and try to render a side-by-side split that has no room to fit. Measuring
 * the container keeps the inline (unified) fallback in sync with the space the
 * widget actually has.
 *
 * Returns a ref to attach to the container and the current narrow state.
 */
export function useIsNarrow(): [React.RefObject<HTMLDivElement | null>, boolean] {
  const ref = useRef<HTMLDivElement | null>(null);
  const [isNarrow, setIsNarrow] = useState(false);

  useEffect(() => {
    const element = ref.current;
    if (!element || typeof ResizeObserver === "undefined") return;

    let animFrameId: number | null = null;

    const update = (width: number) => {
      const next = width > 0 && width < NARROW_BREAKPOINT;
      setIsNarrow((prev) => (prev === next ? prev : next));
    };

    update(element.clientWidth);

    const observer = new ResizeObserver((entries) => {
      if (entries.length === 0) return;
      const width = entries[0].contentRect.width;
      if (animFrameId !== null) {
        cancelAnimationFrame(animFrameId);
      }
      animFrameId = requestAnimationFrame(() => {
        update(width);
      });
    });

    observer.observe(element);
    return () => {
      if (animFrameId !== null) {
        cancelAnimationFrame(animFrameId);
      }
      observer.disconnect();
    };
  }, []);

  return [ref, isNarrow];
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

  const [containerRef, isNarrow] = useIsNarrow();
  const diffViewType = viewType === "Split" ? "split" : "unified";
  const effectiveViewType = isNarrow ? "unified" : diffViewType;
  const effectiveWordWrap = isNarrow || wordWrap;

  // Per-file display metadata, derived once so both the navigation dropdown and
  // the file list render from the same source of truth.
  const fileMeta = useMemo(() => {
    return files.map((file, fileIndex) => {
      const rawOld = oldRevision || file.oldPath || "";
      const rawNew = newRevision || file.newPath || "";
      const oldName = rawOld === "/dev/null" ? "" : rawOld;
      const newName = rawNew === "/dev/null" ? "" : rawNew;
      const isRename = oldName !== newName && oldName !== "" && newName !== "";
      const hasHeader = Boolean(oldName || newName);
      const elementId = `${id}-${file.newPath || file.oldPath || `diff-${fileIndex}`}`;
      const label = isRename
        ? `${getBasename(oldName)} → ${getBasename(newName)}`
        : getBasename(newName || oldName) || `Diff ${fileIndex + 1}`;

      return { oldName, newName, isRename, hasHeader, elementId, label };
    });
  }, [files, id, oldRevision, newRevision]);

  const scrollToFile = useCallback((elementId: string) => {
    if (typeof document === "undefined") return;
    document
      .getElementById(elementId)
      ?.scrollIntoView({ behavior: "smooth", block: "start" });
  }, []);

  const style: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
    ...(height ? { overflow: "auto" } : {}),
  };

  if (!diff || files.length === 0) {
    return (
      <div ref={containerRef} style={style} className="text-[var(--muted-foreground)] p-4 text-sm">
        No diff to display
      </div>
    );
  }

  // On a narrow container, the stacked file headers crowd the limited space, so
  // collapse the file list into a dropdown that jumps to the chosen file.
  const showFileDropdown = isNarrow && fileMeta.length > 1;

  return (
    <div ref={containerRef} style={style} className={`ivy-diff-view text-xs${effectiveWordWrap ? " diff-wrap" : ""}`}>
      {showFileDropdown && (
        <div
          className="sticky top-0 z-20 flex items-center gap-2 px-3 py-1.5 bg-[var(--muted)] border-b border-[var(--border)]"
          style={{ fontFamily: 'var(--font-sans, sans-serif)' }}
        >
          <span className="text-[11px] text-[var(--muted-foreground)] shrink-0">
            {fileMeta.length} files
          </span>
          <select
            aria-label="Jump to file"
            className="flex-1 min-w-0 text-[11px] px-2 py-1 rounded bg-[var(--background)] text-[var(--foreground)] border border-[var(--border)]"
            style={{ fontFamily: 'var(--font-sans, sans-serif)' }}
            defaultValue=""
            onChange={(e) => {
              if (e.target.value) scrollToFile(e.target.value);
            }}
          >
            <option value="" disabled>
              Jump to file…
            </option>
            {fileMeta.map((meta, fileIndex) => (
              <option key={fileIndex} value={meta.elementId}>
                {meta.label}
              </option>
            ))}
          </select>
        </div>
      )}
      {files.map((file, fileIndex) => {
        const { oldName, newName, isRename, hasHeader, elementId } = fileMeta[fileIndex];

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
          // scrollMarginTop keeps the jump-to-file target clear of the sticky dropdown.
          <div key={fileIndex} id={elementId} style={{ scrollMarginTop: showFileDropdown ? "2rem" : 0 }}>
            {hasHeader && (
              <div
                className={`relative flex items-center gap-2 px-3 py-1.5 text-[11px] bg-[var(--muted)] text-[var(--muted-foreground)] border-b border-[var(--border)] sticky top-0 z-10 rounded-t-md before:absolute before:-top-px before:inset-x-0 before:h-2 before:bg-[var(--muted)] before:rounded-t-md${collapsible ? " cursor-pointer select-none" : ""}`}
                style={{
                  fontFamily: 'var(--font-sans, sans-serif)',
                  // Sit below the file dropdown when it is shown, otherwise at the top.
                  top: showFileDropdown ? "2rem" : 0,
                }}
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
