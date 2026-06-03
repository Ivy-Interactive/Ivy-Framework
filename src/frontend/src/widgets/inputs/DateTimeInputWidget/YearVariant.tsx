import * as React from "react";
import { useState, useCallback, useMemo, useEffect, useRef } from "react";
import { Button } from "@/components/ui/button";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { format } from "date-fns";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { cn } from "@/lib/utils";
import { inputStyles } from "@/lib/styles";
import { Densities } from "@/types/density";
import {
  dateTimeInputVariant,
  dateTimeInputTextVariant,
} from "@/components/ui/input/date-time-input-variant";
import { YearVariantProps } from "./types";
import { ClearAndInvalidIcons } from "./shared";
import { dateInputControlInvalid, dateInputTriggerTrailingPadding } from "./affix";

function getDecadeStart(year: number): number {
  return Math.floor(year / 10) * 10;
}

export const YearVariant: React.FC<YearVariantProps> = ({
  value,
  placeholder,
  disabled,
  nullable,
  invalid,
  onDateChange,
  format: formatProp,
  min,
  max,
  density = Densities.Medium,
  autoFocus,
  "data-testid": dataTestId,
  onFocusChange,
  inAffixShell,
  trailingBesideSuffix,
}) => {
  const [open, setOpen] = useState(false);

  const hasAutoFocusedRef = useRef(false);
  const buttonRef = useRef<HTMLButtonElement>(null);
  useEffect(() => {
    if (autoFocus && !disabled && !hasAutoFocusedRef.current) {
      hasAutoFocusedRef.current = true;
      buttonRef.current?.focus();
      setOpen(true);
    }
  }, [autoFocus, disabled]);
  const date = useMemo(() => (value ? new Date(value) : undefined), [value]);

  const handleOpenChange = useCallback(
    (newOpen: boolean) => {
      setOpen(newOpen);
      onFocusChange?.(newOpen);
    },
    [onFocusChange],
  );

  const minDate = useMemo(() => (min ? new Date(min) : undefined), [min]);
  const maxDate = useMemo(() => (max ? new Date(max) : undefined), [max]);

  const [decadeStart, setDecadeStart] = useState(() =>
    getDecadeStart(date ? date.getFullYear() : new Date().getFullYear()),
  );
  const prevYearRef = useRef(date?.getFullYear());

  if (date?.getFullYear() !== prevYearRef.current) {
    prevYearRef.current = date?.getFullYear();
    if (date) {
      setDecadeStart(getDecadeStart(date.getFullYear()));
    }
  }

  const showClear = nullable && !disabled && value != null && value !== "";
  const controlInvalid = dateInputControlInvalid(
    inAffixShell,
    trailingBesideSuffix,
    showClear,
    invalid,
  );
  const trailingPadding = dateInputTriggerTrailingPadding(
    inAffixShell,
    trailingBesideSuffix,
    showClear,
    invalid,
  );

  const handleClear = (e?: React.MouseEvent) => {
    e?.preventDefault();
    e?.stopPropagation();
    onDateChange(undefined);
  };

  const handleYearSelect = useCallback(
    (year: number) => {
      onDateChange(new Date(year, 0, 1));
      setOpen(false);
    },
    [onDateChange],
  );

  const years = useMemo(() => {
    const result: number[] = [];
    for (let i = decadeStart - 1; i <= decadeStart + 10; i++) {
      result.push(i);
    }
    return result;
  }, [decadeStart]);

  const selectedYear = date?.getFullYear();
  const currentYear = new Date().getFullYear();

  const isYearDisabled = (year: number) => {
    if (minDate && year < minDate.getFullYear()) return true;
    if (maxDate && year > maxDate.getFullYear()) return true;
    return false;
  };

  return (
    <div className="relative w-full select-none">
      <Popover open={open} onOpenChange={handleOpenChange}>
        <PopoverTrigger asChild>
          <Button
            ref={buttonRef}
            disabled={disabled}
            variant="outline"
            data-slot="calendar"
            className={cn(
              dateTimeInputVariant({ density }),
              !date && "text-muted-foreground",
              controlInvalid && inputStyles.invalidInput,
              disabled && "cursor-not-allowed",
              trailingPadding,
              "border-0 bg-transparent shadow-none hover:bg-transparent focus-visible:ring-0 focus-visible:ring-offset-0 dark:border-transparent dark:bg-transparent dark:hover:bg-transparent",
            )}
            data-testid={dataTestId}
            onFocus={() => {
              if (!open) onFocusChange?.(true);
            }}
            onBlur={() => {
              if (!open) onFocusChange?.(false);
            }}
          >
            <span
              className={cn(
                "truncate",
                dateTimeInputTextVariant({ density }),
                !date && "text-muted-foreground",
              )}
            >
              {date ? format(date, formatProp || "yyyy") : placeholder || "Pick a year"}
            </span>
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="start">
          <div className="p-3 w-[280px]">
            <div className="flex items-center justify-between mb-3">
              <Button
                variant="ghost"
                size="icon"
                className="size-7"
                aria-label="Previous decade"
                onClick={() => setDecadeStart((d) => d - 10)}
              >
                <ChevronLeft className="size-4" />
              </Button>
              <span className="text-sm font-medium select-none">
                {decadeStart} – {decadeStart + 9}
              </span>
              <Button
                variant="ghost"
                size="icon"
                className="size-7"
                aria-label="Next decade"
                onClick={() => setDecadeStart((d) => d + 10)}
              >
                <ChevronRight className="size-4" />
              </Button>
            </div>
            <div className="grid grid-cols-4 gap-1">
              {years.map((year) => {
                const isOutside = year < decadeStart || year > decadeStart + 9;
                const yearDisabled = isYearDisabled(year);
                return (
                  <Button
                    key={year}
                    variant="ghost"
                    size="sm"
                    disabled={yearDisabled}
                    className={cn(
                      "h-9 w-full text-sm font-normal",
                      isOutside && "text-muted-foreground",
                      year === selectedYear &&
                        "bg-primary text-primary-foreground hover:bg-primary hover:text-primary-foreground",
                      year !== selectedYear &&
                        year === currentYear &&
                        "bg-accent text-accent-foreground",
                      yearDisabled && "opacity-50 cursor-not-allowed",
                    )}
                    onClick={() => handleYearSelect(year)}
                  >
                    {year}
                  </Button>
                );
              })}
            </div>
          </div>
        </PopoverContent>
      </Popover>
      {!inAffixShell && (
        <ClearAndInvalidIcons
          showClear={showClear}
          invalid={invalid}
          density={density}
          onClear={handleClear}
        />
      )}
    </div>
  );
};
