import * as React from "react";
import { useState, useCallback, useMemo } from "react";
import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { format, startOfISOWeek, getISOWeek } from "date-fns";
import { Calendar as CalendarIcon } from "lucide-react";
import { cn } from "@/lib/utils";
import { inputStyles } from "@/lib/styles";
import { Densities } from "@/types/density";
import {
  dateTimeInputVariant,
  dateTimeInputIconVariant,
  dateTimeInputTextVariant,
} from "@/components/ui/input/date-time-input-variant";
import { WeekVariantProps } from "./types";
import { ClearAndInvalidIcons } from "./shared";

function formatWeekDisplay(date: Date, formatProp?: string): string {
  if (formatProp) return format(date, formatProp);
  const weekNum = getISOWeek(date);
  const year = date.getFullYear();
  return `Week ${weekNum}, ${year}`;
}

export const WeekVariant: React.FC<WeekVariantProps> = ({
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

  const selectedMonday = useMemo(() => (date ? startOfISOWeek(date) : undefined), [date]);

  const minDate = useMemo(() => (min ? new Date(min) : undefined), [min]);
  const maxDate = useMemo(() => (max ? new Date(max) : undefined), [max]);

  const disabledDays = useMemo(() => {
    const matchers: Array<{ before: Date } | { after: Date }> = [];
    if (minDate) matchers.push({ before: minDate });
    if (maxDate) matchers.push({ after: maxDate });
    return matchers;
  }, [minDate, maxDate]);

  const showClear = nullable && !disabled && value != null && value !== "";

  const handleClear = (e?: React.MouseEvent) => {
    e?.preventDefault();
    e?.stopPropagation();
    onDateChange(undefined);
  };

  const handleSelect = useCallback(
    (selectedDate: Date | undefined) => {
      if (!selectedDate) {
        onDateChange(undefined);
      } else {
        onDateChange(startOfISOWeek(selectedDate));
      }
      setOpen(false);
    },
    [onDateChange],
  );

  const weekModifiers = useMemo(() => {
    if (!selectedMonday) return {};
    const days: Date[] = [];
    for (let i = 0; i < 7; i++) {
      const d = new Date(selectedMonday);
      d.setDate(d.getDate() + i);
      days.push(d);
    }
    return { selectedWeek: days };
  }, [selectedMonday]);

  const weekModifiersClassNames = useMemo(
    () => ({
      selectedWeek:
        "bg-accent text-accent-foreground rounded-none [&:first-child]:rounded-l-md [&:last-child]:rounded-r-md",
    }),
    [],
  );

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
              {date ? formatWeekDisplay(date, formatProp) : placeholder || "Pick a week"}
            </span>
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="start">
          <Calendar
            mode="single"
            selected={selectedMonday}
            onSelect={handleSelect}
            disabled={disabledDays.length > 0 ? disabledDays : undefined}
            showWeekNumber
            weekStartsOn={1}
            ISOWeek
            initialFocus
            density={density}
            modifiers={weekModifiers}
            modifiersClassNames={weekModifiersClassNames}
          />
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
