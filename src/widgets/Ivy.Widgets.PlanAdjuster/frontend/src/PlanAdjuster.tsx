import React, { useState, useCallback, useMemo, useRef, useEffect } from "react";
import Markdown from "react-markdown";
import rehypeHighlight from "rehype-highlight";
import remarkGfm from "remark-gfm";
import "./plan-adjuster.css";

type EventHandler = (eventName: string, widgetId: string, args: unknown[]) => void;

interface PlanAdjusterProps {
  id: string;
  width?: string;
  height?: string;
  onIvyEvent: EventHandler;
  events?: string[];
  content?: string;
  dangerouslyAllowLocalFiles?: boolean;
}

interface Adjustment {
  paragraphIndex: number;
  originalText: string;
  text: string;
}

function splitMarkdownIntoBlocks(content: string): string[] {
  const blocks: string[] = [];
  let current = "";
  let inCodeFence = false;

  for (const line of content.split("\n")) {
    if (line.trimStart().startsWith("```")) {
      inCodeFence = !inCodeFence;
    }

    if (!inCodeFence && line.trim() === "" && current.trim() !== "") {
      blocks.push(current.trimEnd());
      current = "";
    } else {
      current += (current ? "\n" : "") + line;
    }
  }

  if (current.trim()) blocks.push(current.trimEnd());
  return blocks;
}

function AdjustmentPopover({
  paragraphIndex,
  existingText,
  anchorRect,
  onSave,
  onRemove,
  onCancel,
}: {
  paragraphIndex: number;
  existingText: string;
  anchorRect: DOMRect;
  onSave: (paragraphIndex: number, text: string) => void;
  onRemove: (paragraphIndex: number) => void;
  onCancel: () => void;
}) {
  const [text, setText] = useState(existingText);
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const popoverRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    textareaRef.current?.focus();
  }, []);

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (popoverRef.current && !popoverRef.current.contains(e.target as Node)) {
        onCancel();
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [onCancel]);

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Escape") {
      onCancel();
    }
    if (e.key === "Enter" && (e.ctrlKey || e.metaKey)) {
      e.preventDefault();
      if (text.trim()) {
        onSave(paragraphIndex, text.trim());
      }
    }
  };

  const top = anchorRect.top;
  const left = anchorRect.right + 8;

  return (
    <div
      ref={popoverRef}
      className="plan-adjuster-popover"
      style={{ top, left }}
    >
      <textarea
        ref={textareaRef}
        className="plan-adjuster-textarea"
        value={text}
        onChange={(e) => setText(e.target.value)}
        onKeyDown={handleKeyDown}
        placeholder="Describe the adjustment for this section..."
        rows={4}
      />
      <div className="plan-adjuster-popover-actions">
        {existingText && (
          <button
            className="plan-adjuster-btn plan-adjuster-btn-remove"
            onClick={() => onRemove(paragraphIndex)}
          >
            Remove
          </button>
        )}
        <div className="plan-adjuster-popover-actions-right">
          <button
            className="plan-adjuster-btn plan-adjuster-btn-cancel"
            onClick={onCancel}
          >
            Cancel
          </button>
          <button
            className="plan-adjuster-btn plan-adjuster-btn-save"
            onClick={() => {
              if (text.trim()) onSave(paragraphIndex, text.trim());
            }}
            disabled={!text.trim()}
          >
            {existingText ? "Update" : "Add Adjustment"}
          </button>
        </div>
      </div>
    </div>
  );
}

export const PlanAdjuster: React.FC<PlanAdjusterProps> = ({
  id,
  width,
  height,
  onIvyEvent,
  events: enabledEvents = [],
  content = "",
  dangerouslyAllowLocalFiles = false,
}) => {
  const [adjustments, setAdjustments] = useState<Map<number, string>>(new Map());
  const [hoveredBlock, setHoveredBlock] = useState<number | null>(null);
  const [activePopover, setActivePopover] = useState<number | null>(null);
  const [popoverAnchor, setPopoverAnchor] = useState<DOMRect | null>(null);

  const blocks = useMemo(() => splitMarkdownIntoBlocks(content), [content]);

  const handleIconClick = useCallback(
    (index: number, iconElement: HTMLElement) => {
      const rect = iconElement.getBoundingClientRect();
      setActivePopover(index);
      setPopoverAnchor(rect);
    },
    [],
  );

  const handleSave = useCallback(
    (paragraphIndex: number, text: string) => {
      setAdjustments((prev) => {
        const next = new Map(prev);
        next.set(paragraphIndex, text);
        return next;
      });
      setActivePopover(null);
      setPopoverAnchor(null);
    },
    [],
  );

  const handleRemove = useCallback(
    (paragraphIndex: number) => {
      setAdjustments((prev) => {
        const next = new Map(prev);
        next.delete(paragraphIndex);
        return next;
      });
      setActivePopover(null);
      setPopoverAnchor(null);
    },
    [],
  );

  const handleCancel = useCallback(() => {
    setActivePopover(null);
    setPopoverAnchor(null);
  }, []);

  const handleUpdate = useCallback(() => {
    if (adjustments.size === 0) return;

    const payload = {
      adjustments: Array.from(adjustments.entries()).map(([paragraphIndex, text]) => ({
        paragraphIndex,
        originalText: blocks[paragraphIndex] ?? "",
        text,
      })) satisfies Adjustment[],
    };

    if (enabledEvents.includes("OnUpdate")) {
      onIvyEvent("OnUpdate", id, [JSON.stringify(payload)]);
    }
  }, [adjustments, blocks, enabledEvents, onIvyEvent, id]);

  const handleLinkClick = useCallback(
    (href: string) => {
      if (enabledEvents.includes("OnLinkClick")) {
        onIvyEvent("OnLinkClick", id, [href]);
      }
    },
    [enabledEvents, onIvyEvent, id],
  );

  const markdownComponents = useMemo(
    () => ({
      a: ({
        href,
        children,
        ...props
      }: React.AnchorHTMLAttributes<HTMLAnchorElement>) => {
        const isFileLink = href?.startsWith("file://");
        const isPlanLink = href?.startsWith("plan://");

        if (isFileLink && !dangerouslyAllowLocalFiles) {
          return <span {...props}>{children}</span>;
        }

        return (
          <a
            {...props}
            href={href}
            onClick={(e) => {
              if (isFileLink || isPlanLink) {
                e.preventDefault();
                if (href) handleLinkClick(href);
              }
            }}
          >
            {children}
          </a>
        );
      },
      img: ({
        src,
        ...props
      }: React.ImgHTMLAttributes<HTMLImageElement>) => {
        if (src?.startsWith("file://") && !dangerouslyAllowLocalFiles) {
          return null;
        }
        return <img src={src} {...props} />;
      },
    }),
    [dangerouslyAllowLocalFiles, handleLinkClick],
  );

  const style: React.CSSProperties = {
    width: width ?? "100%",
    height: height ?? "auto",
    overflow: "auto",
    position: "relative",
  };

  return (
    <div className="plan-adjuster-root" style={style}>
      <div className="plan-adjuster-content">
        {blocks.map((block, index) => {
          const hasAdj = adjustments.has(index);
          const isHovered = hoveredBlock === index;

          return (
            <div
              key={index}
              className={`plan-adjuster-block ${hasAdj ? "has-adjustment" : ""}`}
              onMouseEnter={() => setHoveredBlock(index)}
              onMouseLeave={() => {
                if (activePopover !== index) setHoveredBlock(null);
              }}
            >
              <div className="plan-adjuster-block-content">
                <Markdown
                  remarkPlugins={[remarkGfm]}
                  rehypePlugins={[rehypeHighlight]}
                  components={markdownComponents}
                >
                  {block}
                </Markdown>
              </div>

              {(isHovered || hasAdj) && (
                <button
                  className={`plan-adjuster-icon ${hasAdj ? "has-adjustment" : ""}`}
                  onClick={(e) => handleIconClick(index, e.currentTarget)}
                  title={hasAdj ? "Edit adjustment" : "Add adjustment"}
                >
                  {hasAdj ? (
                    <svg
                      width="16"
                      height="16"
                      viewBox="0 0 16 16"
                      fill="currentColor"
                    >
                      <path d="M12.146.854a.5.5 0 0 1 .708 0l2.292 2.292a.5.5 0 0 1 0 .708l-9.5 9.5a.5.5 0 0 1-.168.11l-4 1.5a.5.5 0 0 1-.638-.638l1.5-4a.5.5 0 0 1 .11-.168l9.5-9.5zM11.207 2.5 13.5 4.793 14.793 3.5 12.5 1.207 11.207 2.5zm1.586 3L10.5 3.207l-8 8-.964 2.571 2.571-.964 8-8z" />
                    </svg>
                  ) : (
                    <svg
                      width="16"
                      height="16"
                      viewBox="0 0 16 16"
                      fill="currentColor"
                      opacity="0.5"
                    >
                      <path d="M12.146.854a.5.5 0 0 1 .708 0l2.292 2.292a.5.5 0 0 1 0 .708l-9.5 9.5a.5.5 0 0 1-.168.11l-4 1.5a.5.5 0 0 1-.638-.638l1.5-4a.5.5 0 0 1 .11-.168l9.5-9.5zM11.207 2.5 13.5 4.793 14.793 3.5 12.5 1.207 11.207 2.5zm1.586 3L10.5 3.207l-8 8-.964 2.571 2.571-.964 8-8z" />
                    </svg>
                  )}
                </button>
              )}
            </div>
          );
        })}
      </div>

      {activePopover !== null && popoverAnchor && (
        <AdjustmentPopover
          paragraphIndex={activePopover}
          existingText={adjustments.get(activePopover) ?? ""}
          anchorRect={popoverAnchor}
          onSave={handleSave}
          onRemove={handleRemove}
          onCancel={handleCancel}
        />
      )}

      {adjustments.size > 0 && (
        <div className="plan-adjuster-update-bar">
          <button
            className="plan-adjuster-btn plan-adjuster-btn-update"
            onClick={handleUpdate}
          >
            Update ({adjustments.size} adjustment{adjustments.size !== 1 ? "s" : ""})
          </button>
        </div>
      )}
    </div>
  );
};
