import React, { useMemo, useEffect, useRef, useState, useCallback } from "react";
import Markdown from "react-markdown";
import rehypeHighlight from "rehype-highlight";
import "./claude-json-renderer.css";
import type { EventHandler, ClaudeEvent, ContentBlock, AssistantEvent, UserEvent, ResultEvent } from "./types";
import { getWidth, getHeight } from "./styles";

interface ClaudeJsonRendererProps {
  id: string;
  width?: string;
  height?: string;
  onIvyEvent: EventHandler;
  events?: string[];
  jsonStream?: string;
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

function ToolUseCard({ name, input }: { name: string; input: Record<string, unknown> }) {
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

  return (
    <div className="tool-card my-2">
      <div className="tool-card-header" onClick={() => setOpen(!open)}>
        <span className={`chevron ${open ? "open" : ""}`}>&#9654;</span>
        <span className="opacity-50">Tool:</span>
        <span className="font-semibold font-mono">{name}</span>
      </div>
      {open && (
        <div className="tool-card-body">
          <pre><code>{displayContent}</code></pre>
        </div>
      )}
    </div>
  );
}

function ToolResultCard({ toolName, content }: { toolName?: string; content: string }) {
  const [open, setOpen] = useState(false);
  const preview = content.length > 120 ? content.slice(0, 120) + "..." : content;

  return (
    <div className="tool-card my-2">
      <div className="tool-card-header" onClick={() => setOpen(!open)}>
        <span className={`chevron ${open ? "open" : ""}`}>&#9654;</span>
        <span className="opacity-50">Result{toolName ? ` (${toolName})` : ""}:</span>
        <span className="font-mono truncate">{preview}</span>
      </div>
      {open && (
        <div className="tool-card-body">
          <pre><code>{content}</code></pre>
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
        <span>Cost: ${event.cost_usd.toFixed(4)}</span>
        <span>Duration: {(event.duration_ms / 1000).toFixed(1)}s</span>
        {event.usage && (
          <span>
            Tokens: {event.usage.input_tokens.toLocaleString()} in / {event.usage.output_tokens.toLocaleString()} out
          </span>
        )}
      </div>
    </div>
  );
}

function AssistantMessage({
  event,
  showThinking,
  toolNames,
}: {
  event: AssistantEvent;
  showThinking: boolean;
  toolNames: Map<string, string>;
}) {
  const content = event.message.content;

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
          if (block.id) {
            toolNames.set(block.id, block.name);
          }
          return <ToolUseCard key={i} name={block.name} input={block.input} />;
        }
        return null;
      })}
    </div>
  );
}

function UserMessage({
  event,
  toolNames,
}: {
  event: UserEvent;
  toolNames: Map<string, string>;
}) {
  const resultContent = event.tool_use_result?.content;
  const toolUseId = event.tool_use_result?.tool_use_id;
  const toolName = toolUseId ? toolNames.get(toolUseId) : undefined;

  if (resultContent) {
    return <ToolResultCard toolName={toolName} content={resultContent} />;
  }

  const toolResults = event.message.content.filter(
    (b: ContentBlock) => b.type === "tool_result" && b.content
  );
  if (toolResults.length > 0) {
    return (
      <>
        {toolResults.map((block: ContentBlock, i: number) => (
          <ToolResultCard
            key={i}
            toolName={block.tool_use_id ? toolNames.get(block.tool_use_id) : undefined}
            content={block.content!}
          />
        ))}
      </>
    );
  }

  return null;
}

export const ClaudeJsonRenderer: React.FC<ClaudeJsonRendererProps> = ({
  id,
  width,
  height,
  onIvyEvent,
  events: enabledEvents = [],
  jsonStream,
  autoScroll = true,
  showThinking = false,
  showSystemEvents = false,
}) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const toolNames = useMemo(() => new Map<string, string>(), []);

  const parsedEvents = useMemo(() => parseEvents(jsonStream), [jsonStream]);

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

  if (!jsonStream || parsedEvents.length === 0) {
    return (
      <div style={style} className="claude-renderer text-[var(--muted-foreground)] p-4 text-sm">
        No events to display
      </div>
    );
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
              toolNames={toolNames}
            />
          );
        }

        if (event.type === "user") {
          return <UserMessage key={index} event={event} toolNames={toolNames} />;
        }

        if (event.type === "result") {
          return <ResultSummary key={index} event={event} />;
        }

        return null;
      })}
    </div>
  );
};
