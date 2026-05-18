import React from "react";
import Markdown from "react-markdown";
import rehypeHighlight from "rehype-highlight";
import remarkGfm from "remark-gfm";
import type { NormalizedEvent } from "./types";

interface ResultSummaryProps {
  event: Extract<NormalizedEvent, { kind: "result" }>;
}

export const ResultSummary: React.FC<ResultSummaryProps> = ({ event }) => {
  const isError = !event.success;
  return (
    <div className={`aov-result ${isError ? "error" : ""}`}>
      <div className="aov-result-header">
        <span className="aov-result-title">{isError ? "✗ Error" : "✓ Completed"}</span>
        {event.numTurns != null && (
          <span className="aov-result-meta">
            {event.numTurns} turn{event.numTurns !== 1 ? "s" : ""}
          </span>
        )}
      </div>
      {event.text && (
        <div className="aov-markdown aov-result-body">
          <Markdown remarkPlugins={[remarkGfm]} rehypePlugins={[rehypeHighlight]}>
            {event.text}
          </Markdown>
        </div>
      )}
      <div className="aov-result-stats">
        {event.costUsd != null && <span>Cost: ${event.costUsd.toFixed(4)}</span>}
        {event.durationMs != null && (
          <span>Duration: {(event.durationMs / 1000).toFixed(1)}s</span>
        )}
        {(event.tokensIn != null || event.tokensOut != null) && (
          <span>
            Tokens: {(event.tokensIn ?? 0).toLocaleString()} in /{" "}
            {(event.tokensOut ?? 0).toLocaleString()} out
          </span>
        )}
      </div>
    </div>
  );
};
