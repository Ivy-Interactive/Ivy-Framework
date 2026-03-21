import * as React from 'react';
import { useState, useCallback, useMemo } from 'react';
import { Input } from '@/components/ui/input';
import { format } from 'date-fns';
import { Clock } from 'lucide-react';
import { cn } from '@/lib/utils';
import { inputStyles } from '@/lib/styles';
import { Densities } from '@/types/density';
import {
  dateTimeInputIconVariant,
  dateTimeInputTextVariant,
} from '@/components/ui/input/date-time-input-variant';
import { TimeVariantProps } from './types';
import { ClearAndInvalidIcons } from './shared';
import {
  formatSecondsToHms,
  parseLocalTimeToSeconds,
  parseTimeSpanStepToSeconds,
  snapLocalTimeSeconds,
} from './timeStepSnap';

export const TimeVariant: React.FC<TimeVariantProps> = ({
  value,
  placeholder,
  disabled,
  nullable,
  invalid,
  onTimeChange,
  min,
  max,
  step,
  density = Densities.Medium,
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
    // When nullable and no value, return empty string to show placeholder
    return nullable ? '' : '00:00:00';
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
      // When nullable and no value, keep input empty instead of defaulting to '00:00:00'
      setLocalTimeValue(nullable ? '' : '00:00:00');
    }
  }, [value, nullable]);

  const timeStepSeconds = useMemo(
    () => parseTimeSpanStepToSeconds(step),
    [step]
  );

  const timeMin = useMemo(() => {
    if (!min) return undefined;
    try {
      const d = new Date(min);
      if (!isNaN(d.getTime())) return format(d, 'HH:mm:ss');
    } catch {
      /* ignore */
    }
    return undefined;
  }, [min]);

  const timeMax = useMemo(() => {
    if (!max) return undefined;
    try {
      const d = new Date(max);
      if (!isNaN(d.getTime())) return format(d, 'HH:mm:ss');
    } catch {
      /* ignore */
    }
    return undefined;
  }, [max]);

  const showClear = nullable && !disabled && value != null && value !== '';

  const handleClear = (e?: React.MouseEvent) => {
    e?.preventDefault();
    e?.stopPropagation();
    onTimeChange('');
  };

  const handleTimeChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const newTimeValue = e.target.value;
      setLocalTimeValue(newTimeValue);
    },
    []
  );

  const commitSnappedTime = useCallback(() => {
    const stepSec = parseTimeSpanStepToSeconds(step);
    if (nullable && localTimeValue.trim() === '') {
      onTimeChange('');
      return;
    }
    const parsed = parseLocalTimeToSeconds(localTimeValue);
    if (parsed === null) {
      onTimeChange(localTimeValue);
      return;
    }

    const minSec = timeMin
      ? (parseLocalTimeToSeconds(timeMin) ?? undefined)
      : undefined;
    const maxSec = timeMax
      ? (parseLocalTimeToSeconds(timeMax) ?? undefined)
      : undefined;
    const snapped = snapLocalTimeSeconds(parsed, stepSec, minSec, maxSec);
    const out = formatSecondsToHms(snapped);
    setLocalTimeValue(out);
    onTimeChange(out);
  }, [step, nullable, localTimeValue, timeMin, timeMax, onTimeChange]);

  const handleTimeBlur = useCallback(() => {
    commitSnappedTime();
  }, [commitSnappedTime]);

  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent<HTMLInputElement>) => {
      if (e.key === 'Enter') {
        e.preventDefault();
        e.currentTarget.blur();
      }
    },
    []
  );

  return (
    <div className="relative w-full select-none" data-testid={dataTestId}>
      <div
        className={cn(
          'relative flex items-center rounded-md border border-input bg-transparent shadow-sm focus-within:ring-1 focus-within:ring-ring dark:bg-white/5 dark:border-white/10',
          invalid && inputStyles.invalidInput
        )}
      >
        <Clock
          className={cn(
            'ml-3 shrink-0',
            dateTimeInputIconVariant({ density }),
            disabled && 'opacity-50'
          )}
        />
        <Input
          type="time"
          step={timeStepSeconds}
          min={timeMin}
          max={timeMax}
          density={density}
          value={localTimeValue}
          onChange={handleTimeChange}
          onBlur={handleTimeBlur}
          onKeyDown={handleKeyDown}
          disabled={disabled}
          placeholder={placeholder || 'Select time'}
          className={cn(
            'bg-transparent appearance-none [&::-webkit-calendar-picker-indicator]:hidden cursor-pointer w-full border-0 shadow-none focus-visible:ring-0',
            dateTimeInputTextVariant({ density }),
            invalid && inputStyles.invalidInput,
            disabled && 'cursor-not-allowed',
            showClear && invalid ? 'pr-16' : showClear || invalid ? 'pr-8' : ''
          )}
        />
      </div>
      <ClearAndInvalidIcons
        showClear={showClear}
        invalid={invalid}
        density={density}
        onClear={handleClear}
      />
    </div>
  );
};
