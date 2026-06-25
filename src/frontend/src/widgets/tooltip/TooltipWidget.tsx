import {
  Tooltip,
  TooltipArrow,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import React from "react";
import Icon from "@/components/Icon";
import { cn } from "@/lib/utils";
import { Densities } from "@/types/density";
import { useEventHandler } from "@/components/event-handler";

interface TooltipWidgetProps {
  id: string;
  density?: Densities;
  /** Controlled open state. When provided, the tooltip is driven by this value instead of hover/focus. */
  open?: boolean;
  /** Renders an arrow ("bubble") pointing at the trigger. Set via `.Bubble()` in C#. */
  showArrow?: boolean;
  /** Keeps the tooltip open until dismissed and renders a close button. Set via `.Persist()` in C#. */
  persistent?: boolean;
  events?: string[];
  slots?: {
    Trigger?: React.ReactNode[];
    Content?: React.ReactNode[];
  };
}

function extractText(nodes: React.ReactNode[]): string | undefined {
  const parts: string[] = [];
  for (const node of nodes) {
    if (typeof node === "string") parts.push(node);
    else if (typeof node === "number") parts.push(String(node));
  }
  return parts.length > 0 ? parts.join("") : undefined;
}

const EMPTY_EVENTS: string[] = [];

export const TooltipWidget: React.FC<TooltipWidgetProps> = ({
  id,
  density: _density = Densities.Medium,
  open: openProp,
  showArrow = false,
  persistent = false,
  events = EMPTY_EVENTS,
  slots,
}) => {
  const eventHandler = useEventHandler();

  // A persistent or server-controlled tooltip is controlled; a plain hover tooltip is uncontrolled.
  const isControlled = persistent || openProp !== undefined;

  // Local open state seeds from the server's `open` prop (or stays open while persistent).
  const [open, setOpen] = React.useState<boolean>(openProp ?? false);

  // Keep local state in sync when the server pushes a new `open` value.
  React.useEffect(() => {
    if (openProp !== undefined) setOpen(openProp);
  }, [openProp]);

  const handleOpenChange = React.useCallback(
    (next: boolean) => {
      // A persistent tooltip never auto-closes — only the close button dismisses it.
      if (persistent && !next) return;

      setOpen(next);
      if (events.includes("OnOpenChange")) eventHandler("OnOpenChange", id, [next]);
      if (next) {
        if (events.includes("OnOpen")) eventHandler("OnOpen", id, []);
      } else if (events.includes("OnClose")) {
        eventHandler("OnClose", id, []);
      }
    },
    [persistent, events, eventHandler, id],
  );

  const handleClose = React.useCallback(() => {
    setOpen(false);
    if (events.includes("OnOpenChange")) eventHandler("OnOpenChange", id, [false]);
    if (events.includes("OnClose")) eventHandler("OnClose", id, []);
  }, [events, eventHandler, id]);

  if (!slots?.Trigger || !slots?.Content) {
    return (
      <div className="text-red-500">Error: Tooltip requires both Trigger and Content slots.</div>
    );
  }

  const ariaLabel = extractText(slots.Content);

  // asChild + span: we need a single DOM node that receives ref/handlers (slot widgets like ButtonWidget don't forward ref).
  // A span wrapper avoids TooltipTrigger's default <button> so we don't get invalid button-in-button.
  return (
    <TooltipProvider>
      <Tooltip {...(isControlled ? { open } : {})} onOpenChange={handleOpenChange}>
        <TooltipTrigger asChild>
          {/* width:fit-content (not just inline-block): inside a flex/grid layout the wrapper is
              blockified and stretches to the row width, which makes Radix anchor — and center —
              the tooltip on the full row instead of the trigger. */}
          <span
            style={{ display: "inline-block", width: "fit-content", maxWidth: "100%" }}
            aria-label={ariaLabel}
          >
            {slots.Trigger}
          </span>
        </TooltipTrigger>
        <TooltipContent
          className="bg-popover text-popover-foreground shadow-md"
          // A persistent tooltip stays put when the pointer leaves or focus changes.
          onPointerDownOutside={persistent ? (e) => e.preventDefault() : undefined}
          onEscapeKeyDown={persistent ? handleClose : undefined}
        >
          <div className={cn("flex items-start gap-2", persistent && "pr-1")}>
            <div className="min-w-0">{slots.Content}</div>
            {persistent && (
              <button
                type="button"
                aria-label="Close"
                onClick={handleClose}
                className="-mr-1 -mt-0.5 shrink-0 rounded-selector p-0.5 text-popover-foreground/70 hover:bg-foreground/10 hover:text-popover-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
              >
                <Icon name="X" className="h-3 w-3" />
              </button>
            )}
          </div>
          {showArrow && <TooltipArrow width={11} height={5} />}
        </TooltipContent>
      </Tooltip>
    </TooltipProvider>
  );
};
