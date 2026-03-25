import * as React from "react";
import { X, ChevronDown } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Command, CommandGroup, CommandItem } from "@/components/ui/command";
import { Command as CommandPrimitive } from "cmdk";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";
import { cn } from "@/lib/utils";
import { cva } from "class-variance-authority";
import { Densities } from "@/types/density";
import { xIconVariant } from "@/components/ui/input/text-input-variant";
import { selectTriggerVariant } from "@/components/ui/select/variant";

// Variants for MultipleSelector - matches selectTriggerVariant exactly
const multipleSelectorVariant = selectTriggerVariant;

// Variants for menu items
const menuItemVariant = cva("cursor-pointer", {
  variants: {
    density: {
      Small: "px-2 py-1 text-xs",
      Medium: "px-3 py-2 text-sm",
      Large: "px-4 py-3 text-base",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

// Variants for Badge components
const badgeVariant = cva("hover:bg-secondary", {
  variants: {
    density: {
      Small: "text-xs",
      Medium: "text-sm",
      Large: "text-base",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

export interface Option {
  label: string;
  value: string;
  disable?: boolean;
  tooltip?: string;
}

interface MultipleSelectorProps {
  value?: Option[];
  defaultOptions?: Option[];
  onValueChange?: (value: Option[]) => void;
  placeholder?: string;
  disabled?: boolean;
  className?: string;
  commandProps?: {
    label?: string;
  };
  hidePlaceholderWhenSelected?: boolean;
  emptyIndicator?: React.ReactNode;
  invalid?: boolean;
  density?: Densities;
  maxVisibleBadges?: number;
  ghost?: boolean;
}

const MultipleSelector = React.forwardRef<
  React.ElementRef<typeof CommandPrimitive>,
  MultipleSelectorProps
>(
  (
    {
      value = [],
      defaultOptions = [],
      onValueChange,
      placeholder = "Select options...",
      disabled = false,
      className,
      commandProps,
      hidePlaceholderWhenSelected = false,
      emptyIndicator,
      invalid = false,
      density = Densities.Medium,
      maxVisibleBadges,
      ghost = false,
    },
    ref,
  ) => {
    const inputRef = React.useRef<HTMLInputElement>(null);
    const containerRef = React.useRef<HTMLSpanElement>(null);
    const triggerWrapperRef = React.useRef<HTMLDivElement>(null);
    const dropdownRef = React.useRef<HTMLDivElement>(null);
    const [open, setOpen] = React.useState(false);
    const [openUpward, setOpenUpward] = React.useState(false);
    const [inputValue, setInputValue] = React.useState("");
    const measureRef = React.useRef<HTMLDivElement>(null);
    const [visibleCount, setVisibleCount] = React.useState(maxVisibleBadges ?? 1);

    React.useEffect(() => {
      if (open && triggerWrapperRef.current) {
        requestAnimationFrame(() => {
          if (!triggerWrapperRef.current) return;
          const rect = triggerWrapperRef.current.getBoundingClientRect();
          const spaceBelow = window.innerHeight - rect.bottom;
          const dropdownHeight = dropdownRef.current?.offsetHeight ?? 0;
          setOpenUpward(spaceBelow < dropdownHeight + 8);
        });
      } else {
        setOpenUpward(false);
      }
    }, [open]);

    React.useEffect(() => {
      if (open && dropdownRef.current) {
        requestAnimationFrame(() => {
          const scrollableElement = dropdownRef.current?.querySelector("[cmdk-group]");
          if (scrollableElement) {
            scrollableElement.scrollTop = 0;
          }
        });
      }
    }, [open]);

    React.useEffect(() => {
      if (maxVisibleBadges !== undefined) {
        setVisibleCount(maxVisibleBadges);
        return;
      }

      if (value.length === 0) {
        setVisibleCount(0);
        return;
      }

      const container = containerRef.current;
      const measureContainer = measureRef.current;
      if (!container || !measureContainer) return;

      const gap = parseFloat(getComputedStyle(container).gap) || 4;
      const input = container.querySelector("input");
      const inputReserve = input
        ? (parseFloat(getComputedStyle(input).minWidth) || 0) +
          (parseFloat(getComputedStyle(input).marginLeft) || 0)
        : 0;

      const calculate = () => {
        const containerWidth = container.clientWidth;
        if (containerWidth === 0) return;

        const badges = Array.from(measureContainer.children) as HTMLElement[];
        const availableWidth = containerWidth - inputReserve;

        if (availableWidth <= 0 || badges.length === 0) {
          setVisibleCount(1);
          return;
        }

        const overflowBadge = badges[value.length];
        const overflowBadgeWidth = overflowBadge ? overflowBadge.offsetWidth : 40;

        let usedWidth = 0;
        let count = 0;

        for (let i = 0; i < value.length && i < badges.length; i++) {
          const badgeWidth = badges[i].offsetWidth;
          const gapWidth = count > 0 ? gap : 0;
          const newTotal = usedWidth + badgeWidth + gapWidth;

          const remainingItems = value.length - count - 1;
          const needsOverflow = remainingItems > 0;
          const maxAllowed = needsOverflow
            ? availableWidth - overflowBadgeWidth - gap
            : availableWidth;

          if (newTotal > maxAllowed && count > 0) break;

          usedWidth = newTotal;
          count++;
        }

        setVisibleCount(Math.max(1, count));
      };

      requestAnimationFrame(calculate);

      const observer = new ResizeObserver(calculate);
      observer.observe(container);

      return () => observer.disconnect();
    }, [value, maxVisibleBadges, density]);

    const handleUnselect = React.useCallback(
      (option: Option) => {
        onValueChange?.(value.filter((item) => item.value !== option.value));
      },
      [onValueChange, value],
    );

    const handleKeyDown = React.useCallback(
      (e: React.KeyboardEvent<HTMLDivElement>) => {
        const input = inputRef.current;
        if (input) {
          if (e.key === "Delete" || e.key === "Backspace") {
            if (input.value === "" && value.length > 0) {
              const lastValue = value[value.length - 1];
              handleUnselect(lastValue);
            }
          }
          if (e.key === "Escape") {
            input.blur();
          }
        }
      },
      [value, handleUnselect],
    );

    const isSelected = React.useCallback(
      (option: Option) => value.some((item) => item.value === option.value),
      [value],
    );

    const toggleOption = React.useCallback(
      (option: Option) => {
        if (isSelected(option)) {
          handleUnselect(option);
        } else {
          onValueChange?.([...value, option]);
        }
      },
      [isSelected, handleUnselect, onValueChange, value],
    );

    return (
      <Command
        ref={ref}
        onKeyDown={handleKeyDown}
        className={cn(
          "overflow-visible bg-transparent h-auto flex-row rounded-none border-0 shadow-none p-0",
          className,
        )}
        {...commandProps}
      >
        <div ref={triggerWrapperRef} className="relative w-full">
          {maxVisibleBadges === undefined && value.length > 0 && (
            <div
              ref={measureRef}
              aria-hidden="true"
              style={{
                position: "absolute",
                visibility: "hidden",
                pointerEvents: "none",
                display: "flex",
                gap: "4px",
                top: 0,
                left: 0,
              }}
            >
              {value.map((option) => (
                <Badge
                  key={`measure-${option.value}`}
                  variant="secondary"
                  className={cn(badgeVariant({ density }), "shrink-0")}
                >
                  {option.label}
                  <span className="ml-1 p-1 h-3" style={{ display: "inline-flex" }}>
                    <X className={xIconVariant({ density })} />
                  </span>
                </Badge>
              ))}
              <Badge
                variant="outline"
                className={cn(badgeVariant({ density }), "bg-muted text-muted-foreground shrink-0")}
              >
                +{Math.max(1, value.length - 1)}
              </Badge>
            </div>
          )}
          <div
            className={cn(
              multipleSelectorVariant({ density }),
              disabled && "cursor-not-allowed opacity-50",
              (!value || value.length === 0) && "text-muted-foreground",
              invalid
                ? "border-destructive text-destructive-foreground focus-within:ring-destructive focus-within:border-destructive"
                : undefined,
              ghost &&
                "border-transparent shadow-none bg-transparent hover:bg-accent hover:text-accent-foreground dark:border-transparent dark:bg-transparent dark:hover:bg-accent dark:hover:text-accent-foreground",
            )}
          >
            <span
              ref={containerRef}
              className="flex gap-1 items-center flex-1 min-w-0 overflow-hidden"
            >
              {value.slice(0, visibleCount).map((option) => (
                <Badge
                  key={option.value}
                  variant="secondary"
                  className={cn(
                    badgeVariant({ density }),
                    "shrink-0",
                    invalid && "bg-destructive/10 border-destructive text-destructive",
                  )}
                >
                  {option.label}
                  <button
                    type="button"
                    tabIndex={-1}
                    aria-label="Remove"
                    className="ml-1 p-0.5 rounded-sm hover:bg-black/10 dark:hover:bg-white/10 focus:outline-none cursor-pointer flex items-center justify-center opacity-70 hover:opacity-100 transition-opacity"
                    onKeyDown={(e) => {
                      if (e.key === "Enter" || e.key === " ") {
                        e.preventDefault();
                        handleUnselect(option);
                      }
                    }}
                    onMouseDown={(e) => {
                      e.preventDefault();
                      e.stopPropagation();
                    }}
                    onClick={() => handleUnselect(option)}
                  >
                    <X className={xIconVariant({ density })} />
                  </button>
                </Badge>
              ))}
              {value.length > visibleCount && (
                <Badge
                  variant="outline"
                  className={cn(
                    badgeVariant({ density }),
                    "bg-muted text-muted-foreground shrink-0",
                  )}
                >
                  +{value.length - visibleCount}
                </Badge>
              )}
              <CommandPrimitive.Input
                ref={inputRef}
                value={inputValue}
                onValueChange={setInputValue}
                onBlur={() => {
                  setOpen(false);
                  if (containerRef.current) {
                    containerRef.current.scrollLeft = 0;
                  }
                }}
                onFocus={() => {
                  setOpen(true);
                  requestAnimationFrame(() => {
                    if (containerRef.current) {
                      containerRef.current.scrollLeft = 0;
                    }
                  });
                }}
                placeholder={
                  hidePlaceholderWhenSelected && value.length > 0 ? undefined : placeholder
                }
                disabled={disabled}
                readOnly={!open}
                className="ml-2 bg-transparent outline-none placeholder:text-muted-foreground flex-1 min-w-[120px] cursor-pointer"
              />
            </span>
            <ChevronDown
              className={cn(
                "h-4 w-4 ml-2 opacity-50 shrink-0 cursor-pointer",
                disabled && "cursor-not-allowed opacity-50",
              )}
              onClick={(e) => {
                if (disabled) return;
                e.preventDefault();
                e.stopPropagation();
                if (inputRef.current) {
                  if (open) {
                    inputRef.current.blur();
                  } else {
                    inputRef.current.focus();
                  }
                }
              }}
              onKeyDown={(e) => {
                if (disabled) return;
                if (e.key === "Enter" || e.key === " ") {
                  e.preventDefault();
                  e.stopPropagation();
                  if (inputRef.current) {
                    if (open) {
                      inputRef.current.blur();
                    } else {
                      inputRef.current.focus();
                    }
                  }
                }
              }}
              role="button"
              tabIndex={disabled ? -1 : 0}
              aria-label="Toggle dropdown"
            />
          </div>
          {open && defaultOptions.length > 0 && (
            <div
              ref={dropdownRef}
              className={cn(
                "absolute w-full z-50 rounded-box border bg-popover text-popover-foreground shadow-md outline-none animate-in",
                openUpward ? "bottom-full mb-1" : "top-full mt-1",
              )}
            >
              <CommandGroup className="h-full overflow-auto max-h-[300px]">
                {defaultOptions.map((option) => {
                  const selected = isSelected(option);
                  return (
                    <CommandItem
                      key={option.value}
                      onMouseDown={(e) => {
                        e.preventDefault();
                        e.stopPropagation();
                      }}
                      onSelect={() => {
                        setInputValue("");
                        toggleOption(option);
                      }}
                      className={cn(
                        menuItemVariant({ density }),
                        "flex items-center justify-between",
                      )}
                      disabled={option.disable}
                    >
                      {option.tooltip ? (
                        <TooltipProvider>
                          <Tooltip delayDuration={300}>
                            <TooltipTrigger asChild>
                              <div className="flex items-center justify-between w-full">
                                <span>{option.label}</span>
                                {selected && (
                                  <X
                                    className={cn(
                                      xIconVariant({ density }),
                                      "text-muted-foreground hover:text-foreground",
                                    )}
                                  />
                                )}
                              </div>
                            </TooltipTrigger>
                            <TooltipContent>{option.tooltip}</TooltipContent>
                          </Tooltip>
                        </TooltipProvider>
                      ) : (
                        <>
                          <span>{option.label}</span>
                          {selected && (
                            <X
                              className={cn(
                                xIconVariant({ density }),
                                "text-muted-foreground hover:text-foreground",
                              )}
                            />
                          )}
                        </>
                      )}
                    </CommandItem>
                  );
                })}
              </CommandGroup>
            </div>
          )}
          {open && defaultOptions.length === 0 && emptyIndicator && (
            <div
              className={cn(
                "absolute w-full z-50 rounded-box border bg-popover text-popover-foreground shadow-md outline-none p-2",
                openUpward ? "bottom-full mb-1" : "top-full mt-1",
              )}
            >
              {emptyIndicator}
            </div>
          )}
        </div>
      </Command>
    );
  },
);

MultipleSelector.displayName = "MultipleSelector";

export { MultipleSelector };
