import React, { useMemo } from "react";
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
}

function getLineNumber(change: ChangeData | null): number {
  if (!change) return 0;
  if (change.type === "normal") return change.newLineNumber;
  return change.lineNumber;
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
}) => {
  const files = useMemo(() => {
    if (!diff) return [];
    try {
      return parseDiff(diff);
    } catch {
      return [];
    }
  }, [diff]);

  const diffViewType = viewType === "Split" ? "split" : "unified";

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
    <div style={style} className={`ivy-diff-view text-xs${wordWrap ? " diff-wrap" : ""}`}>
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

        return (
          <div key={fileIndex} id={fileId}>
            {hasHeader && (
              <div className="flex items-center gap-2 px-3 py-1.5 text-[11px] bg-[var(--muted)] text-[var(--muted-foreground)] border-b border-[var(--border)] sticky top-0 z-10" style={{ fontFamily: 'var(--font-sans, sans-serif)' }}>
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
            <Diff
              viewType={diffViewType}
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
          </div>
        );
      })}
    </div>
  );
};
