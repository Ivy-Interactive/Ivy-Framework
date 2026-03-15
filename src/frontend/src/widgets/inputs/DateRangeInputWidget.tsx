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
  addMonths,
  endOfMonth,
  endOfYear,
  format,
  isBefore,
  isSameMonth,
  startOfMonth,
  startOfYear,
  subDays,
  subMonths,
  subYears,
  format as formatDate,
  isValid,
} from 'date-fns';
import { useEventHandler } from '@/components/event-handler';
import { InvalidIcon } from '@/components/InvalidIcon';
import { Densities } from '@/types/density';
import {
  dateRangeInputVariant,
  dateRangeInputIconVariant,
  dateRangeInputTextVariant,
} from '@/components/ui/input/date-range-input-variant';

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
  density?: Densities;
  events: string[];
  'data-testid'?: string;
}

type WeekDay = 0 | 1 | 2 | 3 | 4 | 5 | 6;

const EMPTY_EVENTS: string[] = [];

const dayOfWeekMap: Record<string, WeekDay> = {
  Sunday: 0, Monday: 1, Tuesday: 2, Wednesday: 3,
  Thursday: 4, Friday: 5, Saturday: 6,
};

function resolveDayOfWeek(value?: WeekDay | string): WeekDay | undefined {
  if (value == null) return undefined;
  if (typeof value === 'number') return value as WeekDay;
  return dayOfWeekMap[value];
}

export const DateRangeInputWidget: React.FC<DateRangeInputWidgetProps> = ({
  id,
  value,
  disabled = false,
  placeholder = 'Pick a date range',
  startPlaceholder,
  endPlaceholder,
  format: formatProp,
  invalid,
  nullable = false,
  firstDayOfWeek: firstDayOfWeekRaw,
  density = Densities.Medium,
  events = EMPTY_EVENTS,
  'data-testid': dataTestId,
}) => {
  const firstDayOfWeek = resolveDayOfWeek(firstDayOfWeekRaw);
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
    from: parseDate(value?.item1),
    to: parseDate(value?.item2),
  };

  const [leftMonth, setLeftMonth] = useState(() => new Date());
  const [rightMonth, setRightMonth] = useState(() => addMonths(new Date(), 1));
  const [isOpen, setIsOpen] = useState(false);

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
            data-slot="calendar"
            className={cn(
              dateRangeInputVariant({ density }),
              'dark:bg-white/5 dark:hover:bg-white/10 dark:border-white/10',
              !date && 'text-muted-foreground',
              invalid && 'border-destructive focus-visible:ring-destructive',
              showClear && invalid
                ? 'pr-16'
                : showClear || invalid
                  ? 'pr-8'
                  : ''
            )}
          >
            <CalendarIcon
              className={cn(
                'mr-2 shrink-0',
                dateRangeInputIconVariant({ density })
              )}
            />
            {date?.from ? (
              date.to ? (
                <span
                  className={cn(
                    'truncate',
                    dateRangeInputTextVariant({ density })
                  )}
                >
                  {format(date.from, displayFormat)} -{' '}
                  {format(date.to, displayFormat)}
                </span>
              ) : (
                <span
                  className={cn(
                    'truncate',
                    dateRangeInputTextVariant({ density })
                  )}
                >
                  {format(date.from, displayFormat)}
                  {(endPlaceholder || startPlaceholder) && (
                    <span className="text-muted-foreground"> - {endPlaceholder || placeholder || 'Pick a date range'}</span>
                  )}
                </span>
              )
            ) : startPlaceholder || endPlaceholder ? (
              <span
                className={cn(
                  'truncate',
                  dateRangeInputTextVariant({ density }),
                  'text-muted-foreground'
                )}
              >
                {startPlaceholder || placeholder || 'Start'} - {endPlaceholder || placeholder || 'End'}
              </span>
            ) : (
              <span
                className={cn(
                  'truncate',
                  dateRangeInputTextVariant({ density }),
                  'text-muted-foreground'
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
                  <div className="flex flex-col px-2">
                    <Button
                      variant="ghost"
                      size="sm"
                      className={cn(
                        'w-full justify-start cursor-pointer',
                        dateRangeInputTextVariant({ density })
                      )}
                      onClick={() => {
                        handleChange({
                          from: today,
                          to: today,
                        });
                        setLeftMonth(today);
                        setRightMonth(addMonths(today, 1));
                        setIsOpen(false);
                      }}
                    >
                      Today
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      className={cn(
                        'w-full justify-start cursor-pointer',
                        dateRangeInputTextVariant({ density })
                      )}
                      onClick={() => {
                        handleChange(yesterday);
                        setLeftMonth(yesterday.from);
                        setRightMonth(addMonths(yesterday.from, 1));
                        setIsOpen(false);
                      }}
                    >
                      Yesterday
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      className={cn(
                        'w-full justify-start cursor-pointer',
                        dateRangeInputTextVariant({ density })
                      )}
                      onClick={() => {
                        handleChange(last7Days);
                        setLeftMonth(last7Days.from);
                        setRightMonth(last7Days.to);
                        setIsOpen(false);
                      }}
                    >
                      Last 7 Days
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      className={cn(
                        'w-full justify-start cursor-pointer',
                        dateRangeInputTextVariant({ density })
                      )}
                      onClick={() => {
                        handleChange(last30Days);
                        setLeftMonth(last30Days.from);
                        setRightMonth(last30Days.to);
                        setIsOpen(false);
                      }}
                    >
                      Last 30 Days
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      className={cn(
                        'w-full justify-start cursor-pointer',
                        dateRangeInputTextVariant({ density })
                      )}
                      onClick={() => {
                        handleChange(monthToDate);
                        setLeftMonth(monthToDate.from);
                        setRightMonth(monthToDate.to);
                        setIsOpen(false);
                      }}
                    >
                      Month to Date
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      className={cn(
                        'w-full justify-start cursor-pointer',
                        dateRangeInputTextVariant({ density })
                      )}
                      onClick={() => {
                        handleChange(lastMonth);
                        setLeftMonth(lastMonth.from);
                        setRightMonth(lastMonth.to);
                        setIsOpen(false);
                      }}
                    >
                      Last Month
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      className={cn(
                        'w-full justify-start cursor-pointer',
                        dateRangeInputTextVariant({ density })
                      )}
                      onClick={() => {
                        handleChange(yearToDate);
                        setLeftMonth(yearToDate.from);
                        setRightMonth(yearToDate.to);
                        setIsOpen(false);
                      }}
                    >
                      Year to Date
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      className={cn(
                        'w-full justify-start cursor-pointer',
                        dateRangeInputTextVariant({ density })
                      )}
                      onClick={() => {
                        handleChange(lastYear);
                        setLeftMonth(lastYear.from);
                        setRightMonth(lastYear.to);
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
                  onSelect={newDate => newDate && handleChange(newDate)}
                  month={leftMonth}
                  onMonthChange={handleLeftMonthChange}
                  className="p-2 bg-background"
                  disabled={[{ after: today }]}
                  weekStartsOn={firstDayOfWeek}
                  density={density}
                />

                <Calendar
                  mode="range"
                  selected={date}
                  onSelect={newDate => newDate && handleChange(newDate)}
                  month={rightMonth}
                  onMonthChange={handleRightMonthChange}
                  className="p-2 bg-background"
                  disabled={[{ after: today }]}
                  weekStartsOn={firstDayOfWeek}
                  density={density}
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
              <X
                className={cn(
                  dateRangeInputIconVariant({ density }),
                  'text-muted-foreground hover:text-foreground'
                )}
              />
            </button>
          )}
          {invalid && <InvalidIcon message={invalid} />}
        </div>
      )}
    </div>
  );
};
