import React, { useMemo, useEffect, useRef, useState, useCallback } from "react";
import Markdown from "react-markdown";
import rehypeHighlight from "rehype-highlight";
import "./claude-json-renderer.css";
import type { EventHandler, ClaudeEvent, ContentBlock, AssistantEvent, ResultEvent } from "./types";
import { getWidth, getHeight } from "./styles";

function contentToString(content: unknown): string {
  if (typeof content === "string") return content;
  if (Array.isArray(content)) {
    return content
      .map((c: Record<string, unknown>) => c.text ?? c.content ?? JSON.stringify(c))
      .join("\n");
  }
  if (content && typeof content === "object") {
    const obj = content as Record<string, unknown>;
    return (obj.text ?? obj.stdout ?? obj.content ?? JSON.stringify(content)) as string;
  }
  return String(content ?? "");
}

function buildToolResultMap(events: ClaudeEvent[]): Map<string, string> {
  const map = new Map<string, string>();
  for (const event of events) {
    if (event.type !== "user") continue;
    const toolResult = event.tool_use_result as Record<string, unknown> | undefined;
    const blocks = event.message?.content ?? [];
    for (const block of blocks) {
      if (block.type === "tool_result" && block.tool_use_id) {
        const text = contentToString(block.content)
          || contentToString(toolResult?.stdout)
          || contentToString(toolResult?.content)
          || "";
        map.set(block.tool_use_id, text);
      }
    }
  }
  return map;
}

type StreamSubscriber = (streamId: string, onData: (data: unknown) => void) => () => void;

interface ClaudeJsonRendererProps {
  id: string;
  width?: string;
  height?: string;
  onIvyEvent: EventHandler;
  events?: string[];
  jsonStream?: string;
  stream?: { id: string };
  subscribeToStream?: StreamSubscriber;
  autoScroll?: boolean;
  showThinking?: boolean;
  showSystemEvents?: boolean;
}

function parseEvents(jsonStream: string | undefined): ClaudeEvent[] {
  if (!jsonStream) return [];
  const events: ClaudeEvent[] = [];
  for (const line of jsonStream.split("\n")) {
    const trimmed = line.trim();
    if (!trimmed) continue;
    try {
      events.push(JSON.parse(trimmed) as ClaudeEvent);
    } catch {
      // skip malformed lines
    }
  }
  return events;
}

function ToolUseCard({ name, input, result }: { name: string; input: Record<string, unknown>; result?: string }) {
  const [open, setOpen] = useState(false);

  let displayContent: string;
  if (name === "Bash" && typeof input.command === "string") {
    displayContent = input.command;
  } else if ((name === "Write" || name === "Edit") && typeof input.file_path === "string") {
    displayContent = `File: ${input.file_path}`;
    if (typeof input.content === "string") {
      displayContent += `\n${input.content.slice(0, 500)}${input.content.length > 500 ? "\n..." : ""}`;
    }
  } else if (name === "Read" && typeof input.file_path === "string") {
    displayContent = `File: ${input.file_path}`;
  } else {
    displayContent = JSON.stringify(input, null, 2);
  }

  const resultPreview = result != null
    ? (result.length > 120 ? result.slice(0, 120) + "..." : result)
    : null;

  return (
    <div className="tool-card my-2">
      <div className="tool-card-header" onClick={() => setOpen(!open)}>
        <span className={`chevron ${open ? "open" : ""}`}>&#9654;</span>
        <span className="font-semibold font-mono">{name}</span>
        {resultPreview != null && (
          <span className="font-mono truncate opacity-60 ml-2">{resultPreview}</span>
        )}
        {result == null && (
          <span className="opacity-40 ml-2">running...</span>
        )}
      </div>
      {open && (
        <div className="tool-card-body">
          <div className="opacity-50 text-xs mb-1">Input</div>
          <pre><code>{displayContent}</code></pre>
          {result != null && (
            <>
              <div className="opacity-50 text-xs mb-1 mt-3">Output</div>
              <pre><code>{result}</code></pre>
            </>
          )}
        </div>
      )}
    </div>
  );
}

function ResultSummary({ event }: { event: ResultEvent }) {
  const isError = event.is_error || event.subtype === "error";
  return (
    <div className={`result-card my-3 ${isError ? "error" : ""}`}>
      <div className="flex items-center gap-2 mb-2">
        <span className="font-semibold text-sm">
          {isError ? "Error" : "Completed"}
        </span>
        <span className="text-xs opacity-60">
          {event.num_turns} turn{event.num_turns !== 1 ? "s" : ""}
        </span>
      </div>
      {event.result && (
        <div className="text-sm mb-2">{event.result}</div>
      )}
      <div className="flex flex-wrap gap-4 text-xs opacity-70">
        <span>Cost: ${(event.cost_usd ?? event.total_cost_usd ?? 0).toFixed(4)}</span>
        <span>Duration: {(event.duration_ms / 1000).toFixed(1)}s</span>
        {event.usage && (
          <span>
            Tokens: {(event.usage.input_tokens ?? 0).toLocaleString()} in / {(event.usage.output_tokens ?? 0).toLocaleString()} out
          </span>
        )}
      </div>
    </div>
  );
}

function AssistantMessage({
  event,
  showThinking,
  toolResults,
}: {
  event: AssistantEvent;
  showThinking: boolean;
  toolResults: Map<string, string>;
}) {
  const content = event.message?.content;
  if (!Array.isArray(content)) return null;

  return (
    <div className="my-3">
      {content.map((block: ContentBlock, i: number) => {
        if (block.type === "text" && block.text) {
          return (
            <div key={i} className="claude-renderer">
              <Markdown rehypePlugins={[rehypeHighlight]}>{block.text}</Markdown>
            </div>
          );
        }
        if (block.type === "thinking" && block.thinking) {
          if (!showThinking) return null;
          return (
            <div key={i} className="thinking-block my-2">
              {block.thinking}
            </div>
          );
        }
        if (block.type === "tool_use" && block.name && block.input) {
          const result = block.id ? toolResults.get(block.id) : undefined;
          return <ToolUseCard key={i} name={block.name} input={block.input} result={result} />;
        }
        return null;
      })}
    </div>
  );
}

export const ClaudeJsonRenderer: React.FC<ClaudeJsonRendererProps> = ({
  id,
  width,
  height,
  onIvyEvent,
  events: enabledEvents = [],
  jsonStream,
  stream,
  subscribeToStream,
  autoScroll = true,
  showThinking = false,
  showSystemEvents = false,
}) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const [streamedLines, setStreamedLines] = useState<string[]>([]);

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
    return parts.join("\n") || undefined;
  }, [jsonStream, streamedLines]);

  const parsedEvents = useMemo(() => parseEvents(combinedStream), [combinedStream]);
  const toolResults = useMemo(() => buildToolResultMap(parsedEvents), [parsedEvents]);

  const handleComplete = useCallback(
    (resultJson: string) => {
      if (enabledEvents.includes("OnComplete")) {
        onIvyEvent("OnComplete", id, [resultJson]);
      }
    },
    [enabledEvents, onIvyEvent, id],
  );

  useEffect(() => {
    const lastEvent = parsedEvents[parsedEvents.length - 1];
    if (lastEvent && lastEvent.type === "result") {
      handleComplete(JSON.stringify(lastEvent));
    }
  }, [parsedEvents, handleComplete]);

  useEffect(() => {
    if (autoScroll && containerRef.current) {
      containerRef.current.scrollTop = containerRef.current.scrollHeight;
    }
  }, [parsedEvents, autoScroll]);

  const style: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
    overflow: "auto",
  };

  if (parsedEvents.length === 0) {
    return <div style={style} className="claude-renderer" />;
  }

  return (
    <div ref={containerRef} style={style} className="claude-renderer p-4">
      {parsedEvents.map((event, index) => {
        if (event.type === "system") {
          if (!showSystemEvents) return null;
          return (
            <div key={index} className="system-event my-1">
              System: {event.subtype}
              {event.model && ` (${event.model})`}
            </div>
          );
        }

        if (event.type === "assistant") {
          return (
            <AssistantMessage
              key={index}
              event={event}
              showThinking={showThinking}
              toolResults={toolResults}
            />
          );
        }

        // User events are now rendered inline with tool use cards -- skip them
        if (event.type === "user") {
          return null;
        }

        if (event.type === "result") {
          return <ResultSummary key={index} event={event} />;
        }

        return null;
      })}
    </div>
  );
};
