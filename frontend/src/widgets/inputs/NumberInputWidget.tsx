import { memo, useCallback, useMemo } from 'react';
import { useEventHandler, EventHandler } from '@/components/event-handler';
import NumberInput from '@/components/NumberInput';
import { Slider } from '@/components/ui/slider';
import { cn } from '@/lib/utils';
import { inputStyles } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';
import { X } from 'lucide-react';
import React from 'react';
import { Scales } from '@/types/scale';

const formatStyleMap = {
  Decimal: 'decimal',
  Currency: 'currency',
  Percent: 'percent',
} as const;

type FormatStyle = keyof typeof formatStyleMap;

// Type limits for validation
const TYPE_LIMITS = {
  byte: { min: 0, max: 255 },
  sbyte: { min: -128, max: 127 },
  short: { min: -32768, max: 32767 },
  ushort: { min: 0, max: 65535 },
  int: { min: -2147483648, max: 2147483647 },
  uint: { min: 0, max: 4294967295 },
  long: { min: Number.MIN_SAFE_INTEGER, max: Number.MAX_SAFE_INTEGER },
  ulong: { min: 0, max: Number.MAX_SAFE_INTEGER },
  float: { min: -999999999999.99, max: 999999999999.99 },
  double: { min: -999999999999.99, max: 999999999999.99 },
  decimal: { min: -999999999999.99, max: 999999999999.99 },
} as const;

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
  'data-testid'?: string;
  // Add type information for validation
  targetType?: string;
  scale?: Scales;
}

interface NumberInputWidgetProps
  extends Omit<NumberInputBaseProps, 'onValueChange'> {
  variant?: 'Default' | 'Slider';
  targetType?: string;
}

// Function to validate and cap values based on target type
const validateAndCapValue = (
  value: number | null,
  targetType?: string
): number | null => {
  if (value === null) return null;
  if (!targetType) return value;

  const limits = TYPE_LIMITS[targetType as keyof typeof TYPE_LIMITS];
  if (!limits) return value;

  // Cap the value to the type limits
  const cappedValue = Math.min(Math.max(value, limits.min), limits.max);

  // For integer types, ensure we don't send fractional values
  if (
    [
      'byte',
      'sbyte',
      'short',
      'ushort',
      'int',
      'uint',
      'long',
      'ulong',
    ].includes(targetType)
  ) {
    return Math.floor(cappedValue);
  }

  return cappedValue;
};

// Size variants for text styling
const sizeVariants: Record<string, { text: string }> = {
  Small: {
    text: 'text-xs',
  },
  Medium: {
    text: 'text-sm font-normal',
  },
  Large: {
    text: 'text-ml font-medium',
  },
};

const SliderVariant = memo(
  ({
    value,
    min = 0,
    max = 100,
    step = 1,
    disabled = false,
    invalid,
    currency,
    scale = Scales.Medium,
    onValueChange,
    'data-testid': dataTestId,
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
    const handleSliderCommit = useCallback(
      (values: number[]) => {
        const newValue = values[0];
        if (typeof newValue === 'number') {
          onValueChange(newValue);
        }
      },
      [onValueChange]
    );

    // For slider, we need a numeric value - use 0 as fallback for null
    const sliderValue = localValue ?? 0;

    return (
      <div className="relative w-full flex-1 flex flex-col gap-1 pt-6 pb-2 my-auto justify-center">
        <Slider
          min={min}
          max={max}
          step={step}
          value={[sliderValue]}
          disabled={disabled}
          currency={currency}
          scale={scale}
          onValueChange={handleSliderChange}
          onValueCommit={handleSliderCommit}
          className={cn(invalid && inputStyles.invalidInput)}
          data-testid={dataTestId}
        />
        <span
          className={cn(
            'flex w-full items-center justify-between gap-1',
            sizeVariants[String(scale)].text
          )}
          aria-hidden="true"
        >
          {min !== undefined && max !== undefined && (
            <>
              <span>{min}</span>
              <span>{max}</span>
            </>
          )}
        </span>
        {invalid && (
          <div className="absolute right-2.5 translate-y-1/2 -top-1.5">
            <InvalidIcon message={invalid} />
          </div>
        )}
      </div>
    );
  }
);

SliderVariant.displayName = 'SliderVariant';

const NumberVariant = memo(
  ({
    placeholder = '',
    value,
    min,
    max,
    step = 1,
    formatStyle = 'Decimal',
    precision = 2,
    disabled = false,
    invalid,
    nullable = false,
    onValueChange,
    currency,
    scale = Scales.Medium,
    'data-testid': dataTestId,
  }: NumberInputBaseProps) => {
    const formatConfig = useMemo(
      () => ({
        style: formatStyleMap[formatStyle],
        minimumFractionDigits: 0,
        maximumFractionDigits: precision,
        useGrouping: true,
        notation: 'standard' as const,
        currency: currency || undefined,
      }),
      [currency, formatStyle, precision]
    );

    const handleNumberChange = useCallback(
      (newValue: number | null) => {
        // If not nullable and value is null, convert to 0
        if (!nullable && newValue === null) {
          onValueChange(0);
        } else {
          onValueChange(newValue);
        }
      },
      [onValueChange, nullable]
    );

    return (
      <div className="relative w-full flex-1">
        <NumberInput
          min={min}
          max={max}
          step={step}
          format={formatConfig}
          placeholder={placeholder}
          value={value}
          disabled={disabled}
          scale={scale}
          onChange={handleNumberChange}
          className={cn(
            invalid && inputStyles.invalidInput,
            // Add padding for icon container
            ((nullable && value !== null && !disabled) || invalid) && 'pr-12'
          )}
          data-testid={dataTestId}
        />
        {/* Icon container - flex row aligned to right */}
        {((nullable && value !== null && !disabled) || invalid) && (
          <div className="absolute right-2 top-1/2 -translate-y-1/2 flex flex-row items-center gap-1">
            {/* Clear (X) button - leftmost */}
            {nullable && value !== null && !disabled && (
              <button
                type="button"
                tabIndex={-1}
                aria-label="Clear"
                onClick={() => onValueChange(null)}
                className="p-1 rounded hover:bg-accent focus:outline-none cursor-pointer"
              >
                <X className="h-4 w-4 text-muted-foreground hover:text-foreground" />
              </button>
            )}
            {/* Invalid icon - rightmost */}
            {invalid && <InvalidIcon message={invalid} />}
          </div>
        )}
      </div>
    );
  }
);

NumberVariant.displayName = 'NumberVariant';

export const NumberInputWidget = memo(
  ({
    id,
    variant = 'Default',
    nullable = false,
    ...props
  }: NumberInputWidgetProps) => {
    const eventHandler = useEventHandler() as EventHandler;

    // Normalize undefined to null when nullable
    const normalizedValue =
      nullable && props.value === undefined ? null : props.value;

    const handleChange = useCallback(
      (newValue: number | null) => {
        // Apply bounds only if value is not null
        if (newValue !== null) {
          // First apply component-level bounds (min/max props) only when provided
          let boundedValue = newValue;
          if (props.min !== undefined) {
            boundedValue = Math.max(boundedValue, props.min);
          }
          if (props.max !== undefined) {
            boundedValue = Math.min(boundedValue, props.max);
          }

          // Then apply type-level validation to prevent overflow
          const validatedValue = validateAndCapValue(
            boundedValue,
            props.targetType
          );

          eventHandler('OnChange', id, [validatedValue]);
        } else {
          // Pass null directly for nullable inputs
          eventHandler('OnChange', id, [newValue]);
        }
      },
      [eventHandler, id, props.min, props.max, props.targetType]
    );

    return variant === 'Slider' ? (
      <SliderVariant
        id={id}
        {...props}
        value={normalizedValue}
        onValueChange={handleChange}
      />
    ) : (
      <NumberVariant
        id={id}
        {...props}
        value={normalizedValue}
        nullable={nullable}
        onValueChange={handleChange}
      />
    );
  }
);

NumberInputWidget.displayName = 'NumberInputWidget';
