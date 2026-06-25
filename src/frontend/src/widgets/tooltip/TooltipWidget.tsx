import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";
import React from "react";
import Icon from "@/components/Icon";
import { cn } from "@/lib/utils";
import { Densities } from "@/types/density";
import { useEventHandler } from "@/components/event-handler";

type TooltipVariant = "Default" | "Info" | "Success" | "Warning" | "Error";

interface TooltipWidgetProps {
  id: string;
  density?: Densities;
  open?: boolean;
  showArrow?: boolean;
  persistent?: boolean;
  variant?: TooltipVariant;
  events?: string[];
  slots?: {
    Trigger?: React.ReactNode[];
    Content?: React.ReactNode[];
  };
}

const VARIANT_COLOR_TOKEN: Record<Exclude<TooltipVariant, "Default">, string> = {
  Info: "info",
  Success: "success",
  Warning: "warning",
  Error: "destructive",
};

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
  variant = "Default",
  events = EMPTY_EVENTS,
  slots,
}) => {
  const eventHandler = useEventHandler();

  const colorToken = variant === "Default" ? undefined : VARIANT_COLOR_TOKEN[variant];
  const contentStyle: React.CSSProperties | undefined = colorToken
    ? {
        backgroundColor: `var(--${colorToken})`,
        color: `var(--${colorToken}-foreground)`,
        ["--tooltip-arrow-fill" as string]: `var(--${colorToken})`,
      }
    : undefined;

  const serverControlled = openProp !== undefined;
  const isControlled = serverControlled || persistent;

  const [open, setOpen] = React.useState<boolean>(openProp ?? false);

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
      if (isControlled && !next) {
        return;
      }
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

  return (
    <TooltipProvider>
      <Tooltip {...(isControlled ? { open } : {})} onOpenChange={handleOpenChange}>
        <TooltipTrigger asChild>
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
