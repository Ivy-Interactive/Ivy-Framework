import * as React from "react";
import { useCallback, useMemo } from "react";
import { useEventHandler } from "@/components/event-handler";
import { useOptimisticValue } from "../shared/useOptimisticValue";
import { Densities } from "@/types/density";
import {
  DateTimeInputWidgetProps,
  BaseVariantProps,
  DateChangeProp,
  TimeChangeProp,
  VariantType,
  WeekDay,
} from "./types";
import { DateVariant } from "./DateVariant";
import { DateTimeVariant } from "./DateTimeVariant";
import { TimeVariant } from "./TimeVariant";
import { MonthVariant } from "./MonthVariant";
import { WeekVariant } from "./WeekVariant";
import { YearVariant } from "./YearVariant";
import { EMPTY_ARRAY } from "@/lib/constants";
import { cn } from "@/lib/utils";

const VariantComponents: Record<
  VariantType,
  React.FC<BaseVariantProps & DateChangeProp & TimeChangeProp>
> = {
  Date: DateVariant,
  DateTime: DateTimeVariant,
  Time: TimeVariant,
  Month: MonthVariant,
  Week: WeekVariant,
  Year: YearVariant,
};

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

export const DateTimeInputWidget: React.FC<DateTimeInputWidgetProps> = ({
  id,
  value,
  placeholder,
  disabled = false,
  variant = "Date",
  nullable = false,
  invalid,
  format: formatProp,
  firstDayOfWeek: firstDayOfWeekRaw,
  min,
  max,
  step,
  density = Densities.Medium,
  autoFocus,
  events = EMPTY_ARRAY,
  "data-testid": dataTestId,
  slots,
}) => {
  const eventHandler = useEventHandler();
  const firstDayOfWeek = resolveDayOfWeek(firstDayOfWeekRaw);

  // Normalize undefined to null when nullable
  const normalizedValue = nullable && value === undefined ? undefined : value;

  const [localValue, setLocalValue] = useOptimisticValue(normalizedValue, false);

  const handleDateChange = useCallback(
    (selectedDate: Date | undefined) => {
      if (disabled) return;
      const isoString = selectedDate?.toISOString();
      setLocalValue(isoString);
      if (events.includes("OnChange")) eventHandler("OnChange", id, [isoString]);
    },
    [disabled, eventHandler, id, setLocalValue, events],
  );

  const handleTimeChange = useCallback(
    (time: string) => {
      if (disabled) return;

      // For Time variant, send the time string directly
      if (variant === "Time") {
        setLocalValue(time);
        if (events.includes("OnChange")) eventHandler("OnChange", id, [time]);
      } else {
        // DateTime variant: merge time into current date so we don't overwrite with today
        if (!time?.trim()) return;
        const parts = time.split(":").map(Number);
        const [hours, minutes, seconds] = [parts[0] || 0, parts[1] || 0, parts[2] || 0];
        let baseDate: Date;
        if (localValue && typeof localValue === "string") {
          const parsed = new Date(localValue);
          baseDate = !isNaN(parsed.getTime()) ? parsed : new Date();
        } else {
          baseDate = new Date();
        }
        baseDate.setHours(hours, minutes, seconds);
        const isoString = baseDate.toISOString();
        setLocalValue(isoString);
        if (events.includes("OnChange")) eventHandler("OnChange", id, [isoString]);
      }
    },
    [disabled, eventHandler, id, variant, localValue, setLocalValue, events],
  );

  const VariantComponent = useMemo(() => VariantComponents[variant], [variant]);

  const handleFocusChange = useCallback(
    (focused: boolean) => {
      if (disabled) return;
      if (focused) {
        if (events.includes("OnFocus")) eventHandler("OnFocus", id, []);
      } else {
        if (events.includes("OnBlur")) eventHandler("OnBlur", id, []);
      }
    },
    [disabled, events, eventHandler, id],
  );

  const prefixContent = slots?.Prefix;
  const suffixContent = slots?.Suffix;
  const hasPrefix = (prefixContent?.length ?? 0) > 0;
  const hasSuffix = (suffixContent?.length ?? 0) > 0;
  const hasAffixes = hasPrefix || hasSuffix;

  const variantElement = (
    <VariantComponent
      id={id}
      value={localValue}
      placeholder={placeholder}
      disabled={disabled}
      nullable={nullable}
      invalid={invalid}
      format={formatProp}
      firstDayOfWeek={firstDayOfWeek}
      min={min}
      max={max}
      step={step}
      density={density}
      autoFocus={autoFocus}
      onDateChange={handleDateChange}
      onTimeChange={handleTimeChange}
      onFocusChange={handleFocusChange}
      data-testid={dataTestId}
    />
  );

  if (!hasAffixes) {
    return <div className="relative w-full">{variantElement}</div>;
  }

  return (
    <div
      className={cn(
        "relative flex flex-1 items-stretch rounded-field border border-input bg-transparent shadow-sm transition-colors dark:bg-white/5 dark:border-white/10",
        invalid && "border-destructive",
        disabled && "opacity-50 cursor-not-allowed",
      )}
    >
      {hasPrefix && (
        <div className="flex items-center px-3 bg-muted text-muted-foreground border-r border-input rounded-tl-[var(--radius-fields)] rounded-bl-[var(--radius-fields)]">
          {prefixContent}
        </div>
      )}
      <div
        className={cn(
          "flex-1 relative w-full [&_button]:border-0 [&_button]:shadow-none [&_input]:border-0 [&_input]:shadow-none",
          hasPrefix && "[&_button]:rounded-l-none [&_input]:rounded-l-none",
          hasSuffix && "[&_button]:rounded-r-none [&_input]:rounded-r-none",
        )}
      >
        {variantElement}
      </div>
      {hasSuffix && (
        <div className="flex items-center px-3 bg-muted text-muted-foreground border-l border-input rounded-tr-[var(--radius-fields)] rounded-br-[var(--radius-fields)]">
          {suffixContent}
        </div>
      )}
    </div>
  );
};
