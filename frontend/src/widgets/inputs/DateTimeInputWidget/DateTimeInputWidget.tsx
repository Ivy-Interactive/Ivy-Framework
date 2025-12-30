import * as React from 'react';
import { useCallback, useMemo } from 'react';
import { useEventHandler } from '@/components/event-handler';
import { Scales } from '@/types/scale';
import { DateTimeInputWidgetProps } from './types';
import { DateVariant } from './DateVariant';
import { DateTimeVariant } from './DateTimeVariant';
import { TimeVariant } from './TimeVariant';

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
  scale = Scales.Medium,
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
      scale={scale}
      onDateChange={handleDateChange}
      onTimeChange={handleTimeChange}
      data-testid={dataTestId}
    />
  );
};
