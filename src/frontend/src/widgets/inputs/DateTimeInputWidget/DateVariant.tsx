import * as React from "react";
import { useState, useCallback, useMemo, useEffect, useRef } from "react";
import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { format } from "date-fns";
import { cn } from "@/lib/utils";
import { inputStyles } from "@/lib/styles";
import { Densities } from "@/types/density";
import {
  dateTimeInputVariant,
  dateTimeInputTextVariant,
} from "@/components/ui/input/date-time-input-variant";
import { DateVariantProps } from "./types";
import { ClearAndInvalidIcons } from "./shared";
import { dateInputControlInvalid, dateInputTriggerTrailingPadding } from "./affix";

export const DateVariant: React.FC<DateVariantProps> = ({
  value,
  placeholder,
  disabled,
  nullable,
  invalid,
  onDateChange,
  format: formatProp,
  firstDayOfWeek,
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
  const [prevDisabled, setPrevDisabled] = useState(disabled);
  const [prevAutoFocus, setPrevAutoFocus] = useState(autoFocus);

  const hasAutoFocusedRef = useRef(false);
  const buttonRef = useRef<HTMLButtonElement>(null);

  if (disabled !== prevDisabled || autoFocus !== prevAutoFocus) {
    setPrevDisabled(disabled);
    setPrevAutoFocus(autoFocus);
    if (autoFocus && !disabled && !hasAutoFocusedRef.current) {
      setOpen(true);
    }
  }

  useEffect(() => {
    if (autoFocus && !disabled && !hasAutoFocusedRef.current) {
      hasAutoFocusedRef.current = true;
      buttonRef.current?.focus();
    }
  }, [autoFocus, disabled]);

  const date = value ? new Date(value) : undefined;
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

  const disabledDays = useMemo(() => {
    const matchers: Array<{ before: Date } | { after: Date }> = [];
    if (min) matchers.push({ before: new Date(min) });
    if (max) matchers.push({ after: new Date(max) });
    return matchers;
  }, [min, max]);

  const handleOpenChange = useCallback(
    (newOpen: boolean) => {
      setOpen(newOpen);
      onFocusChange?.(newOpen);
    },
    [onFocusChange],
  );

  const handleClear = (e?: React.MouseEvent) => {
    e?.preventDefault();
    e?.stopPropagation();
    onDateChange(undefined);
  };

  const handleSelect = useCallback(
    (selectedDate: Date | undefined) => {
      onDateChange(selectedDate);
      setOpen(false);
    },
    [onDateChange],
  );

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
              {date ? format(date, formatProp || "yyyy-MM-dd") : placeholder || "Pick a date"}
            </span>
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="start">
          <Calendar
            mode="single"
            selected={date}
            onSelect={handleSelect}
            disabled={disabledDays.length > 0 ? disabledDays : undefined}
            initialFocus
            weekStartsOn={firstDayOfWeek}
            density={density}
          />
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
