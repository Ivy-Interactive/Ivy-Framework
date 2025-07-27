import { InvalidIcon } from '@/components/InvalidIcon';
import { Input } from '@/components/ui/input';
import { ChevronUp, ChevronDown, X } from 'lucide-react';
import React, {
  useState,
  useCallback,
  useMemo,
  useEffect,
  ChangeEvent,
  FocusEvent,
  WheelEvent,
  MouseEvent as ReactMouseEvent,
} from 'react';

interface NumberInputProps {
  min?: number;
  max?: number;
  step?: number;
  disabled?: boolean;
  placeholder?: string;
  value: number | null;
  onChange?: (value: number | null) => void;
  onBlur?: (e: FocusEvent<HTMLInputElement>) => void;
  format?: Intl.NumberFormatOptions;
  allowNegative?: boolean;
  className?: string;
  nullable?: boolean;
  showArrows?: boolean;
  'data-testid'?: string;
}

interface DragState {
  isDragging: boolean;
  startX: number;
  startValue: number;
  lastValue: number;
}

const NumberInput = React.forwardRef<HTMLInputElement, NumberInputProps>(
  (
    {
      min,
      max,
      step = 1,
      disabled = false,
      placeholder = '',
      value,
      onChange,
      onBlur,
      format = {
        style: 'decimal',
        minimumFractionDigits: 0,
        maximumFractionDigits: 2,
        useGrouping: true,
        notation: 'standard',
      },
      allowNegative = true,
      className = '',
      nullable = false,
      showArrows = false,
      'data-testid': dataTestId,
      ...props
    },
    ref
  ) => {
    const [displayValue, setDisplayValue] = useState<string>('');
    const [isFocused, setIsFocused] = useState(false);
    const [isValid, setIsValid] = useState(true);
    const [dragState, setDragState] = useState<DragState | null>(null);
    const inputRef = React.useRef<HTMLInputElement | null>(null);

    const formatter = useMemo(
      () => new Intl.NumberFormat(undefined, format),
      [format]
    );

    const formatValue = useCallback(
      (num: number | null): string => {
        if (num === null) return '';
        try {
          return isFocused ? num.toString() : formatter.format(num);
        } catch {
          return '';
        }
      },
      [formatter, isFocused]
    );

    const parseValue = useCallback(
      (input: string, shouldRound = true): number | null => {
        if (!input) return null;

        const cleaned = input.replace(/[^\d.-]/g, '');
        const parts = cleaned.split('.');
        const sanitized = parts[0] + (parts.length > 1 ? '.' + parts[1] : '');
        const parsed = parseFloat(sanitized);

        if (isNaN(parsed)) return null;
        if (!allowNegative && parsed < 0) return null;
        if (min !== undefined && parsed < min) return min;
        if (max !== undefined && parsed > max) return max;

        if (step && shouldRound) {
          const rounded = Math.round(parsed / step) * step;
          return Number(rounded.toFixed(10));
        }

        return parsed;
      },
      [min, max, step, allowNegative]
    );

    const handleStep = useCallback(
      (multiplier: number) => {
        if (disabled) return;

        const current = value ?? 0;
        const newValue = parseValue(
          (current + step * multiplier).toString(),
          true
        );

        if (newValue !== null) {
          if (max !== undefined && newValue > max) return;
          if (min !== undefined && newValue < min) return;

          onChange?.(newValue);
          setDisplayValue(formatValue(newValue));
        }
      },
      [value, step, parseValue, onChange, formatValue, disabled, min, max]
    );

    const calculateDragValue = useCallback(
      (currentX: number, startX: number, startValue: number) => {
        const distance = currentX - startX;
        const direction = distance >= 0 ? 1 : -1;
        const absoluteDistance = Math.abs(distance);

        let multiplier = 1;
        if (absoluteDistance > 100) multiplier = 5;
        if (absoluteDistance > 200) multiplier = 10;
        if (absoluteDistance > 300) multiplier = 20;

        const stepChange =
          Math.floor(absoluteDistance / 5) * direction * step * multiplier;
        return parseValue((startValue + stepChange).toString(), true);
      },
      [parseValue, step]
    );

    const handleMouseDown = useCallback(
      (e: ReactMouseEvent<HTMLInputElement>) => {
        if (disabled || !inputRef.current) return;

        const startValue = value ?? 0;
        setDragState({
          isDragging: true,
          startX: e.clientX,
          startValue,
          lastValue: startValue,
        });

        e.preventDefault();

        document.body.style.cursor = 'ew-resize';

        inputRef.current.focus();
      },
      [disabled, value]
    );

    useEffect(() => {
      if (!dragState?.isDragging) return;

      const handleMouseMove = (e: globalThis.MouseEvent) => {
        if (!dragState) return;

        const newValue = calculateDragValue(
          e.clientX,
          dragState.startX,
          dragState.startValue
        );

        if (newValue !== null && newValue !== dragState.lastValue) {
          setDisplayValue(formatValue(newValue));
          setDragState(prev =>
            prev ? { ...prev, lastValue: newValue } : null
          );
        }
      };

      const handleMouseUp = () => {
        // Call onChange only when mouse is released
        if (dragState && dragState.lastValue !== dragState.startValue) {
          onChange?.(dragState.lastValue);
        }
        setDragState(null);
        document.body.style.cursor = '';
      };

      document.addEventListener('mousemove', handleMouseMove);
      document.addEventListener('mouseup', handleMouseUp);

      return () => {
        document.removeEventListener('mousemove', handleMouseMove);
        document.removeEventListener('mouseup', handleMouseUp);
      };
    }, [dragState, calculateDragValue, onChange, formatValue]);

    const handleKeyDown = useCallback(
      (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (disabled) return;

        switch (e.key) {
          case 'ArrowUp':
            e.preventDefault();
            handleStep(1);
            break;
          case 'ArrowDown':
            e.preventDefault();
            handleStep(-1);
            break;
        }
      },
      [disabled, handleStep]
    );

    const handleWheel = useCallback(
      (e: WheelEvent<HTMLInputElement>) => {
        if (!isFocused || disabled) return;

        e.preventDefault();
        const delta = -Math.sign(e.deltaY);
        handleStep(delta);
      },
      [isFocused, disabled, handleStep]
    );

    const handleChange = useCallback(
      (e: ChangeEvent<HTMLInputElement>) => {
        const inputValue = e.target.value;
        if (inputValue === '') {
          setDisplayValue('');
          setIsValid(true);
          onChange?.(null);
          return;
        }

        const newValue = parseValue(inputValue, false);
        setDisplayValue(inputValue);
        setIsValid(newValue !== null);

        if (newValue !== null) {
          onChange?.(newValue);
        }
      },
      [parseValue, onChange]
    );

    const handleFocus = useCallback(() => {
      setIsFocused(true);
      setDisplayValue(value?.toString() ?? '');
    }, [value]);

    const handleBlur = useCallback(
      (e: FocusEvent<HTMLInputElement>) => {
        setIsFocused(false);
        if (value === null) {
          setDisplayValue('');
        } else {
          setDisplayValue(formatValue(value));
        }
        onBlur?.(e);
      },
      [formatValue, value, onBlur]
    );

    useEffect(() => {
      if (!isFocused) {
        setDisplayValue(formatValue(value));
      }
    }, [value, formatValue, isFocused]);

    return (
      <div className="relative">
        <Input
          ref={node => {
            inputRef.current = node;
            if (typeof ref === 'function') {
              ref(node);
            } else if (ref) {
              ref.current = node;
            }
          }}
          type="text"
          inputMode="decimal"
          value={displayValue}
          onChange={handleChange}
          onFocus={handleFocus}
          onBlur={handleBlur}
          onKeyDown={handleKeyDown}
          onWheel={handleWheel}
          onMouseDown={handleMouseDown}
          min={min}
          max={max}
          step={step}
          disabled={disabled}
          placeholder={placeholder}
          className={`${className} ${showArrows ? 'pr-14' : ''} ${
            !isValid ? 'border-red-500' : ''
          } ${dragState?.isDragging ? 'select-none' : ''} cursor-pointer`}
          data-testid={dataTestId}
          {...props}
        />
        {/* Right-side icon container */}
        {(nullable && value !== null && !disabled && onChange) || !isValid ? (
          <div
            className={`absolute top-1/2 -translate-y-1/2 flex items-center gap-1 ${
              showArrows ? 'right-14' : 'right-2'
            }`}
            style={{ zIndex: 2 }}
          >
            {/* Invalid icon */}
            {!isValid && (
              <span className="flex items-center">
                <InvalidIcon
                  message={
                    typeof placeholder === 'string'
                      ? placeholder
                      : 'Invalid value'
                  }
                />
              </span>
            )}
            {/* Clear (X) button */}
            {nullable && value !== null && !disabled && onChange && (
              <button
                type="button"
                tabIndex={-1}
                aria-label="Clear"
                onClick={() => {
                  setDisplayValue('');
                  onChange(null);
                }}
                className="p-1 rounded hover:bg-accent focus:outline-none cursor-pointer"
              >
                <X className="h-4 w-4 text-muted-foreground hover:text-foreground" />
              </button>
            )}
          </div>
        ) : null}
        {showArrows && (
          <div className="absolute right-0 top-0 bottom-0 flex flex-col border-l">
            <button
              type="button"
              tabIndex={-1}
              disabled={
                disabled ||
                (max !== undefined && value !== null && value >= max)
              }
              onClick={() => handleStep(1)}
              className="flex-1 px-1 flex items-center justify-center hover:bg-accent disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer"
            >
              <ChevronUp className="h-3 w-3" />
            </button>
            <button
              type="button"
              tabIndex={-1}
              disabled={
                disabled ||
                (min !== undefined && value !== null && value <= min)
              }
              onClick={() => handleStep(-1)}
              className="flex-1 px-1 flex items-center justify-center hover:bg-accent disabled:opacity-50 disabled:cursor-not-allowed border-t cursor-pointer"
            >
              <ChevronDown className="h-3 w-3" />
            </button>
          </div>
        )}
      </div>
    );
  }
);

NumberInput.displayName = 'NumberInput';

export default NumberInput;
