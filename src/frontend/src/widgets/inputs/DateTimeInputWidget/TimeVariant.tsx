import * as React from "react";
import { useState, useCallback } from "react";
import { Input } from "@/components/ui/input";
import { format } from "date-fns";
import { Clock } from "lucide-react";
import { cn } from "@/lib/utils";
import { inputStyles } from "@/lib/styles";
import { Densities } from "@/types/density";
import {
  dateTimeInputIconVariant,
  dateTimeInputTextVariant,
} from "@/components/ui/input/date-time-input-variant";
import { TimeVariantProps } from "./types";
import { ClearAndInvalidIcons } from "./shared";
import { useTimeConstraints } from "./useTimeConstraints";

export const TimeVariant: React.FC<TimeVariantProps> = ({
  value,
  placeholder,
  disabled,
  nullable,
  invalid,
  onTimeChange,
  min,
  max,
  step,
  density = Densities.Medium,
  autoFocus,
  "data-testid": dataTestId,
  onFocusChange,
}) => {
  // Use local state for the input value to make it uncontrolled
  const [localTimeValue, setLocalTimeValue] = useState(() => {
    if (value && typeof value === "string") {
      const date = new Date(value);
      if (!isNaN(date.getTime())) {
        return format(date, "HH:mm:ss");
      }
      // Fallback for simple time strings that new Date() might fail on
      if (/^\d{1,2}:\d{2}(:\d{2})?$/.test(value)) {
        return value.split(":").length === 2 ? value + ":00" : value;
      }
    }
    // When nullable and no value, return empty string to show placeholder
    return nullable && (value === undefined || value === null || value === "") ? "" : "00:00:00";
  });

  // Update local state when value prop changes (from parent)
  React.useEffect(() => {
    if (value && typeof value === "string") {
      const date = new Date(value);
      if (!isNaN(date.getTime())) {
        const newTimeValue = format(date, "HH:mm:ss");
        setLocalTimeValue(newTimeValue);
      } else if (/^\d{1,2}:\d{2}(:\d{2})?$/.test(value)) {
        // Handle direct time strings
        const newTimeValue = value.split(":").length === 2 ? value + ":00" : value;
        setLocalTimeValue(newTimeValue);
      }
    } else if (nullable && (value === undefined || value === null || value === "")) {
      setLocalTimeValue("");
    } else {
      // When not nullable and no value, default to '00:00:00'
      setLocalTimeValue("00:00:00");
    }
  }, [value, nullable]);

  const { timeStepSeconds, timeMin, timeMax, getSnappedTime } = useTimeConstraints(min, max, step);

  const showClear = nullable && !disabled && value != null && value !== "";

  const handleClear = (e?: React.MouseEvent) => {
    e?.preventDefault();
    e?.stopPropagation();
    onTimeChange("");
  };

  const handleTimeChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const newTimeValue = e.target.value;
    setLocalTimeValue(newTimeValue);
  }, []);

  const commitSnappedTime = useCallback(() => {
    if (nullable && localTimeValue.trim() === "") {
      onTimeChange("");
      return;
    }

    const out = getSnappedTime(localTimeValue);
    setLocalTimeValue(out);
    onTimeChange(out);
  }, [nullable, localTimeValue, getSnappedTime, onTimeChange]);

  const handleTimeBlur = useCallback(() => {
    commitSnappedTime();
    onFocusChange?.(false);
  }, [commitSnappedTime, onFocusChange]);

  const handleKeyDown = useCallback((e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter") {
      e.preventDefault();
      e.currentTarget.blur();
    }
  }, []);

  return (
    <div className="relative w-full select-none" data-testid={dataTestId}>
      <div
        className={cn(
          "relative flex items-center rounded-md border border-input bg-transparent shadow-sm focus-within:ring-1 focus-within:ring-ring dark:bg-white/5 dark:border-white/10",
          invalid && inputStyles.invalidInput,
        )}
      >
        <Clock
          className={cn(
            "ml-3 shrink-0",
            dateTimeInputIconVariant({ density }),
            disabled && "opacity-50",
          )}
        />
        <Input
          type="time"
          step={timeStepSeconds}
          min={timeMin}
          max={timeMax}
          density={density}
          value={localTimeValue}
          onChange={handleTimeChange}
          onFocus={() => onFocusChange?.(true)}
          onBlur={handleTimeBlur}
          onKeyDown={handleKeyDown}
          disabled={disabled}
          autoFocus={autoFocus}
          placeholder={placeholder || "Select time"}
          className={cn(
            "bg-transparent appearance-none [&::-webkit-calendar-picker-indicator]:hidden cursor-pointer w-full border-0 shadow-none focus-visible:ring-0",
            dateTimeInputTextVariant({ density }),
            invalid && inputStyles.invalidInput,
            disabled && "cursor-not-allowed opacity-50 text-muted-foreground",
            showClear && invalid ? "pr-16" : showClear || invalid ? "pr-8" : "",
          )}
        />
      </div>
      <ClearAndInvalidIcons
        showClear={showClear}
        invalid={invalid}
        density={density}
        onClear={handleClear}
      />
    </div>
  );
};
