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
import { CalendarIcon } from 'lucide-react';
import {
  endOfMonth,
  endOfYear,
  format,
  startOfMonth,
  startOfYear,
  subDays,
  subMonths,
  subYears,
} from 'date-fns';
import { useEventHandler } from '@/components/EventHandlerContext';

interface DateRangeInputWidgetProps {
  id: string;
  value: {
    item1: string;
    item2: string;
  };
  disabled: boolean;
  events: string[];
}

export const DateRangeInputWidget: React.FC<DateRangeInputWidgetProps> = ({
  id,
  value,
  disabled,
  events,
}) => {
  const eventHandler = useEventHandler();

  const handleChange = useCallback(
    (e: DateRange) => {
      if (!events.includes('OnChange')) return;
      if (disabled) return;
      eventHandler('OnChange', id, [{ item1: e.from, item2: e.to }]);
    },
    [id, disabled]
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

  const date: DateRange = {
    from: new Date(value.item1),
    to: new Date(value.item2),
  };

  const [month, setMonth] = useState(today);
  const [isOpen, setIsOpen] = useState(false);

  return (
    <div>
      <div className="w-full">
        <Popover open={isOpen} onOpenChange={setIsOpen}>
          <PopoverTrigger asChild>
            <Button
              variant="outline"
              disabled={disabled}
              className={cn(
                'w-full justify-start text-left font-normal',
                !date && 'text-muted-foreground'
              )}
            >
              <CalendarIcon className="mr-2 h-4 w-4" />
              {date?.from ? (
                date.to ? (
                  <>
                    {format(date.from, 'LLL dd, y')} -{' '}
                    {format(date.to, 'LLL dd, y')}
                  </>
                ) : (
                  format(date.from, 'LLL dd, y')
                )
              ) : (
                <span>Pick a date range</span>
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
                        className="w-full justify-start"
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
                        className="w-full justify-start"
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
                        className="w-full justify-start"
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
                        className="w-full justify-start"
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
                        className="w-full justify-start"
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
                        className="w-full justify-start"
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
                        className="w-full justify-start"
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
                        className="w-full justify-start"
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
      </div>
    </div>
  );
};
