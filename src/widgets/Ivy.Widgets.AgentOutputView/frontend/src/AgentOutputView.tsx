import React, { useCallback, useEffect, useMemo, useState } from "react";
import Markdown from "react-markdown";
import rehypeHighlight from "rehype-highlight";
import remarkGfm from "remark-gfm";
import "./agent-output-view.css";
import type { EventHandler, NormalizedEvent } from "./types";
import { getHeight, getWidth } from "./styles";
import { useAutoScroll } from "./use-auto-scroll";
import { pickRenderer } from "./renderers/registry";
import { deriveStatus } from "./renderers/status";
import { AnimatedStatus } from "./animated-status";
import { ToolUseCard } from "./tool-use-card";
import { ResultSummary } from "./result-summary";

type StreamSubscriber = (streamId: string, onData: (data: unknown) => void) => () => void;

interface AgentOutputViewProps {
  id: string;
  width?: string;
  height?: string;
  onIvyEvent: EventHandler;
  events?: string[];
  provider?: string;
  jsonStream?: string;
  stream?: { id: string };
  subscribeToStream?: StreamSubscriber;
  autoScroll?: boolean;
  showThinking?: boolean;
  showSystemEvents?: boolean;
  showStatusLabel?: boolean;
  statusLabelOverride?: string;
  resetToken?: number;
}

export const AgentOutputView: React.FC<AgentOutputViewProps> = ({
  id,
  width,
  height,
  onIvyEvent,
  events: enabledEvents = [],
  provider = "claude",
  jsonStream,
  stream,
  subscribeToStream,
  autoScroll = true,
  showThinking = false,
  showSystemEvents = false,
  showStatusLabel = true,
  statusLabelOverride,
  resetToken = 0,
}) => {
  const [streamedLines, setStreamedLines] = useState<string[]>([]);

  useEffect(() => {
    setStreamedLines([]);
  }, [resetToken]);

  useEffect(() => {
    if (!stream?.id || !subscribeToStream) return;
    const unsubscribe = subscribeToStream(stream.id, (data) => {
      if (typeof data === "string") {
        setStreamedLines((prev) => [...prev, data]);
      }
    });
    return unsubscribe;
  }, [stream?.id, subscribeToStream]);

  const combinedStream = useMemo(() => {
    const parts: string[] = [];
    if (jsonStream) parts.push(jsonStream);
    if (streamedLines.length > 0) parts.push(streamedLines.join("\n"));
    return parts.join("\n");
  }, [jsonStream, streamedLines]);

  const renderer = useMemo(() => pickRenderer(provider), [provider]);
  const parsedEvents = useMemo<NormalizedEvent[]>(
    () => renderer.parseEvents(combinedStream),
    [renderer, combinedStream],
  );

  const derived = useMemo(() => deriveStatus(parsedEvents), [parsedEvents]);
  const statusText = statusLabelOverride ?? derived.text;
  const isComplete = derived.complete;

  const { scrollRef, disableAutoScroll } = useAutoScroll({
    content: parsedEvents,
    enabled: autoScroll,
    smooth: false,
  });

  const handleComplete = useCallback(
    (resultJson: string) => {
      if (enabledEvents.includes("OnComplete")) {
        onIvyEvent("OnComplete", id, [resultJson]);
      }
    },
    [enabledEvents, onIvyEvent, id],
  );

  useEffect(() => {
    const last = parsedEvents[parsedEvents.length - 1];
    if (last && last.kind === "result") {
      handleComplete(JSON.stringify(last));
    }
  }, [parsedEvents, handleComplete]);

  const shellStyle: React.CSSProperties = {
    boxSizing: "border-box",
    minWidth: 0,
    display: "flex",
    flexDirection: "column",
    overflow: "hidden",
    ...getWidth(width),
    ...getHeight(height),
  };

  // Suppress the final assistant-text when the next event is a result whose text matches
  // (avoids duplicate rendering of the final answer).
  const suppressIndices = new Set<number>();
  for (let i = 0; i < parsedEvents.length - 1; i++) {
    const cur = parsedEvents[i];
    const next = parsedEvents[i + 1];
    if (cur.kind === "assistant-text" && next.kind === "result" && next.text?.trim() === cur.text.trim()) {
      suppressIndices.add(i);
    }
  }

  return (
    <div style={shellStyle} className="aov-shell">
      <div
        ref={scrollRef}
        className="aov-body"
        onWheel={autoScroll ? disableAutoScroll : undefined}
        onTouchMove={autoScroll ? disableAutoScroll : undefined}
      >
        {parsedEvents.map((event, idx) => {
          if (suppressIndices.has(idx)) return null;
          switch (event.kind) {
            case "system":
              if (!showSystemEvents) return null;
              return (
                <div key={idx} className="aov-system">
                  system: {event.subtype}
                  {event.model ? ` (${event.model})` : ""}
                </div>
              );
            case "thinking":
              if (!showThinking) return null;
              return (
                <div key={idx} className="aov-thinking">
                  {event.text}
                </div>
              );
            case "assistant-text":
              return (
                <div key={idx} className="aov-markdown aov-assistant">
                  <Markdown remarkPlugins={[remarkGfm]} rehypePlugins={[rehypeHighlight]}>
                    {event.text}
                  </Markdown>
                </div>
              );
            case "tool-use":
              return (
                <ToolUseCard
                  key={idx}
                  name={event.name}
                  input={event.input}
                  result={event.result}
                />
              );
            case "result":
              return <ResultSummary key={idx} event={event} />;
            default:
              return null;
          }
        })}
      </div>
      {showStatusLabel && (
        <div className="aov-status-row">
          <AnimatedStatus statusText={statusText} isComplete={isComplete} />
        </div>
      )}
    </div>
  );
};
