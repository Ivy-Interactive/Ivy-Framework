import React, { useState, useCallback, useMemo } from "react";
import { getColor, getOverflow, Overflow } from "@/lib/styles";
import { cn } from "@/lib/utils";
import { typography } from "@/lib/styles";
import { useStream } from "@/components/stream-handler/hooks";
import { useEventHandler } from "@/components/event-handler/hooks";
import { Densities } from "@/types/density";
import { TextAlignment } from "@/types/textAlignment";
import MarkdownRenderer from "@/components/MarkdownRenderer";

interface TextRun {
  content: string;
  bold?: boolean;
  italic?: boolean;
  strikeThrough?: boolean;
  color?: string;
  highlightColor?: string;
  word?: boolean;
  link?: string;
  linkTarget?: "Self" | "Blank";
  lineBreak?: boolean;
  code?: boolean;
  heading?: number;
  paragraph?: boolean;
  bulletItem?: number;
  orderedItem?: number;
  horizontalRule?: boolean;
  codeBlock?: string;
  blockquote?: boolean;
  table?: string;
}

interface RichTextBlockWidgetProps {
  id: string;
  runs?: TextRun[];
  stream?: { id: string };
  textAlignment?: TextAlignment;
  noWrap?: boolean;
  overflow?: Overflow;
  density?: Densities;
  events?: string[];
}

const scaleClasses: Record<string, string> = {
  [Densities.Small]: typography.small,
  [Densities.Large]: typography.large,
};

const EMPTY_RUNS: TextRun[] = [];
const EMPTY_EVENTS: string[] = [];

const headingClasses: Record<number, string> = {
  1: typography.h1,
  2: typography.h2,
  3: typography.h3,
  4: typography.h4,
  5: typography.h5,
  6: typography.h6,
};

function renderInlineContent(
  run: TextRun,
  index: number,
  events: string[],
  eventHandler: ReturnType<typeof useEventHandler>,
  widgetId: string,
) {
  const runStyles: React.CSSProperties = {
    ...getColor(run.color, "color", "background"),
    ...getColor(run.highlightColor, "backgroundColor", "background"),
  };

  const className = cn(
    run.bold && "font-semibold",
    run.italic && "italic",
    run.strikeThrough && "line-through",
  );

  const content = (
    <>
      {run.word && index > 0 ? " " : ""}
      {run.content}
    </>
  );

  if (run.link) {
    const isBlank = run.linkTarget === "Blank";
    if (events.includes("OnLinkClick")) {
      return (
        <button
          key={`run-${index}`}
          type="button"
          className={cn(className, "underline cursor-pointer text-left")}
          style={runStyles}
          onClick={() => {
            eventHandler("OnLinkClick", widgetId, [run.link]);
          }}
        >
          {content}
        </button>
      );
    }

    return (
      <a
        key={`run-${index}`}
        href={run.link}
        target={isBlank ? "_blank" : "_self"}
        rel={isBlank ? "noopener noreferrer" : undefined}
        className={cn(className, "underline")}
        style={runStyles}
      >
        {content}
      </a>
    );
  }

  if (run.code) {
    return (
      <code key={`run-${index}`} className={typography.code} style={runStyles}>
        {run.content}
      </code>
    );
  }

  return (
    <span key={`run-${index}`} className={className} style={runStyles}>
      {content}
    </span>
  );
}

interface GroupedBlock {
  type: "inline" | "bullet" | "ordered";
  runs: { run: TextRun; index: number }[];
}

function groupRuns(allRuns: TextRun[]): GroupedBlock[] {
  const groups: GroupedBlock[] = [];
  let currentGroup: GroupedBlock | null = null;

  allRuns.forEach((run, index) => {
    if (run.bulletItem && run.bulletItem > 0) {
      if (currentGroup?.type === "bullet") {
        currentGroup.runs.push({ run, index });
      } else {
        currentGroup = { type: "bullet", runs: [{ run, index }] };
        groups.push(currentGroup);
      }
    } else if (run.orderedItem && run.orderedItem > 0) {
      if (currentGroup?.type === "ordered") {
        currentGroup.runs.push({ run, index });
      } else {
        currentGroup = { type: "ordered", runs: [{ run, index }] };
        groups.push(currentGroup);
      }
    } else {
      currentGroup = { type: "inline", runs: [{ run, index }] };
      groups.push(currentGroup);
    }
  });

  return groups;
}

export const RichTextBlockWidget: React.FC<RichTextBlockWidgetProps> = ({
  id,
  runs = EMPTY_RUNS,
  stream,
  textAlignment,
  noWrap,
  overflow,
  density,
  events = EMPTY_EVENTS,
}) => {
  const [streamedRuns, setStreamedRuns] = useState<TextRun[]>([]);
  const eventHandler = useEventHandler();

  const onData = useCallback((run: TextRun) => {
    setStreamedRuns((prev) => [...prev, run]);
  }, []);

  useStream<TextRun>(stream?.id, onData);

  const allRuns = useMemo(() => [...runs, ...streamedRuns], [runs, streamedRuns]);

  const styles: React.CSSProperties = {
    ...getOverflow(overflow),
    wordBreak: "normal",
    overflowWrap: "break-word",
    ...(textAlignment && {
      textAlign: textAlignment.toLowerCase() as React.CSSProperties["textAlign"],
    }),
  };

  const hasBlockRuns = allRuns.some(
    (r) =>
      r.lineBreak ||
      r.horizontalRule ||
      r.heading ||
      r.codeBlock != null ||
      r.blockquote ||
      r.bulletItem ||
      r.orderedItem ||
      r.paragraph ||
      r.table,
  );

  // If no block-level runs, use original simple rendering
  if (!hasBlockRuns) {
    return (
      <span
        style={styles}
        className={cn(noWrap && "whitespace-nowrap", density && scaleClasses[density])}
      >
        {allRuns.map((run, index) => renderInlineContent(run, index, events, eventHandler, id))}
      </span>
    );
  }

  // Block-level rendering
  const groups = groupRuns(allRuns);

  return (
    <div
      style={styles}
      className={cn(noWrap && "whitespace-nowrap", density && scaleClasses[density])}
    >
      {groups.map((group, groupIndex) => {
        if (group.type === "bullet") {
          return (
            <ul key={`group-${groupIndex}`} className={typography.ul}>
              {group.runs.map(({ run, index }) => (
                <li
                  key={`run-${index}`}
                  className={run.bulletItem && run.bulletItem > 1 ? "ml-4" : undefined}
                >
                  {renderInlineContent(run, index, events, eventHandler, id)}
                </li>
              ))}
            </ul>
          );
        }

        if (group.type === "ordered") {
          return (
            <ol key={`group-${groupIndex}`} className={typography.ol}>
              {group.runs.map(({ run, index }) => (
                <li key={`run-${index}`}>
                  {renderInlineContent(run, index, events, eventHandler, id)}
                </li>
              ))}
            </ol>
          );
        }

        // Inline group — render each run individually with block-level wrappers
        return group.runs.map(({ run, index }) => {
          if (run.lineBreak) {
            return <br key={`run-${index}`} />;
          }

          if (run.horizontalRule) {
            return <hr key={`run-${index}`} className={typography.hr} />;
          }

          if (run.heading && run.heading >= 1 && run.heading <= 6) {
            const Tag = `h${run.heading}` as keyof React.JSX.IntrinsicElements;
            const hClass = headingClasses[run.heading] || "";
            return (
              <Tag key={`run-${index}`} className={hClass}>
                {renderInlineContent(run, index, events, eventHandler, id)}
              </Tag>
            );
          }

          if (run.codeBlock !== undefined) {
            return (
              <pre key={`run-${index}`} className="rounded-md bg-muted p-4 overflow-x-auto">
                <code className={run.codeBlock ? `language-${run.codeBlock}` : undefined}>
                  {run.content}
                </code>
              </pre>
            );
          }

          if (run.blockquote) {
            return (
              <blockquote key={`run-${index}`} className={typography.blockquote}>
                {renderInlineContent(run, index, events, eventHandler, id)}
              </blockquote>
            );
          }

          if (run.table) {
            return <MarkdownRenderer key={`run-${index}`} content={run.table} />;
          }

          if (run.paragraph) {
            return (
              <p key={`run-${index}`} className={typography.p}>
                {renderInlineContent(run, index, events, eventHandler, id)}
              </p>
            );
          }

          return renderInlineContent(run, index, events, eventHandler, id);
        });
      })}
    </div>
  );
};
