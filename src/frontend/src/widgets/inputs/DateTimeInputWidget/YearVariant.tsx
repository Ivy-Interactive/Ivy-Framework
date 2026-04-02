import * as React from "react";
import { useState, useCallback, useMemo } from "react";
import { Button } from "@/components/ui/button";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { format } from "date-fns";
import { Calendar as CalendarIcon, ChevronLeft, ChevronRight } from "lucide-react";
import { cn } from "@/lib/utils";
import { inputStyles } from "@/lib/styles";
import { Densities } from "@/types/density";
import {
  dateTimeInputVariant,
  dateTimeInputIconVariant,
  dateTimeInputTextVariant,
} from "@/components/ui/input/date-time-input-variant";
import { YearVariantProps } from "./types";
import { ClearAndInvalidIcons } from "./shared";

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
}) => {
  const [open, setOpen] = useState(false);
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

  React.useEffect(() => {
    if (date) setDecadeStart(getDecadeStart(date.getFullYear()));
  }, [date]);

  const showClear = nullable && !disabled && value != null && value !== "";

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
            disabled={disabled}
            variant="outline"
            autoFocus={autoFocus}
            data-slot="calendar"
            className={cn(
              dateTimeInputVariant({ density }),
              "dark:bg-white/5 dark:hover:bg-white/10 dark:border-white/10",
              !date && "text-muted-foreground",
              invalid && inputStyles.invalidInput,
              disabled && "cursor-not-allowed",
              showClear && invalid ? "pr-16" : showClear || invalid ? "pr-8" : "",
            )}
            data-testid={dataTestId}
            onFocus={() => {
              if (!open) onFocusChange?.(true);
            }}
            onBlur={() => {
              if (!open) onFocusChange?.(false);
            }}
          >
            <CalendarIcon className={cn("mr-2 shrink-0", dateTimeInputIconVariant({ density }))} />
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
                className="h-7 w-7"
                onClick={() => setDecadeStart((d) => d - 10)}
              >
                <ChevronLeft className="h-4 w-4" />
              </Button>
              <span className="text-sm font-medium select-none">
                {decadeStart} – {decadeStart + 9}
              </span>
              <Button
                variant="ghost"
                size="icon"
                className="h-7 w-7"
                onClick={() => setDecadeStart((d) => d + 10)}
              >
                <ChevronRight className="h-4 w-4" />
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
      <ClearAndInvalidIcons
        showClear={showClear}
        invalid={invalid}
        density={density}
        onClear={handleClear}
      />
    </div>
  );
};
