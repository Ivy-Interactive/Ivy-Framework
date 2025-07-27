import React, { useCallback, useState } from 'react';
import { DateRange } from 'react-day-picker';
import { Button } from '@/components/ui/button';
import { Calendar } from '@/components/ui/calendar';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import { cn } from '@/lib/utils';
import { CalendarIcon, X } from 'lucide-react';
import {
  endOfMonth,
  endOfYear,
  format,
  startOfMonth,
  startOfYear,
  subDays,
  subMonths,
  subYears,
  format as formatDate,
  isValid,
} from 'date-fns';
import { useEventHandler } from '@/components/EventHandlerContext';
import { InvalidIcon } from '@/components/InvalidIcon';

interface DateRangeInputWidgetProps {
  id: string;
  value: {
    item1: string;
    item2: string;
  };
  disabled?: boolean;
  placeholder?: string;
  format?: string;
  invalid?: string;
  nullable?: boolean;
  events: string[];
  'data-testid'?: string;
}

export const DateRangeInputWidget: React.FC<DateRangeInputWidgetProps> = ({
  id,
  value,
  disabled = false,
  placeholder = 'Pick a date range',
  format: formatProp,
  invalid,
  nullable = false,
  events,
  'data-testid': dataTestId,
}) => {
  const eventHandler = useEventHandler();

  const handleChange = useCallback(
    (e: DateRange) => {
      if (!events.includes('OnChange')) return;
      if (disabled) return;
      // Convert to yyyy-MM-dd or null
      const item1 =
        e.from && isValid(e.from) ? formatDate(e.from, 'yyyy-MM-dd') : null;
      const item2 =
        e.to && isValid(e.to) ? formatDate(e.to, 'yyyy-MM-dd') : null;
      eventHandler('OnChange', id, [{ item1, item2 }]);
    },
    [id, disabled, events, eventHandler]
  );

  const handleClear = useCallback(
    (e: React.MouseEvent) => {
      e.preventDefault();
      e.stopPropagation();
      if (!events.includes('OnChange')) return;
      if (disabled) return;
      eventHandler('OnChange', id, [{ item1: null, item2: null }]);
    },
    [id, disabled, events, eventHandler]
  );

  const today = new Date();

  const yesterday = {
    from: subDays(today, 1),
    to: subDays(today, 1),
  };

  const last7Days = {
    from: subDays(today, 6),
    to: today,
  };

  const last30Days = {
    from: subDays(today, 29),
    to: today,
  };

  const monthToDate = {
    from: startOfMonth(today),
    to: today,
  };

  const lastMonth = {
    from: startOfMonth(subMonths(today, 1)),
    to: endOfMonth(subMonths(today, 1)),
  };

  const yearToDate = {
    from: startOfYear(today),
    to: today,
  };

  const lastYear = {
    from: startOfYear(subYears(today, 1)),
    to: endOfYear(subYears(today, 1)),
  };

  const parseDate = (val: string | null | undefined) => {
    if (!val) return undefined;
    const d = new Date(val);
    return isNaN(d.getTime()) ? undefined : d;
  };

  const date: DateRange = {
    from: parseDate(value.item1),
    to: parseDate(value.item2),
  };

  const [month, setMonth] = useState(today);
  const [isOpen, setIsOpen] = useState(false);

  // Use custom format if provided, otherwise use default
  const displayFormat = formatProp || 'LLL dd, y';

  // Show clear button if nullable, not disabled, and has a value
  const showClear = nullable && !disabled && (date?.from || date?.to);

  return (
    <div className="relative w-full select-none">
      <Popover open={isOpen} onOpenChange={setIsOpen}>
        <PopoverTrigger asChild>
          <Button
            variant="outline"
            disabled={disabled}
            data-testid={dataTestId}
            className={cn(
              'w-full justify-start text-left font-normal cursor-pointer',
              !date && 'text-muted-foreground',
              invalid && 'border-destructive focus-visible:ring-destructive',
              showClear && invalid
                ? 'pr-16'
                : showClear || invalid
                  ? 'pr-8'
                  : ''
            )}
          >
            <CalendarIcon className="mr-2 h-4 w-4" />
            {date?.from ? (
              date.to ? (
                <>
                  {format(date.from, displayFormat)} -{' '}
                  {format(date.to, displayFormat)}
                </>
              ) : (
                format(date.from, displayFormat)
              )
            ) : (
              <span>{placeholder}</span>
            )}
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="start">
          <div className="rounded-lg border border-border">
            <div className="flex max-sm:flex-col">
              <div className="relative border-border py-4 max-sm:order-1 max-sm:border-t sm:w-32">
                <div className="h-full border-border sm:border-e">
                  <div className="flex flex-col px-2">
                    <Button
                      variant="ghost"
                      size="sm"
                      className="w-full justify-start cursor-pointer"
                      onClick={() => {
                        handleChange({
                          from: today,
                          to: today,
                        });
                        setMonth(today);
                        setIsOpen(false);
                      }}
                    >
                      Today
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      className="w-full justify-start cursor-pointer"
                      onClick={() => {
                        handleChange(yesterday);
                        setMonth(yesterday.to);
                        setIsOpen(false);
                      }}
                    >
                      Yesterday
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      className="w-full justify-start cursor-pointer"
                      onClick={() => {
                        handleChange(last7Days);
                        setMonth(last7Days.to);
                        setIsOpen(false);
                      }}
                    >
                      Last 7 Days
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      className="w-full justify-start cursor-pointer"
                      onClick={() => {
                        handleChange(last30Days);
                        setMonth(last30Days.to);
                        setIsOpen(false);
                      }}
                    >
                      Last 30 Days
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      className="w-full justify-start cursor-pointer"
                      onClick={() => {
                        handleChange(monthToDate);
                        setMonth(monthToDate.to);
                        setIsOpen(false);
                      }}
                    >
                      Month to Date
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      className="w-full justify-start cursor-pointer"
                      onClick={() => {
                        handleChange(lastMonth);
                        setMonth(lastMonth.to);
                        setIsOpen(false);
                      }}
                    >
                      Last Month
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      className="w-full justify-start cursor-pointer"
                      onClick={() => {
                        handleChange(yearToDate);
                        setMonth(yearToDate.to);
                        setIsOpen(false);
                      }}
                    >
                      Year to Date
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      className="w-full justify-start cursor-pointer"
                      onClick={() => {
                        handleChange(lastYear);
                        setMonth(lastYear.to);
                        setIsOpen(false);
                      }}
                    >
                      Last Year
                    </Button>
                  </div>
                </div>
              </div>
              <div className="flex">
                <Calendar
                  mode="range"
                  selected={date}
                  onSelect={newDate => {
                    if (newDate) {
                      handleChange(newDate);
                    }
                  }}
                  month={month}
                  onMonthChange={setMonth}
                  numberOfMonths={2}
                  className="p-2 bg-background"
                  disabled={[{ after: today }]}
                />
              </div>
            </div>
          </div>
        </PopoverContent>
      </Popover>
      {/* Icons absolutely positioned */}
      {(showClear || invalid) && (
        <div className="absolute right-2.5 top-1/2 -translate-y-1/2 flex items-center gap-2">
          {showClear && (
            <button
              type="button"
              tabIndex={-1}
              aria-label="Clear"
              onClick={handleClear}
              className="p-1 rounded hover:bg-accent focus:outline-none cursor-pointer"
            >
              <X className="h-4 w-4 text-muted-foreground hover:text-foreground" />
            </button>
          )}
          {invalid && <InvalidIcon message={invalid} />}
        </div>
      )}
    </div>
  );
};
