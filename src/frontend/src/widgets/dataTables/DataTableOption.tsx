import React, { ReactNode, useState, useRef } from "react";
import { LucideIcon } from "lucide-react";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { cn } from "@/lib/utils";
import { Densities } from "@/types/density";
import { controlHeight, controlSize } from "@/components/ui/density-scale";
import { useCurrentBreakpoint } from "@/hooks/use-breakpoint-context";

/**
 * Display modes for DataTableOption
 */
export type OptionDisplayMode = "popover" | "inline";
export type InlineDirection = "right" | "left" | "below";

/**
 * Props for DataTableOption component
 */
export interface DataTableOptionProps {
  icon: LucideIcon;
  label: string;
  tooltip?: string;
  children: ReactNode;
  className?: string;
  contentClassName?: string;

  // Display mode configuration
  displayMode?: OptionDisplayMode;

  // Popover specific props
  align?: "start" | "center" | "end";
  side?: "top" | "right" | "bottom" | "left";
  sideOffset?: number;
  contentWidth?: string;

  // Inline specific props
  inlineDirection?: InlineDirection;
  defaultExpanded?: boolean;

  // Button configuration
  showLabel?: boolean;
  density?: Densities;
}

/**
 * DataTableOption - A configurable option button that can show content
 * either in a popover or inline expansion with unified border animation
 */
export const DataTableOption: React.FC<DataTableOptionProps> = ({
  icon: Icon,
  label,
  tooltip,
  children,
  className,
  contentClassName,
  displayMode = "inline",
  align = "start",
  side = "bottom",
  sideOffset = 8,
  contentWidth = "w-[400px]",
  inlineDirection = "right",
  defaultExpanded: propDefaultExpanded = false,
  showLabel = true,
  density = Densities.Medium,
}) => {
  const [expanded, setExpanded] = useState(() => propDefaultExpanded);
  const prevDefaultExpandedRef = useRef(propDefaultExpanded);

  if (propDefaultExpanded !== prevDefaultExpandedRef.current) {
    prevDefaultExpandedRef.current = propDefaultExpanded;
    setExpanded(propDefaultExpanded);
  }
  const containerRef = useRef<HTMLDivElement>(null);

  // Constrain the expanded inline width on smaller screens so the filter never
  // overflows the viewport horizontally. The interaction stays identical to
  // desktop (inline expansion) — only the expanded width adapts.
  const breakpoint = useCurrentBreakpoint();
  const isCompact = breakpoint === "mobile" || breakpoint === "tablet";

  const heightClass = controlHeight[density] || controlHeight.Medium;
  const sizeClass = controlSize[density] || controlSize.Medium;
  const pxClass =
    density === Densities.Small ? "px-2" : density === Densities.Large ? "px-4" : "px-3";

  // Handle click outside to collapse
  // useEffect(() => {
  //   if (!expanded || displayMode === 'popover') return;

  //   const handleClickOutside = (event: MouseEvent) => {
  //     if (
  //       containerRef.current &&
  //       !containerRef.current.contains(event.target as Node)
  //     ) {
  //       setExpanded(false);
  //     }
  //   };

  //   document.addEventListener('mousedown', handleClickOutside);
  //   return () => {
  //     document.removeEventListener('mousedown', handleClickOutside);
  //   };
  // }, [expanded, displayMode]);

  // Popover mode - uses default button styling
  if (displayMode === "popover") {
    return (
      <Popover>
        <PopoverTrigger asChild>
          <button
            className={cn(
              "inline-flex items-center justify-center rounded-md text-sm font-medium",
              `${heightClass} ${pxClass} gap-2 cursor-pointer`,
              "bg-transparent hover:bg-accent hover:text-accent-foreground",
              "border border-input",
              "transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring",
              className,
            )}
            title={tooltip || label}
          >
            <Icon className="size-4" />
            {showLabel && <span className="text-sm">{label}</span>}
          </button>
        </PopoverTrigger>
        <PopoverContent
          align={align}
          side={side}
          sideOffset={sideOffset}
          className={cn(contentWidth, "p-0", contentClassName)}
        >
          {children}
        </PopoverContent>
      </Popover>
    );
  }

  // Inline expansion mode with unified border
  const buttonContent = (
    <>
      <Icon className="size-4" />
      {showLabel && <span className="text-sm">{label}</span>}
    </>
  );

  if (inlineDirection === "right") {
    return (
      <div
        ref={containerRef}
        className={cn(
          // When expanded on compact screens the container spans the full available
          // width so the panel can fill the viewport instead of overflowing it.
          // Collapsed (or on desktop) it shrinks to the button.
          isCompact && expanded ? "flex w-full items-center" : "inline-flex items-center",
          "rounded-field border border-input bg-transparent shadow-sm",
          "dark:border-white/10 dark:bg-white/5",
          "focus-within:outline-none focus-within:ring-1 focus-within:ring-ring",
          "transition-all duration-300 ease-in-out",
          className,
        )}
      >
        <button
          className={cn(
            "inline-flex items-center justify-center text-sm font-medium",
            `${sizeClass} shrink-0 gap-2 cursor-pointer`,
            "bg-transparent rounded-l-fields",
            "transition-colors focus-visible:outline-none",
            expanded
              ? "bg-accent hover:bg-accent hover:text-accent-foreground"
              : "hover:bg-accent hover:text-accent-foreground",
          )}
          onClick={() => setExpanded(!expanded)}
          title={tooltip || label}
        >
          {buttonContent}
        </button>

        {/* Content container - fills remaining width on compact screens, fixed on desktop */}
        <div
          className={cn(
            `border-l ${heightClass}`,
            "transition-all duration-300 ease-in-out",
            expanded
              ? cn("opacity-100 border-input", isCompact ? "flex-1 min-w-0" : "w-[450px]")
              : "w-0 opacity-0 border-transparent",
          )}
        >
          <div
            className={cn(
              "flex h-full min-h-0 min-w-0 max-w-full items-stretch",
              isCompact ? "w-full" : "w-[450px]",
              "overflow-hidden rounded-l-none rounded-tr-fields rounded-br-fields",
              contentClassName,
            )}
          >
            {React.isValidElement(children)
              ? React.cloneElement(children as React.ReactElement<{ isExpanded?: boolean }>, {
                  isExpanded: expanded,
                })
              : children}
          </div>
        </div>
      </div>
    );
  }

  if (inlineDirection === "left") {
    return (
      <div
        ref={containerRef}
        className={cn(
          "inline-flex items-center",
          "border rounded-field",
          "transition-all duration-300 ease-in-out",
          "bg-transparent",
          expanded ? "border-input" : "border-input hover:bg-accent",
          className,
        )}
      >
        {/* Sliding content container */}
        <div
          className={cn(
            "transition-all duration-300 ease-in-out",
            "border-r",
            expanded
              ? "max-w-[800px] opacity-100 border-input/30"
              : "max-w-0 opacity-0 border-transparent",
          )}
        >
          <div className={cn(`${heightClass} flex items-center`, contentClassName)}>
            {React.isValidElement(children)
              ? React.cloneElement(children as React.ReactElement<{ isExpanded?: boolean }>, {
                  isExpanded: expanded,
                })
              : children}
          </div>
        </div>

        <button
          className={cn(
            "inline-flex items-center justify-center text-sm font-medium",
            `${heightClass} ${pxClass} gap-2 cursor-pointer`,
            "bg-transparent hover:bg-accent hover:text-accent-foreground rounded-r-md",
            "transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring",
            expanded && "bg-accent",
          )}
          onClick={() => setExpanded(!expanded)}
          title={tooltip || label}
        >
          {buttonContent}
        </button>
      </div>
    );
  }

  // Default: below with unified border
  return (
    <div
      ref={containerRef}
      className={cn(
        "inline-flex flex-col",
        "border rounded-field",
        "transition-all duration-300 ease-in-out",
        "bg-transparent",
        expanded ? "border-input" : "border-input hover:bg-accent",
        className,
      )}
    >
      <button
        className={cn(
          "inline-flex items-center justify-center text-sm font-medium",
          `${heightClass} ${pxClass} gap-2 w-full cursor-pointer`,
          "bg-transparent hover:bg-accent hover:text-accent-foreground",
          "transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring",
          expanded && "bg-accent border-b border-input/30 rounded-t-md",
        )}
        onClick={() => setExpanded(!expanded)}
        title={tooltip || label}
      >
        {buttonContent}
      </button>

      <div
        className={cn(
          "transition-all duration-300 ease-in-out",
          expanded ? "max-h-[200px] opacity-100" : "max-h-0 opacity-0",
        )}
      >
        <div className={cn("p-2", contentClassName)}>
          {React.isValidElement(children)
            ? React.cloneElement(children as React.ReactElement<{ isExpanded?: boolean }>, {
                isExpanded: expanded,
              })
            : children}
        </div>
      </div>
    </div>
  );
};
