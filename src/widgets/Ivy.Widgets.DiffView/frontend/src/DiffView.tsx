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
        const hasHeader = oldRevision || newRevision || file.oldPath || file.newPath;
        return (
          <div key={fileIndex}>
            {hasHeader && (
              <div className="flex items-center gap-4 px-3 py-2 text-[10px] font-mono bg-[var(--muted)] text-[var(--muted-foreground)] border-b border-[var(--border)] sticky top-0 z-10">
                <div className="flex items-center gap-2">
                  <span className="opacity-50">OLD:</span>
                  <span className="font-bold">{oldRevision || file.oldPath || "none"}</span>
                </div>
                <span className="opacity-30">&rarr;</span>
                <div className="flex items-center gap-2">
                  <span className="opacity-50">NEW:</span>
                  <span className="font-bold">{newRevision || file.newPath || "none"}</span>
                </div>
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
