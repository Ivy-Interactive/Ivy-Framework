import React, { useCallback, useState } from "react";
import { useOptimisticValue } from "./shared/useOptimisticValue";
import { DateRange } from "react-day-picker";
import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { cn } from "@/lib/utils";
import {
  addMonths,
  format,
  isBefore,
  isSameMonth,
  subMonths,
  format as formatDate,
  isValid,
} from "date-fns";
import { useEventHandler } from "@/components/event-handler";
import { Densities } from "@/types/density";
import {
  dateRangeInputVariant,
  dateRangeInputTextVariant,
} from "@/components/ui/input/date-range-input-variant";
import { EMPTY_ARRAY } from "@/lib/constants";
import { DateRangePresets } from "./DateRangePresets";
import {
  DateInputAffixShell,
  dateInputControlInvalid,
  dateInputEmbeddedControlClasses,
  dateInputFieldShellClasses,
  dateInputTriggerTrailingPadding,
} from "./DateTimeInputWidget/affix";
import { ClearAndInvalidIcons } from "./DateTimeInputWidget/shared";

interface DateRangeInputWidgetProps {
  id: string;
  value?: {
    item1: string | null;
    item2: string | null;
  } | null;
  disabled?: boolean;
  placeholder?: string;
  startPlaceholder?: string;
  endPlaceholder?: string;
  format?: string;
  invalid?: string;
  nullable?: boolean;
  firstDayOfWeek?: WeekDay | string;
  min?: string | null;
  max?: string | null;
  density?: Densities;
  events: string[];
  autoFocus?: boolean;
  "data-testid"?: string;
  slots?: { Prefix?: React.ReactNode[]; Suffix?: React.ReactNode[] };
}

type WeekDay = 0 | 1 | 2 | 3 | 4 | 5 | 6;

const dayOfWeekMap: Record<string, WeekDay> = {
  Sunday: 0,
  Monday: 1,
  Tuesday: 2,
  Wednesday: 3,
  Thursday: 4,
  Friday: 5,
  Saturday: 6,
};

function resolveDayOfWeek(value?: WeekDay | string): WeekDay | undefined {
  if (value == null) return undefined;
  if (typeof value === "number") return value as WeekDay;
  return dayOfWeekMap[value];
}

export const DateRangeInputWidget: React.FC<DateRangeInputWidgetProps> = ({
  id,
  value,
  disabled = false,
  placeholder = "Pick a date range",
  startPlaceholder,
  endPlaceholder,
  format: formatProp,
  invalid,
  nullable = false,
  firstDayOfWeek: firstDayOfWeekRaw,
  min,
  max,
  density = Densities.Medium,
  events = EMPTY_ARRAY,
  autoFocus,
  "data-testid": dataTestId,
  slots,
}) => {
  const firstDayOfWeek = resolveDayOfWeek(firstDayOfWeekRaw);
  const eventHandler = useEventHandler();

  type RangeValue = { item1: string | null; item2: string | null } | null;
  const serverRange: RangeValue = value ?? null;

  const rangeEqual = (a: RangeValue, b: RangeValue): boolean => {
    if (a === b) return true;
    if (a == null || b == null) return a == b;
    return a.item1 === b.item1 && a.item2 === b.item2;
  };

  const [localRange, setLocalRange] = useOptimisticValue(serverRange, false, rangeEqual);

  const handleChange = useCallback(
    (e: DateRange) => {
      if (!events.includes("OnChange")) return;
      if (disabled) return;
      // Convert to yyyy-MM-dd or null
      const item1 = e.from && isValid(e.from) ? formatDate(e.from, "yyyy-MM-dd") : null;
      const item2 = e.to && isValid(e.to) ? formatDate(e.to, "yyyy-MM-dd") : null;
      const newRange = { item1, item2 };
      setLocalRange(newRange);
      eventHandler("OnChange", id, [newRange]);
    },
    [id, disabled, events, eventHandler, setLocalRange],
  );

  const handleClear = useCallback(
    (e?: React.MouseEvent) => {
      e?.preventDefault();
      e?.stopPropagation();
      if (!events.includes("OnChange")) return;
      if (disabled) return;
      const cleared = { item1: null, item2: null };
      setLocalRange(cleared);
      eventHandler("OnChange", id, [cleared]);
    },
    [id, disabled, events, eventHandler, setLocalRange],
  );
  const parseDate = (val: string | null | undefined) => {
    if (!val) return undefined;
    const d = new Date(val);
    return isNaN(d.getTime()) ? undefined : d;
  };

  const date: DateRange = {
    from: parseDate(localRange?.item1),
    to: parseDate(localRange?.item2),
  };

  const minDate = parseDate(min);
  const maxDate = parseDate(max);

  const disabledMatchers = [
    ...(minDate ? [{ before: minDate }] : []),
    ...(maxDate ? [{ after: maxDate }] : []),
  ];

  const [leftMonth, setLeftMonth] = useState(() => new Date());
  const [rightMonth, setRightMonth] = useState(() => addMonths(new Date(), 1));
  const [isOpen, setIsOpen] = useState(false);

  const [prevDisabled, setPrevDisabled] = React.useState(disabled);
  const [prevAutoFocus, setPrevAutoFocus] = React.useState(autoFocus);
  const hasAutoFocusedRef = React.useRef(false);

  if (disabled !== prevDisabled || autoFocus !== prevAutoFocus) {
    setPrevDisabled(disabled);
    setPrevAutoFocus(autoFocus);
    if (autoFocus && !disabled && !hasAutoFocusedRef.current) {
      setIsOpen(true);
    }
  }

  React.useEffect(() => {
    if (autoFocus && !disabled && !hasAutoFocusedRef.current) {
      hasAutoFocusedRef.current = true;
    }
  }, [autoFocus, disabled]);

  const handleLeftMonthChange = (newLeft: Date) => {
    setLeftMonth(newLeft);
    if (isBefore(rightMonth, newLeft) || isSameMonth(rightMonth, newLeft)) {
      setRightMonth(addMonths(newLeft, 1));
    }
  };

  const handleRightMonthChange = (newRight: Date) => {
    if (isBefore(newRight, leftMonth) || isSameMonth(newRight, leftMonth)) {
      setLeftMonth(subMonths(leftMonth, 1));
      setRightMonth(newRight);
    } else {
      setRightMonth(newRight);
    }
  };

  const handleOpenChange = useCallback(
    (newOpen: boolean) => {
      if (disabled) return;
      setIsOpen(newOpen);
      if (!newOpen) {
        if (events.includes("OnBlur")) eventHandler("OnBlur", id, []);
      } else {
        if (events.includes("OnFocus")) eventHandler("OnFocus", id, []);
      }
    },
    [disabled, eventHandler, id, events, setIsOpen],
  );

  // Use custom format if provided, otherwise use default
  const displayFormat = formatProp || "LLL dd, y";

  // Show clear button if nullable, not disabled, and has a value
  const showClear = nullable && !disabled && Boolean(date?.from ?? date?.to);

  const prefixContent = slots?.Prefix;
  const suffixContent = slots?.Suffix;
  const hasPrefix = (prefixContent?.length ?? 0) > 0;
  const hasSuffix = (suffixContent?.length ?? 0) > 0;
  const hasAffixes = hasPrefix || hasSuffix;
  const trailingBesideSuffix = hasSuffix;
  const showTrailing = showClear || Boolean(invalid);
  const controlInvalid = dateInputControlInvalid(
    hasAffixes,
    trailingBesideSuffix,
    showClear,
    invalid,
  );
  const trailingPadding = dateInputTriggerTrailingPadding(
    hasAffixes,
    trailingBesideSuffix,
    showClear,
    invalid,
  );

  const triggerContent = (
    <Popover open={isOpen} onOpenChange={handleOpenChange}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          disabled={disabled}
          autoFocus={autoFocus}
          data-testid={dataTestId}
          data-slot="calendar"
          className={cn(
            dateRangeInputVariant({ density }),
            "inline-flex items-center",
            !date && "text-muted-foreground",
            controlInvalid && "border-destructive focus-visible:ring-destructive",
            trailingPadding,
            "border-0 bg-transparent shadow-none hover:bg-transparent focus-visible:ring-0 focus-visible:ring-offset-0 dark:border-transparent dark:bg-transparent dark:hover:bg-transparent",
            hasAffixes && "rounded-l-none",
            (hasSuffix || (showTrailing && !trailingBesideSuffix)) && "rounded-r-none",
          )}
          onBlur={() => {
            if (disabled) return;
            if (events.includes("OnBlur") && !isOpen) eventHandler("OnBlur", id, []);
          }}
          onFocus={() => {
            if (disabled) return;
            if (events.includes("OnFocus") && !isOpen) eventHandler("OnFocus", id, []);
          }}
        >
          {date?.from ? (
            date.to ? (
              <span className={cn("truncate", dateRangeInputTextVariant({ density }))}>
                {format(date.from, displayFormat)} - {format(date.to, displayFormat)}
              </span>
            ) : (
              <span className={cn("truncate", dateRangeInputTextVariant({ density }))}>
                {format(date.from, displayFormat)}
                {(endPlaceholder || startPlaceholder) && (
                  <span className="text-muted-foreground">
                    {" "}
                    - {endPlaceholder || placeholder || "Pick a date range"}
                  </span>
                )}
              </span>
            )
          ) : startPlaceholder || endPlaceholder ? (
            <span
              className={cn(
                "truncate",
                dateRangeInputTextVariant({ density }),
                "text-muted-foreground",
              )}
            >
              {startPlaceholder || placeholder || "Start"} -{" "}
              {endPlaceholder || placeholder || "End"}
            </span>
          ) : (
            <span
              className={cn(
                "truncate",
                dateRangeInputTextVariant({ density }),
                "text-muted-foreground",
              )}
            >
              {placeholder}
            </span>
          )}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-auto p-0" align="start">
        <div className="rounded-box">
          <div className="flex max-sm:flex-col">
            <div className="relative border-border py-4 max-sm:order-1 max-sm:border-t sm:w-32">
              <div className="h-full border-border sm:border-e">
                <DateRangePresets
                  density={density}
                  onSelect={(range, left, right) => {
                    handleChange(range);
                    setLeftMonth(left);
                    setRightMonth(right);
                    setIsOpen(false);
                  }}
                />
              </div>
            </div>
            <div className="flex">
              <Calendar
                mode="range"
                selected={date}
                onSelect={(newDate) => newDate && handleChange(newDate)}
                month={leftMonth}
                onMonthChange={handleLeftMonthChange}
                className="p-2 bg-background"
                disabled={disabledMatchers}
                weekStartsOn={firstDayOfWeek}
                density={density}
              />

              <Calendar
                mode="range"
                selected={date}
                onSelect={(newDate) => newDate && handleChange(newDate)}
                month={rightMonth}
                onMonthChange={handleRightMonthChange}
                className="p-2 bg-background"
                disabled={disabledMatchers}
                weekStartsOn={firstDayOfWeek}
                density={density}
              />
            </div>
          </div>
        </div>
      </PopoverContent>
    </Popover>
  );

  if (!hasAffixes) {
    return (
      <div className="relative w-full select-none">
        <div
          className={dateInputFieldShellClasses({
            focused: isOpen,
            invalid,
            disabled,
          })}
        >
          <div className={cn("relative min-w-0 flex-1", dateInputEmbeddedControlClasses)}>
            {triggerContent}
          </div>
        </div>
        {showTrailing && (
          <ClearAndInvalidIcons
            showClear={showClear}
            invalid={invalid}
            density={density}
            onClear={handleClear}
          />
        )}
      </div>
    );
  }

  return (
    <div className="relative w-full select-none">
      <DateInputAffixShell
        density={density}
        invalid={invalid}
        disabled={disabled}
        focused={isOpen}
        hasPrefix={hasPrefix}
        hasSuffix={hasSuffix}
        prefixContent={prefixContent}
        suffixContent={suffixContent}
        showClear={showClear}
        onClear={handleClear}
      >
        <div className={cn("w-full min-w-0", dateInputEmbeddedControlClasses)}>
          {triggerContent}
        </div>
      </DateInputAffixShell>
    </div>
  );
};
