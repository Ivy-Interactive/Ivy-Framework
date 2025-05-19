import * as React from "react"
import * as ScrollAreaPrimitive from "@radix-ui/react-scroll-area"

import { cn } from "@/lib/utils"

const ScrollArea = React.forwardRef<
  React.ElementRef<typeof ScrollAreaPrimitive.Root>,
  React.ComponentPropsWithoutRef<typeof ScrollAreaPrimitive.Root> & { scrollbarPosition?: "top" | "bottom", horizontalScroll?: boolean }
>(({ className, children, scrollbarPosition = "bottom", horizontalScroll = false, ...props }, ref) => (
  <ScrollAreaPrimitive.Root
    ref={ref}
    className={cn("relative overflow-hidden", className)}
    {...props}
  >
    {scrollbarPosition === "top" && <ScrollBar orientation="horizontal" scrollbarPosition="top" className="invisible-scrollbar" />}
    <ScrollAreaPrimitive.Viewport
      className="h-full w-full rounded-[inherit] overflow-x-auto"
      {...(horizontalScroll ? {
        onWheel: (e: React.WheelEvent<HTMLDivElement>) => {
          if (Math.abs(e.deltaY) > Math.abs(e.deltaX)) {
            e.currentTarget.scrollLeft += e.deltaY;
            e.preventDefault();
          }
        }
      } : {})}
    >
      {children}
    </ScrollAreaPrimitive.Viewport>
    <ScrollBar orientation="vertical" />
    {scrollbarPosition === "bottom" && <ScrollBar orientation="horizontal" scrollbarPosition="bottom" />}
    <ScrollAreaPrimitive.Corner />
  </ScrollAreaPrimitive.Root>
))
ScrollArea.displayName = ScrollAreaPrimitive.Root.displayName

const ScrollBar = React.forwardRef<
  React.ElementRef<typeof ScrollAreaPrimitive.ScrollAreaScrollbar>,
  React.ComponentPropsWithoutRef<typeof ScrollAreaPrimitive.ScrollAreaScrollbar> & { scrollbarPosition?: "top" | "bottom" }
>(({ className, orientation = "vertical", scrollbarPosition, ...props }, ref) => (
  <ScrollAreaPrimitive.ScrollAreaScrollbar
    ref={ref}
    orientation={orientation}
    className={cn(
      "flex touch-none select-none transition-colors",
      orientation === "vertical" &&
        "h-full w-2.5 border-l border-l-transparent p-[1px]",
      orientation === "horizontal" &&
        "h-2.5 flex-col border-t border-t-transparent p-[1px]",
      orientation === "horizontal" && scrollbarPosition === "top" && "scrollbar-top",
      className
    )}
    {...props}
  >
    <ScrollAreaPrimitive.ScrollAreaThumb className="relative flex-1 rounded-full bg-border" />
  </ScrollAreaPrimitive.ScrollAreaScrollbar>
))
ScrollBar.displayName = ScrollAreaPrimitive.ScrollAreaScrollbar.displayName

export { ScrollArea, ScrollBar }
