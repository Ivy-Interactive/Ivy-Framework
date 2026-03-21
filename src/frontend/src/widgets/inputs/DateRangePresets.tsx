import React, { useMemo } from "react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { dateRangeInputTextVariant } from "@/components/ui/input/date-range-input-variant";
import { Densities } from "@/types/density";
import { DateRange } from "react-day-picker";
import {
  subDays,
  startOfMonth,
  subMonths,
  endOfMonth,
  startOfYear,
  subYears,
  endOfYear,
  addMonths,
} from "date-fns";

interface DateRangePresetsProps {
  density: Densities;
  onSelect: (range: DateRange, leftMonth: Date, rightMonth: Date) => void;
}

export const DateRangePresets: React.FC<DateRangePresetsProps> = ({ density, onSelect }) => {
  const presets = useMemo(() => {
    const today = new Date();
    return [
      {
        label: "Today",
        range: { from: today, to: today },
        leftMonth: today,
        rightMonth: addMonths(today, 1),
      },
      {
        label: "Yesterday",
        range: { from: subDays(today, 1), to: subDays(today, 1) },
        leftMonth: subDays(today, 1),
        rightMonth: addMonths(subDays(today, 1), 1),
      },
      {
        label: "Last 7 Days",
        range: { from: subDays(today, 6), to: today },
        leftMonth: subDays(today, 6),
        rightMonth: today,
      },
      {
        label: "Last 30 Days",
        range: { from: subDays(today, 29), to: today },
        leftMonth: subDays(today, 29),
        rightMonth: today,
      },
      {
        label: "Month to Date",
        range: { from: startOfMonth(today), to: today },
        leftMonth: startOfMonth(today),
        rightMonth: today,
      },
      {
        label: "Last Month",
        range: {
          from: startOfMonth(subMonths(today, 1)),
          to: endOfMonth(subMonths(today, 1)),
        },
        leftMonth: startOfMonth(subMonths(today, 1)),
        rightMonth: endOfMonth(subMonths(today, 1)),
      },
      {
        label: "Year to Date",
        range: { from: startOfYear(today), to: today },
        leftMonth: startOfYear(today),
        rightMonth: today,
      },
      {
        label: "Last Year",
        range: {
          from: startOfYear(subYears(today, 1)),
          to: endOfYear(subYears(today, 1)),
        },
        leftMonth: startOfYear(subYears(today, 1)),
        rightMonth: endOfYear(subYears(today, 1)),
      },
    ];
  }, []);

  return (
    <div className="flex flex-col px-2">
      {presets.map((preset) => (
        <Button
          key={preset.label}
          variant="ghost"
          size="sm"
          className={cn(
            "w-full justify-start cursor-pointer",
            dateRangeInputTextVariant({ density }),
          )}
          onClick={() => onSelect(preset.range, preset.leftMonth, preset.rightMonth)}
        >
          {preset.label}
        </Button>
      ))}
    </div>
  );
};
