import * as React from 'react';
import { useState, useCallback } from 'react';
import { Button } from '@/components/ui/button';
import { Calendar } from '@/components/ui/calendar';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import { format } from 'date-fns';
import { Calendar as CalendarIcon } from 'lucide-react';
import { cn } from '@/lib/utils';
import { inputStyles } from '@/lib/styles';
import { Scales } from '@/types/scale';
import {
  dateTimeInputVariant,
  dateTimeInputIconVariant,
  dateTimeInputTextVariant,
} from '@/components/ui/input/date-time-input-variant';
import { DateVariantProps } from './types';
import { ClearAndInvalidIcons } from './shared';

export const DateVariant: React.FC<DateVariantProps> = ({
  value,
  placeholder,
  disabled,
  nullable,
  invalid,
  onDateChange,
  format: formatProp,
  scale = Scales.Medium,
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
    <div className="relative w-full select-none">
      <Popover open={open} onOpenChange={setOpen}>
        <PopoverTrigger asChild>
          <Button
            disabled={disabled}
            variant="outline"
            data-slot="calendar"
            className={cn(
              dateTimeInputVariant({ scale }),
              'dark:bg-white/5 dark:hover:bg-white/10 dark:border-white/10',
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
                dateTimeInputIconVariant({ scale })
              )}
            />
            <span
              className={cn(
                'truncate',
                dateTimeInputTextVariant({ scale }),
                !date && 'text-muted-foreground'
              )}
            >
              {date
                ? format(date, formatProp || 'yyyy-MM-dd')
                : placeholder || 'Pick a date'}
            </span>
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="start">
          <Calendar
            mode="single"
            selected={date}
            onSelect={handleSelect}
            initialFocus
            scale={scale}
          />
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
