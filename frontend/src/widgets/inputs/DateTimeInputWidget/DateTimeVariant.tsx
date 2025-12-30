import * as React from 'react';
import { useState, useCallback, useMemo } from 'react';
import { Button } from '@/components/ui/button';
import { Calendar } from '@/components/ui/calendar';
import { Input } from '@/components/ui/input';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import { format } from 'date-fns';
import { Calendar as CalendarIcon, Clock } from 'lucide-react';
import { cn } from '@/lib/utils';
import { inputStyles } from '@/lib/styles';
import { Scales } from '@/types/scale';
import {
  dateTimeInputVariants,
  dateTimeInputIconVariants,
  dateTimeInputTextVariants,
} from '@/components/ui/input/date-time-input-variants';
import { DateTimeVariantProps } from './types';
import { ClearAndInvalidIcons } from './shared';

export const DateTimeVariant: React.FC<DateTimeVariantProps> = ({
  value,
  placeholder,
  disabled,
  nullable,
  invalid,
  onDateChange,
  onTimeChange,
  format: formatProp,
  scale = Scales.Medium,
  'data-testid': dataTestId,
}) => {
  const [open, setOpen] = useState(false);
  const date = useMemo(() => (value ? new Date(value) : undefined), [value]);
  const showClear = nullable && !disabled && value != null && value !== '';

  const handleClear = (e?: React.MouseEvent) => {
    e?.preventDefault();
    e?.stopPropagation();
    onDateChange(undefined);
  };

  // Use local state for the time input to make it uncontrolled
  const [localTimeValue, setLocalTimeValue] = useState(() => {
    if (date) {
      return format(date, formatProp || 'HH:mm:ss');
    }
    return '00:00:00';
  });

  // Track if user is actively editing the time input
  const [isEditingTime, setIsEditingTime] = useState(false);

  // Update local time when date changes, but only if user is not actively editing
  React.useEffect(() => {
    if (!isEditingTime) {
      if (date) {
        const newTimeValue = format(date, formatProp || 'HH:mm:ss');
        setLocalTimeValue(newTimeValue);
      } else if (nullable) {
        // When nullable and no date, keep input empty instead of defaulting to '00:00:00'
        setLocalTimeValue('');
      } else {
        setLocalTimeValue('00:00:00');
      }
    }
  }, [date, formatProp, isEditingTime, nullable]);

  const handleDateSelect = useCallback(
    (selectedDate: Date | undefined) => {
      if (selectedDate) {
        // Preserve the time when selecting a new date
        const currentTime = date ? date : new Date();
        const newDateTime = new Date(selectedDate);
        newDateTime.setHours(
          currentTime.getHours(),
          currentTime.getMinutes(),
          currentTime.getSeconds()
        );
        onDateChange(newDateTime);
      } else {
        onDateChange(undefined);
      }
      // Do not close the popover here, allow user to pick time
    },
    [date, onDateChange]
  );

  const handleTimeChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const newTimeValue = e.target.value;
      setLocalTimeValue(newTimeValue);
      setIsEditingTime(true);
    },
    []
  );

  const handleTimeBlur = useCallback(() => {
    setIsEditingTime(false);
    // When time input loses focus, update the parent
    if (date && localTimeValue) {
      const [hours, minutes, seconds] = localTimeValue.split(':').map(Number);
      const newDateTime = new Date(date);
      newDateTime.setHours(hours, minutes, seconds);
      onDateChange(newDateTime);
    } else if (localTimeValue) {
      // If no date is selected, create a new date with the selected time
      const [hours, minutes, seconds] = localTimeValue.split(':').map(Number);
      const newDateTime = new Date();
      newDateTime.setHours(hours, minutes, seconds);
      onDateChange(newDateTime);
    }
    onTimeChange(localTimeValue);
  }, [date, localTimeValue, onDateChange, onTimeChange]);

  const handleTimeKeyDown = useCallback(
    (e: React.KeyboardEvent<HTMLInputElement>) => {
      // When user presses Enter, update the parent
      if (e.key === 'Enter') {
        setIsEditingTime(false);
        if (date && localTimeValue) {
          const [hours, minutes, seconds] = localTimeValue
            .split(':')
            .map(Number);
          const newDateTime = new Date(date);
          newDateTime.setHours(hours, minutes, seconds);
          onDateChange(newDateTime);
        } else if (localTimeValue) {
          // If no date is selected, create a new date with the selected time
          const [hours, minutes, seconds] = localTimeValue
            .split(':')
            .map(Number);
          const newDateTime = new Date();
          newDateTime.setHours(hours, minutes, seconds);
          onDateChange(newDateTime);
        }
        onTimeChange(localTimeValue);
      }
    },
    [date, localTimeValue, onDateChange, onTimeChange]
  );

  const handleTimeFocus = useCallback(() => {
    setIsEditingTime(true);
  }, []);

  return (
    <div className="relative w-full select-none">
      <Popover open={open} onOpenChange={setOpen}>
        <PopoverTrigger asChild>
          <Button
            disabled={disabled}
            variant="outline"
            className={cn(
              dateTimeInputVariants({ scale }),
              !date && 'text-muted-foreground',
              invalid && inputStyles.invalidInput,
              disabled && 'cursor-not-allowed',
              showClear && invalid
                ? 'pr-16'
                : showClear || invalid
                  ? 'pr-8'
                  : ''
            )}
            data-testid={dataTestId}
          >
            <CalendarIcon
              className={cn(
                'mr-2 shrink-0',
                dateTimeInputIconVariants({ scale })
              )}
            />
            <Clock
              className={cn(
                'mr-2 shrink-0',
                dateTimeInputIconVariants({ scale })
              )}
            />
            <span
              className={cn(
                'truncate',
                dateTimeInputTextVariants({ scale }),
                !date && 'text-muted-foreground'
              )}
            >
              {date
                ? format(date, formatProp || 'yyyy-MM-dd')
                : placeholder || 'Pick a date & time'}
            </span>
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="start">
          <div className="flex flex-col gap-2 p-2">
            <Calendar
              mode="single"
              selected={date}
              onSelect={handleDateSelect}
              initialFocus
              scale={scale}
            />
            <div className="flex items-center gap-2">
              <Clock
                className={cn(
                  dateTimeInputIconVariants({ scale }),
                  'text-muted-foreground'
                )}
              />
              <Input
                type="time"
                step="1"
                value={localTimeValue}
                onChange={handleTimeChange}
                onFocus={handleTimeFocus}
                onBlur={handleTimeBlur}
                onKeyDown={handleTimeKeyDown}
                disabled={disabled}
                className={cn(
                  'bg-transparent appearance-none [&::-webkit-calendar-picker-indicator]:hidden',
                  dateTimeInputTextVariants({ scale }),
                  invalid && inputStyles.invalidInput
                )}
                data-testid={dataTestId ? `${dataTestId}-time` : undefined}
              />
            </div>
          </div>
        </PopoverContent>
      </Popover>
      <ClearAndInvalidIcons
        showClear={showClear}
        invalid={invalid}
        scale={scale}
        onClear={handleClear}
      />
    </div>
  );
};
