import * as React from "react";
import { useState, useCallback, useMemo } from "react";
import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import { Input } from "@/components/ui/input";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { format } from "date-fns";
import { Calendar as CalendarIcon, Clock } from "lucide-react";
import { cn } from "@/lib/utils";
import { inputStyles } from "@/lib/styles";
import { Densities } from "@/types/density";
import {
  dateTimeInputVariant,
  dateTimeInputIconVariant,
  dateTimeInputTextVariant,
} from "@/components/ui/input/date-time-input-variant";
import { DateTimeVariantProps } from "./types";
import { ClearAndInvalidIcons } from "./shared";
import { useTimeConstraints } from "./useTimeConstraints";

export const DateTimeVariant: React.FC<DateTimeVariantProps> = ({
  value,
  placeholder,
  disabled,
  nullable,
  invalid,
  onDateChange,
  onTimeChange,
  format: formatProp,
  firstDayOfWeek,
  min,
  max,
  step,
  density = Densities.Medium,
  "data-testid": dataTestId,
  onFocusChange,
}) => {
  const [open, setOpen] = useState(false);
  const date = useMemo(() => (value ? new Date(value) : undefined), [value]);
  const minDate = useMemo(() => (min ? new Date(min) : undefined), [min]);
  const maxDate = useMemo(() => (max ? new Date(max) : undefined), [max]);
  const showClear = nullable && !disabled && value != null && value !== "";

  const disabledDays = useMemo(() => {
    const matchers: Array<{ before: Date } | { after: Date }> = [];
    if (minDate) matchers.push({ before: minDate });
    if (maxDate) matchers.push({ after: maxDate });
    return matchers;
  }, [minDate, maxDate]);

  const { timeStepSeconds, timeMin, timeMax, getSnappedTime } = useTimeConstraints(min, max, step);

  const handleClear = (e?: React.MouseEvent) => {
    e?.preventDefault();
    e?.stopPropagation();
    onDateChange(undefined);
  };

  // Use local state for the time input to make it uncontrolled
  const [localTimeValue, setLocalTimeValue] = useState(() => {
    if (date) {
      return format(date, formatProp || "HH:mm:ss");
    }
    return "00:00:00";
  });

  // Track if user is actively editing the time input
  const [isEditingTime, setIsEditingTime] = useState(false);

  // Update local time when date changes, but only if user is not actively editing
  React.useEffect(() => {
    if (!isEditingTime) {
      if (date) {
        const newTimeValue = format(date, formatProp || "HH:mm:ss");
        setLocalTimeValue(newTimeValue);
      } else if (nullable) {
        // When nullable and no date, keep input empty instead of defaulting to '00:00:00'
        setLocalTimeValue("");
      } else {
        setLocalTimeValue("00:00:00");
      }
    }
  }, [date, formatProp, isEditingTime, nullable]);

  const handleDateSelect = useCallback(
    (selectedDate: Date | undefined) => {
      if (selectedDate) {
        // Preserve the time when selecting a new date
        const currentTime = date ? date : new Date();
        const newDateTime = new Date(selectedDate);
        newDateTime.setHours(
          currentTime.getHours(),
          currentTime.getMinutes(),
          currentTime.getSeconds(),
        );
        onDateChange(newDateTime);
      } else {
        onDateChange(undefined);
      }
      // Do not close the popover here, allow user to pick time
    },
    [date, onDateChange],
  );

  const handleTimeChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const newTimeValue = e.target.value;
    setLocalTimeValue(newTimeValue);
    setIsEditingTime(true);
  }, []);

  const commitSnappedTime = useCallback(() => {
    const out = getSnappedTime(localTimeValue);
    setLocalTimeValue(out);

    if (/^\d{1,2}:\d{2}(:\d{2})?$/.test(out)) {
      const parts = out.split(":").map(Number);
      const hours = parts[0];
      const minutes = parts[1];
      const seconds = parts[2] || 0;

      const newDateTime = date ? new Date(date) : new Date();
      newDateTime.setHours(hours, minutes, seconds);
      onDateChange(newDateTime);
    }
    onTimeChange(out);
  }, [getSnappedTime, localTimeValue, date, onDateChange, onTimeChange]);

  const flushTimeInput = useCallback(() => {
    setIsEditingTime(false);
    if (!localTimeValue?.trim()) {
      onTimeChange(localTimeValue ?? "");
      return;
    }
    commitSnappedTime();
  }, [localTimeValue, onTimeChange, commitSnappedTime]);

  const handleOpenChange = useCallback(
    (newOpen: boolean) => {
      if (!newOpen && open) {
        // Dismissal often closes the popover without firing blur on the time field
        flushTimeInput();
      }
      setOpen(newOpen);
      onFocusChange?.(newOpen);
    },
    [open, flushTimeInput, onFocusChange],
  );

  const handleTimeBlur = useCallback(() => {
    flushTimeInput();
  }, [flushTimeInput]);

  const handleTimeKeyDown = useCallback((e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter") {
      e.preventDefault();
      e.currentTarget.blur();
    }
  }, []);

  const handleTimeFocus = useCallback(() => {
    setIsEditingTime(true);
  }, []);

  return (
    <div className="relative w-full select-none">
      <Popover open={open} onOpenChange={handleOpenChange}>
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
            onFocus={() => {
              if (!open) onFocusChange?.(true);
            }}
            onBlur={() => {
              if (!open) onFocusChange?.(false);
            }}
          >
            <CalendarIcon className={cn("mr-2 shrink-0", dateTimeInputIconVariant({ density }))} />
            <Clock className={cn("mr-2 shrink-0", dateTimeInputIconVariant({ density }))} />
            <span
              className={cn(
                "truncate",
                dateTimeInputTextVariant({ density }),
                !date && "text-muted-foreground",
              )}
            >
              {date
                ? format(date, formatProp || "yyyy-MM-dd")
                : placeholder || "Pick a date & time"}
            </span>
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="start">
          <div className="flex flex-col gap-2 p-2">
            <Calendar
              mode="single"
              selected={date}
              onSelect={handleDateSelect}
              disabled={disabledDays.length > 0 ? disabledDays : undefined}
              initialFocus
              weekStartsOn={firstDayOfWeek}
              density={density}
            />
            <div className="flex items-center gap-2">
              <Clock
                className={cn(dateTimeInputIconVariant({ density }), "text-muted-foreground")}
              />
              <Input
                type="time"
                step={timeStepSeconds}
                min={timeMin}
                max={timeMax}
                value={localTimeValue}
                onChange={handleTimeChange}
                onFocus={handleTimeFocus}
                onBlur={handleTimeBlur}
                onKeyDown={handleTimeKeyDown}
                disabled={disabled}
                className={cn(
                  "bg-transparent appearance-none [&::-webkit-calendar-picker-indicator]:hidden",
                  dateTimeInputTextVariant({ density }),
                  invalid && inputStyles.invalidInput,
                )}
                data-testid={dataTestId ? `${dataTestId}-time` : undefined}
              />
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
