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
  const inputRef = React.useRef<HTMLInputElement>(null);
  const hasAutoFocusedRef = React.useRef(false);

  React.useEffect(() => {
    if (autoFocus && !disabled && !hasAutoFocusedRef.current) {
      hasAutoFocusedRef.current = true;
      inputRef.current?.focus();
    }
  }, [autoFocus, disabled]);

  // Use local state for the input value to make it uncontrolled
  const deriveTimeValue = useCallback(
    (val: string | undefined | null) => {
      if (val && typeof val === "string") {
        const date = new Date(val);
        if (!isNaN(date.getTime())) {
          return format(date, "HH:mm:ss");
        }
        if (/^\d{1,2}:\d{2}(:\d{2})?$/.test(val)) {
          return val.split(":").length === 2 ? val + ":00" : val;
        }
      }
      return nullable && (val === undefined || val === null || val === "") ? "" : "00:00:00";
    },
    [nullable],
  );

  const [localTimeValue, setLocalTimeValue] = useState(() => deriveTimeValue(value));
  const [prevValue, setPrevValue] = useState(value);

  if (value !== prevValue) {
    setPrevValue(value);
    setLocalTimeValue(deriveTimeValue(value));
  }

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
          ref={inputRef}
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
