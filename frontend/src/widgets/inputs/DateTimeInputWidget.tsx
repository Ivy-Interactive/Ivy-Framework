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
import { Calendar as CalendarIcon, Clock, X } from 'lucide-react';
import { cn } from '@/lib/utils';
import { useEventHandler } from '@/components/EventHandlerContext';
import { inputStyles } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';

type VariantType = 'Date' | 'DateTime' | 'Time';

interface DateTimeInputWidgetProps {
  id: string;
  value?: string;
  placeholder?: string;
  disabled: boolean;
  variant?: VariantType;
  nullable?: boolean;
  invalid?: string;
  format?: string;
  'data-testid'?: string;
}

interface BaseVariantProps {
  id: string;
  value?: string;
  placeholder?: string;
  disabled: boolean;
  nullable?: boolean;
  invalid?: string;
  format?: string;
  'data-testid'?: string;
}

interface DateVariantProps extends BaseVariantProps {
  onDateChange: (date: Date | undefined) => void;
}

interface DateTimeVariantProps extends BaseVariantProps {
  onDateChange: (date: Date | undefined) => void;
  onTimeChange: (time: string) => void;
}

interface TimeVariantProps extends BaseVariantProps {
  onTimeChange: (time: string) => void;
}

const DateVariant: React.FC<DateVariantProps> = ({
  value,
  placeholder,
  disabled,
  nullable,
  invalid,
  onDateChange,
  format: formatProp,
  'data-testid': dataTestId,
}) => {
  const [open, setOpen] = useState(false);
  const date = value ? new Date(value) : undefined;
  const showClear = nullable && !disabled && value != null && value !== '';

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
    [onDateChange]
  );

  return (
    <div className="relative flex items-center gap-2">
      <Popover open={open} onOpenChange={setOpen}>
        <PopoverTrigger asChild>
          <Button
            disabled={disabled}
            variant="outline"
            className={cn(
              'w-full justify-start text-left font-normal pr-20 cursor-pointer', // pr-20 for clear+icon
              !date && 'text-muted-foreground',
              invalid && inputStyles.invalidInput,
              disabled && 'cursor-not-allowed'
            )}
            data-testid={dataTestId}
          >
            <CalendarIcon className="mr-2 h-4 w-4" />
            {date ? (
              format(date, formatProp || 'yyyy-MM-dd')
            ) : (
              <span>{placeholder || 'Pick a date'}</span>
            )}
            {/* Icons absolutely positioned inside the button */}
            {(showClear || invalid) && (
              <span className="absolute right-3 top-1/2 -translate-y-1/2 flex items-center gap-1 pointer-events-auto">
                {showClear && (
                  <span
                    role="button"
                    tabIndex={-1}
                    aria-label="Clear"
                    onClick={e => handleClear(e)}
                    onKeyDown={e => {
                      if (e.key === 'Enter' || e.key === ' ') {
                        e.preventDefault();
                        e.stopPropagation();
                        handleClear();
                      }
                    }}
                    className="p-1 rounded hover:bg-gray-100 focus:outline-none cursor-pointer"
                    style={{ pointerEvents: 'auto' }}
                  >
                    <X className="h-4 w-4 text-gray-400 hover:text-gray-600" />
                  </span>
                )}
                {invalid && <InvalidIcon message={invalid} />}
              </span>
            )}
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="start">
          <Calendar
            mode="single"
            selected={date}
            onSelect={handleSelect}
            initialFocus
          />
        </PopoverContent>
      </Popover>
    </div>
  );
};

const DateTimeVariant: React.FC<DateTimeVariantProps> = ({
  value,
  placeholder,
  disabled,
  nullable,
  invalid,
  onDateChange,
  onTimeChange,
  format: formatProp,
  'data-testid': dataTestId,
}) => {
  const [open, setOpen] = useState(false);
  const date = value ? new Date(value) : undefined;
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
      } else {
        setLocalTimeValue('00:00:00');
      }
    }
  }, [date, formatProp, isEditingTime]);

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
    <div className="relative flex items-center gap-2">
      <Popover open={open} onOpenChange={setOpen}>
        <PopoverTrigger asChild>
          <Button
            disabled={disabled}
            variant="outline"
            className={cn(
              'w-full justify-start text-left font-normal pr-20 cursor-pointer', // pr-20 for clear+icon
              !date && 'text-muted-foreground',
              invalid && inputStyles.invalidInput,
              disabled && 'cursor-not-allowed'
            )}
            data-testid={dataTestId}
          >
            <CalendarIcon className="mr-2 h-4 w-4" />
            {date ? (
              format(date, formatProp || 'yyyy-MM-dd')
            ) : (
              <span>{placeholder || 'Pick a date & time'}</span>
            )}
            {/* Icons absolutely positioned inside the button */}
            {(showClear || invalid) && (
              <span className="absolute right-3 top-1/2 -translate-y-1/2 flex items-center gap-1 pointer-events-auto">
                {showClear && (
                  <button
                    type="button"
                    tabIndex={-1}
                    aria-label="Clear"
                    onClick={handleClear}
                    className="p-1 rounded hover:bg-accent focus:outline-none cursor-pointer"
                    style={{ pointerEvents: 'auto' }}
                  >
                    <X className="h-4 w-4 text-muted-foreground hover:text-foreground" />
                  </button>
                )}
                {invalid && <InvalidIcon message={invalid} />}
              </span>
            )}
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="start">
          <div className="flex flex-col gap-2 p-2">
            <Calendar
              mode="single"
              selected={date}
              onSelect={handleDateSelect}
              initialFocus
            />
            <div className="flex items-center gap-2">
              <Clock className="h-4 w-4 text-muted-foreground" />
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
                  'bg-background appearance-none [&::-webkit-calendar-picker-indicator]:hidden',
                  invalid && inputStyles.invalidInput
                )}
                data-testid={dataTestId ? `${dataTestId}-time` : undefined}
              />
            </div>
          </div>
        </PopoverContent>
      </Popover>
    </div>
  );
};

const TimeVariant: React.FC<TimeVariantProps> = ({
  value,
  placeholder,
  disabled,
  nullable,
  invalid,
  onTimeChange,
  'data-testid': dataTestId,
}) => {
  // Use local state for the input value to make it uncontrolled
  const [localTimeValue, setLocalTimeValue] = useState(() => {
    if (value) {
      // Parse the value to get time in HH:mm:ss format
      try {
        const date = new Date(value);
        if (!isNaN(date.getTime())) {
          return format(date, 'HH:mm:ss');
        }
      } catch {
        // If parsing fails, try to use the value directly if it looks like a time
        if (
          typeof value === 'string' &&
          /^\d{1,2}:\d{2}(:\d{2})?$/.test(value)
        ) {
          return value.length <= 5 ? value + ':00' : value;
        }
      }
    }
    return '00:00:00';
  });

  // Update local state when value prop changes (from parent)
  React.useEffect(() => {
    if (value) {
      try {
        const date = new Date(value);
        if (!isNaN(date.getTime())) {
          const newTimeValue = format(date, 'HH:mm:ss');
          setLocalTimeValue(newTimeValue);
        }
      } catch {
        // If parsing fails, try to use the value directly if it looks like a time
        if (
          typeof value === 'string' &&
          /^\d{1,2}:\d{2}(:\d{2})?$/.test(value)
        ) {
          const newTimeValue = value.length <= 5 ? value + ':00' : value;
          setLocalTimeValue(newTimeValue);
        }
      }
    } else {
      setLocalTimeValue('00:00:00');
    }
  }, [value]);

  const showClear = nullable && !disabled && value != null && value !== '';

  const handleClear = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    onTimeChange('');
  };

  const handleTimeChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const newTimeValue = e.target.value;
      setLocalTimeValue(newTimeValue);
    },
    []
  );

  const handleTimeBlur = useCallback(() => {
    // When input loses focus, update the parent
    onTimeChange(localTimeValue);
  }, [localTimeValue, onTimeChange]);

  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent<HTMLInputElement>) => {
      // When user presses Enter, update the parent
      if (e.key === 'Enter') {
        onTimeChange(localTimeValue);
      }
    },
    [localTimeValue, onTimeChange]
  );

  return (
    <div className="relative flex items-center gap-2" data-testid={dataTestId}>
      <Clock className="h-4 w-4 text-muted-foreground" />
      <div className="relative w-full">
        <Input
          type="time"
          step="1"
          value={localTimeValue}
          onChange={handleTimeChange}
          onBlur={handleTimeBlur}
          onKeyDown={handleKeyDown}
          disabled={disabled}
          placeholder={placeholder || 'Select time'}
          className={cn(
            'bg-background appearance-none [&::-webkit-calendar-picker-indicator]:hidden pr-20 cursor-pointer', // pr-20 for clear+icon
            invalid && inputStyles.invalidInput,
            disabled && 'cursor-not-allowed'
          )}
        />
        {(showClear || invalid) && (
          <span className="absolute right-3 top-1/2 -translate-y-1/2 flex items-center gap-1 pointer-events-auto">
            {showClear && (
              <button
                type="button"
                tabIndex={-1}
                aria-label="Clear"
                onClick={handleClear}
                className="p-1 rounded hover:bg-accent focus:outline-none cursor-pointer"
                style={{ pointerEvents: 'auto' }}
              >
                <X className="h-4 w-4 text-muted-foreground hover:text-foreground" />
              </button>
            )}
            {invalid && <InvalidIcon message={invalid} />}
          </span>
        )}
      </div>
    </div>
  );
};

const VariantComponents = {
  Date: DateVariant,
  DateTime: DateTimeVariant,
  Time: TimeVariant,
};

export const DateTimeInputWidget: React.FC<DateTimeInputWidgetProps> = ({
  id,
  value,
  placeholder,
  disabled = false,
  variant = 'Date',
  nullable = false,
  invalid,
  format: formatProp,
  'data-testid': dataTestId,
}) => {
  const eventHandler = useEventHandler();

  // Normalize undefined to null when nullable
  const normalizedValue = nullable && value === undefined ? undefined : value;

  const handleDateChange = useCallback(
    (selectedDate: Date | undefined) => {
      if (disabled) return;
      const isoString = selectedDate?.toISOString();
      eventHandler('OnChange', id, [isoString]);
    },
    [disabled, eventHandler, id]
  );

  const handleTimeChange = useCallback(
    (time: string) => {
      if (disabled) return;

      // For Time variant, send the time string directly
      if (variant === 'Time') {
        eventHandler('OnChange', id, [time]);
      } else {
        // For other variants, create a date with the selected time
        const [hours, minutes, seconds] = time.split(':').map(Number);
        const newDateTime = new Date();
        newDateTime.setHours(hours, minutes, seconds);

        eventHandler('OnChange', id, [newDateTime.toISOString()]);
      }
    },
    [disabled, eventHandler, id, variant]
  );

  const VariantComponent = useMemo(() => VariantComponents[variant], [variant]);

  return (
    <VariantComponent
      id={id}
      value={normalizedValue}
      placeholder={placeholder}
      disabled={disabled}
      nullable={nullable}
      invalid={invalid}
      format={formatProp}
      onDateChange={handleDateChange}
      onTimeChange={handleTimeChange}
      data-testid={dataTestId}
    />
  );
};
