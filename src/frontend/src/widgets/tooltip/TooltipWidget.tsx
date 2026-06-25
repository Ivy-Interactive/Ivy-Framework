import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";
import React from "react";
import Icon from "@/components/Icon";
import { cn } from "@/lib/utils";
import { colorNameToCssToken } from "@/lib/styles";
import { Densities } from "@/types/density";
import { useEventHandler } from "@/components/event-handler";

interface TooltipWidgetProps {
  id: string;
  density?: Densities;
  open?: boolean;
  showArrow?: boolean;
  persistent?: boolean;
  background?: string;
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
  background,
  events = EMPTY_EVENTS,
  slots,
}) => {
  const eventHandler = useEventHandler();

  // A semantic background color (e.g. Error -> red, Info -> blue) overrides the default card surface.
  // The arrow fill is driven by the same CSS variable so it melts into the colored bubble.
  const colorToken = background?.trim() ? colorNameToCssToken(background.trim()) : undefined;
  const contentStyle: React.CSSProperties | undefined = colorToken
    ? {
        backgroundColor: `var(--${colorToken})`,
        color: `var(--${colorToken}-foreground)`,
        ["--tooltip-arrow-fill" as string]: `var(--${colorToken})`,
      }
    : undefined;

  // Server-controlled: the `open` prop is set, so the server owns the open state and the tooltip
  // appears in response to an event (e.g. an error). Persistent: stays open until dismissed.
  // Both modes are "controlled" — we own the open state in React and Radix must not auto-close on
  // hover-out. A plain hover tooltip (neither flag) stays fully uncontrolled.
  const serverControlled = openProp !== undefined;
  const isControlled = serverControlled || persistent;

  // Local open state seeds from the server's `open` prop (or stays closed until hovered while persistent).
  const [open, setOpen] = React.useState<boolean>(openProp ?? false);

  // Keep local state in sync when the server pushes a new `open` value.
  React.useEffect(() => {
    if (openProp !== undefined) setOpen(openProp);
  }, [openProp]);

  const emitOpenEvents = React.useCallback(
    (next: boolean) => {
      if (events.includes("OnOpenChange")) eventHandler("OnOpenChange", id, [next]);
      if (next) {
        if (events.includes("OnOpen")) eventHandler("OnOpen", id, []);
      } else if (events.includes("OnClose")) {
        eventHandler("OnClose", id, []);
      }
    },
    [events, eventHandler, id],
  );

  const handleOpenChange = React.useCallback(
    (next: boolean) => {
      // Controlled tooltips (persistent or server-controlled `open`) ignore Radix's hover/focus
      // driven open/close entirely — including the close request Radix fires on hover-out. They open
      // via hover-in (persistent) or the `open` prop (server), and close only via the close button /
      // Escape. Crucially we must NOT emit close events on hover-out: a wired OnClose handler that
      // flips `open` back to false would make the tooltip vanish when the cursor leaves.
      if (isControlled && !next) {
        return;
      }
      // Server-controlled tooltips don't open on hover either — only the `open` prop opens them.
      if (serverControlled && next) {
        return;
      }
      setOpen(next);
      emitOpenEvents(next);
    },
    [isControlled, serverControlled, emitOpenEvents],
  );

  const handleClose = React.useCallback(() => {
    setOpen(false);
    emitOpenEvents(false);
  }, [emitOpenEvents]);

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
          showArrow={showArrow}
          style={contentStyle}
          arrowClassName={colorToken ? "fill-[var(--tooltip-arrow-fill)]" : undefined}
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
                className="-mr-1 -mt-0.5 shrink-0 rounded-selector p-0.5 text-current/70 hover:bg-foreground/10 hover:text-current focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
              >
                <Icon name="X" className="h-3 w-3" />
              </button>
            )}
          </div>
        </TooltipContent>
      </Tooltip>
    </TooltipProvider>
  );
};
