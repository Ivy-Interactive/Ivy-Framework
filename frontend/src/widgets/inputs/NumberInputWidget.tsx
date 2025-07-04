import { memo, useCallback, useMemo } from 'react';
import { useEventHandler, EventHandler } from "@/components/EventHandlerContext";
import NumberInput from "@/components/NumberInput";
import { Slider } from '@/components/ui/slider';
import { cn } from '@/lib/utils';
import { inputStyles } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';
import { generateTestId, getTestId } from '@/lib/utils';
import React from 'react';

const formatStyleMap = {
  Decimal: 'decimal',
  Currency: 'currency',
  Percent: 'percent'
} as const;

type FormatStyle = keyof typeof formatStyleMap;

interface NumberInputBaseProps {
  id: string;
  placeholder?: string;
  value: number | null;
  formatStyle?: FormatStyle;
  min?: number;
  max?: number;
  step?: number;
  precision?: number;
  disabled?: boolean;
  invalid?: string;
  nullable?: boolean;
  onValueChange: (value: number | null) => void;
  currency?: string | undefined;
  testId?: string;
}

interface NumberInputWidgetProps extends Omit<NumberInputBaseProps, 'onValueChange'> {
  variant?: "Default" | "Slider";
  testId?: string;
}

const SliderVariant = memo(({
  value,
  min = 0,
  max = 100,
  step = 1,
  disabled = false,
  invalid,
  onValueChange,
  ...props
}: NumberInputBaseProps) => {
  // Local state for live feedback (optional, fallback to prop value)
  const [localValue, setLocalValue] = React.useState<number | null>(value);

  React.useEffect(() => {
    setLocalValue(value);
  }, [value]);

  // Only update local state on drag
  const handleSliderChange = useCallback((values: number[]) => {
    const newValue = values[0];
    if (typeof newValue === 'number') {
      setLocalValue(newValue);
    }
  }, []);

  // Only call onValueChange (eventHandler) when drag ends
  const handleSliderCommit = useCallback((values: number[]) => {
    const newValue = values[0];
    if (typeof newValue === 'number') {
      onValueChange(newValue);
    }
  }, [onValueChange]);

  // For slider, we need a numeric value - use 0 as fallback for null
  const sliderValue = localValue ?? 0;

  const testId = props.testId || generateTestId({ variant: "Slider", disabled, invalid, ...props });

  return (
    <div className="relative w-full mt-8" data-testid={getTestId(`${testId}-container`)}>
      <Slider
        min={min}
        max={max}
        step={step}
        value={[sliderValue]}
        disabled={disabled}
        onValueChange={handleSliderChange}
        onValueCommit={handleSliderCommit}
        className={cn(invalid && inputStyles.invalid)}
        data-testid={getTestId(`${testId}-slider`)}
      />
      <span
          className="mt-4 flex w-full items-center justify-between gap-1 text-xs font-sm text-muted-foreground"
          aria-hidden="true"
        >
          <span>{min}</span>
          <span>{max}</span>
        </span>
      {invalid && (
        <div className="absolute right-2.5 top-2.5">
          <InvalidIcon message={invalid} />
        </div>
      )}
    </div>
  );
});

SliderVariant.displayName = 'SliderVariant';

const NumberVariant = memo(({
  placeholder = "",
  value,
  min = 0,
  max = 100,
  step = 1,
  formatStyle = 'Decimal',
  precision = 2,
  disabled = false,
  invalid,
  nullable = false,
  onValueChange,
  currency,
  ...props
}: NumberInputBaseProps) => {
  const formatConfig = useMemo(() => ({
    style: formatStyleMap[formatStyle],
    minimumFractionDigits: 0,
    maximumFractionDigits: precision,
    useGrouping: true,
    notation: 'standard' as const,
    currency: currency || undefined
  }), [formatStyle, precision]);

  const handleNumberChange = useCallback((newValue: number | null) => {
    // If not nullable and value is null, convert to 0
    if (!nullable && newValue === null) {
      onValueChange(0);
    } else {
      onValueChange(newValue);
    }
  }, [onValueChange, nullable]);

  const testId = props.testId || generateTestId({ variant: "Default", disabled, invalid, nullable, placeholder, formatStyle, currency, min, max, step, precision, ...props });

  return (
    <div className="relative">
      <NumberInput 
        min={min}
        max={max}
        step={step}
        format={formatConfig}
        placeholder={placeholder}
        value={value}
        disabled={disabled}
        onChange={handleNumberChange}
        className={cn(
          invalid && inputStyles.invalid,
          invalid && "pr-8"
        )}
        nullable={nullable}
        data-testid={getTestId(testId)}
      />
      {invalid && (
        <div className="absolute right-8 top-2">
          <InvalidIcon message={invalid} />
        </div>
      )}
    </div>
  );
});

NumberVariant.displayName = 'NumberVariant';

export const NumberInputWidget = memo(({ 
  id, 
  variant = "Default",
  nullable = false,
  testId,
  ...props
}: NumberInputWidgetProps) => {
  const eventHandler = useEventHandler() as EventHandler;
   
  // Normalize undefined to null when nullable
  const normalizedValue = nullable && props.value === undefined ? null : props.value;
   
  const handleChange = useCallback((newValue: number | null) => {
    // Apply bounds only if value is not null
    if (newValue !== null) {
      const boundedValue = Math.min(Math.max(newValue, props.min ?? 0), props.max ?? 100);
      eventHandler("OnChange", id, [boundedValue]);
    } else {
      // Pass null directly for nullable inputs
      eventHandler("OnChange", id, [newValue]);
    }
  }, [eventHandler, id, props.min, props.max]);

  return variant === "Slider" ? (
    <SliderVariant id={id} {...props} testId={testId} value={normalizedValue} onValueChange={handleChange} />
  ) : (
    <NumberVariant id={id} {...props} testId={testId} value={normalizedValue} nullable={nullable} onValueChange={handleChange} />
  );
});

NumberInputWidget.displayName = 'NumberInputWidget';