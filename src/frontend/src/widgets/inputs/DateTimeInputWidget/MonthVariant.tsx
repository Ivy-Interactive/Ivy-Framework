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
import { MonthVariantProps } from "./types";
import { ClearAndInvalidIcons } from "./shared";

const MONTHS = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

export const MonthVariant: React.FC<MonthVariantProps> = ({
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
  "data-testid": dataTestId,
}) => {
  const [open, setOpen] = useState(false);
  const date = useMemo(() => (value ? new Date(value) : undefined), [value]);

  const minDate = useMemo(() => (min ? new Date(min) : undefined), [min]);
  const maxDate = useMemo(() => (max ? new Date(max) : undefined), [max]);

  const [viewYear, setViewYear] = useState(() =>
    date ? date.getFullYear() : new Date().getFullYear(),
  );

  React.useEffect(() => {
    if (date) setViewYear(date.getFullYear());
  }, [date]);

  const showClear = nullable && !disabled && value != null && value !== "";

  const isMonthDisabled = (monthIndex: number) => {
    if (minDate && new Date(viewYear, monthIndex + 1, 0) < minDate) return true;
    if (maxDate && new Date(viewYear, monthIndex, 1) > maxDate) return true;
    return false;
  };

  const handleClear = (e?: React.MouseEvent) => {
    e?.preventDefault();
    e?.stopPropagation();
    onDateChange(undefined);
  };

  const handleMonthSelect = useCallback(
    (monthIndex: number) => {
      onDateChange(new Date(viewYear, monthIndex, 1));
      setOpen(false);
    },
    [viewYear, onDateChange],
  );

  const isSelected = (monthIndex: number) =>
    date !== undefined && date.getFullYear() === viewYear && date.getMonth() === monthIndex;

  const isCurrentMonth = (monthIndex: number) => {
    const now = new Date();
    return now.getFullYear() === viewYear && now.getMonth() === monthIndex;
  };

  return (
    <div className="relative w-full select-none">
      <Popover open={open} onOpenChange={setOpen}>
        <PopoverTrigger asChild>
          <Button
            disabled={disabled}
            variant="outline"
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
          >
            <CalendarIcon className={cn("mr-2 shrink-0", dateTimeInputIconVariant({ density }))} />
            <span
              className={cn(
                "truncate",
                dateTimeInputTextVariant({ density }),
                !date && "text-muted-foreground",
              )}
            >
              {date ? format(date, formatProp || "MMM yyyy") : placeholder || "Pick a month"}
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
                onClick={() => setViewYear((y) => y - 1)}
              >
                <ChevronLeft className="h-4 w-4" />
              </Button>
              <span className="text-sm font-medium select-none">{viewYear}</span>
              <Button
                variant="ghost"
                size="icon"
                className="h-7 w-7"
                onClick={() => setViewYear((y) => y + 1)}
              >
                <ChevronRight className="h-4 w-4" />
              </Button>
            </div>
            <div className="grid grid-cols-4 gap-1">
              {MONTHS.map((month, i) => {
                const monthDisabled = isMonthDisabled(i);
                return (
                  <Button
                    key={month}
                    variant="ghost"
                    size="sm"
                    disabled={monthDisabled}
                    className={cn(
                      "h-9 w-full text-sm font-normal",
                      isSelected(i) &&
                        "bg-primary text-primary-foreground hover:bg-primary hover:text-primary-foreground",
                      !isSelected(i) && isCurrentMonth(i) && "bg-accent text-accent-foreground",
                      monthDisabled && "opacity-50 cursor-not-allowed",
                    )}
                    onClick={() => handleMonthSelect(i)}
                  >
                    {month}
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
