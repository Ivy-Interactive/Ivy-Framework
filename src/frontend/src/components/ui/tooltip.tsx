import * as React from "react";
import * as TooltipPrimitive from "@radix-ui/react-tooltip";

import { cn } from "@/lib/utils";

const TooltipProvider = ({
  delayDuration = 500,
  ...props
}: React.ComponentPropsWithoutRef<typeof TooltipPrimitive.Provider>) => (
  <TooltipPrimitive.Provider delayDuration={delayDuration} {...props} />
);

type TooltipControl = {
  open: boolean;
  setOpen: (next: boolean) => void;
};

const TooltipContext = React.createContext<TooltipControl | null>(null);

/**
 * Radix Tooltip is hover/focus driven, so coarse pointers (finger / stylus tap)
 * never see it on iPad/iPhone. Wrap Root with a small controlled-state shim and
 * let the Trigger toggle it on touch / pen pointerdown without affecting mouse.
 */
const Tooltip = ({
  open: openProp,
  defaultOpen,
  onOpenChange,
  ...props
}: React.ComponentPropsWithoutRef<typeof TooltipPrimitive.Root>) => {
  const [internalOpen, setInternalOpen] = React.useState(defaultOpen ?? false);
  const isControlled = openProp !== undefined;
  const open = isControlled ? openProp : internalOpen;

  const setOpen = React.useCallback(
    (next: boolean) => {
      if (!isControlled) {
        setInternalOpen(next);
      }
      onOpenChange?.(next);
    },
    [isControlled, onOpenChange],
  );

  return (
    <TooltipContext.Provider value={{ open, setOpen }}>
      <TooltipPrimitive.Root open={open} onOpenChange={setOpen} {...props} />
    </TooltipContext.Provider>
  );
};

const TooltipTrigger = React.forwardRef<
  React.ElementRef<typeof TooltipPrimitive.Trigger>,
  React.ComponentPropsWithoutRef<typeof TooltipPrimitive.Trigger>
>((props, ref) => {
  const ctx = React.useContext(TooltipContext);
  return (
    <TooltipPrimitive.Trigger
      ref={ref}
      {...props}
      onPointerDown={(e) => {
        props.onPointerDown?.(e);
        if (!ctx) return;
        if (e.pointerType === "touch" || e.pointerType === "pen") {
          ctx.setOpen(!ctx.open);
        }
      }}
    />
  );
});
TooltipTrigger.displayName = TooltipPrimitive.Trigger.displayName;

const hasNoSpaces = (str: string): boolean => {
  const trimmed = str.trim();
  return trimmed.length > 0 && !trimmed.includes(" ");
};

const needsBreakAll = (content: React.ReactNode): boolean => {
  const checkString = (str: string): boolean => {
    return hasNoSpaces(str);
  };

  if (typeof content === "string") {
    return checkString(content);
  }

  if (React.isValidElement(content)) {
    const props = content.props as { children?: React.ReactNode };
    const children = props?.children;
    if (typeof children === "string") {
      return checkString(children);
    }
    if (Array.isArray(children)) {
      return children.some((child) => needsBreakAll(child));
    }
    if (children) {
      return needsBreakAll(children);
    }
  }

  return false;
};

export type TooltipBreakType = "auto" | "normal" | "all" | "words";

interface TooltipContentProps extends React.ComponentPropsWithoutRef<
  typeof TooltipPrimitive.Content
> {
  breakType?: TooltipBreakType;
}

const TooltipContent = React.forwardRef<
  React.ElementRef<typeof TooltipPrimitive.Content>,
  TooltipContentProps
>(({ className, sideOffset = 4, breakType = "auto", children, ...props }, ref) => {
  const getBreakClass = React.useMemo(() => {
    if (breakType !== "auto") {
      switch (breakType) {
        case "normal":
          return "break-normal";
        case "all":
          return "break-all";
        case "words":
          return "break-words";
        default:
          return "break-normal";
      }
    }

    return needsBreakAll(children) ? "break-all" : "break-normal";
  }, [breakType, children]);

  return (
    <TooltipPrimitive.Portal>
      <TooltipPrimitive.Content
        ref={ref}
        sideOffset={sideOffset}
        className={cn(
          "z-50 overflow-hidden rounded-selector border border-border bg-popover px-3 py-1.5 text-xs text-popover-foreground shadow-md animate-in fade-in-0 zoom-in-95 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:zoom-out-95 data-[side=bottom]:slide-in-from-top-2 data-[side=left]:slide-in-from-right-2 data-[side=right]:slide-in-from-left-2 data-[side=top]:slide-in-from-bottom-2 max-w-sm",
          className,
        )}
        {...props}
      >
        <div className={cn("max-h-[20vh] overflow-hidden", getBreakClass)}>{children}</div>
      </TooltipPrimitive.Content>
    </TooltipPrimitive.Portal>
  );
});
TooltipContent.displayName = TooltipPrimitive.Content.displayName;

export { Tooltip, TooltipTrigger, TooltipContent, TooltipProvider };
export type { TooltipContentProps };
