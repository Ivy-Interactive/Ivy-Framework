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
  density = Densities.Medium,
  "data-testid": dataTestId,
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
      eventHandler("OnChange", id, [isoString]);
    },
    [disabled, eventHandler, id, setLocalValue],
  );

  const handleTimeChange = useCallback(
    (time: string) => {
      if (disabled) return;

      // For Time variant, send the time string directly
      if (variant === "Time") {
        setLocalValue(time);
        eventHandler("OnChange", id, [time]);
      } else {
        // For other variants, create a date with the selected time
        const [hours, minutes, seconds] = time.split(":").map(Number);
        const newDateTime = new Date();
        newDateTime.setHours(hours, minutes, seconds);

        const isoString = newDateTime.toISOString();
        setLocalValue(isoString);
        eventHandler("OnChange", id, [isoString]);
      }
    },
    [disabled, eventHandler, id, variant, setLocalValue],
  );

  const VariantComponent = useMemo(() => VariantComponents[variant], [variant]);

  return (
    <VariantComponent
      id={id}
      value={localValue}
      placeholder={placeholder}
      disabled={disabled}
      nullable={nullable}
      invalid={invalid}
      format={formatProp}
      firstDayOfWeek={firstDayOfWeek}
      density={density}
      onDateChange={handleDateChange}
      onTimeChange={handleTimeChange}
      data-testid={dataTestId}
    />
  );
};
