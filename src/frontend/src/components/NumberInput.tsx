import { Input } from "@/components/ui/input";
import { Densities } from "@/types/density";
import React, {
  useState,
  useCallback,
  useMemo,
  useEffect,
  ChangeEvent,
  FocusEvent,
  WheelEvent,
} from "react";
import { formatBytes } from "@/lib/formatters";

interface NumberInputProps {
  min?: number;
  max?: number;
  step?: number;
  disabled?: boolean;
  placeholder?: string;
  value: number | null;
  onChange?: (value: number | null) => void;
  onBlur?: (e: FocusEvent<HTMLInputElement>) => void;
  onFocus?: (e: FocusEvent<HTMLInputElement>) => void;
  format?: Intl.NumberFormatOptions;
  isBytesFormat?: boolean;
  allowNegative?: boolean;
  autoFocus?: boolean;
  className?: string;
  density?: Densities;
  "data-testid"?: string;
}

const NumberInput = React.forwardRef<HTMLInputElement, NumberInputProps>(
  (
    {
      min,
      max,
      step = 1,
      disabled = false,
      placeholder = "",
      value = null,
      onChange,
      onBlur,
      onFocus,
      format = {
        style: "decimal",
        minimumFractionDigits: 0,
        maximumFractionDigits: 2,
        useGrouping: true,
        notation: "standard",
      },
      isBytesFormat = false,
      allowNegative = true,
      className = "",
      density = Densities.Medium,
      "data-testid": dataTestId,
      ...props
    },
    ref,
  ) => {
    const [displayValue, setDisplayValue] = useState<string>("");
    const [isFocused, setIsFocused] = useState(false);
    const [isValid, setIsValid] = useState(true);
    const inputRef = React.useRef<HTMLInputElement | null>(null);

    const formatter = useMemo(() => new Intl.NumberFormat(undefined, format), [format]);

    const formatValue = useCallback(
      (num: number | null): string => {
        if (num === null) return "";
        try {
          if (isFocused) return num.toString();
          if (isBytesFormat) return formatBytes(num, format.maximumFractionDigits ?? 2);
          return formatter.format(num);
        } catch {
          return num.toString();
        }
      },
      [formatter, isFocused, isBytesFormat, format.maximumFractionDigits],
    );

    const parseValue = useCallback(
      (input: string, shouldRound = true): number | null => {
        if (!input) return null;

        const cleaned = input.replace(/[^\d.-]/g, "");
        const parts = cleaned.split(".");
        const sanitized = parts[0] + (parts.length > 1 ? "." + parts[1] : "");
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
      [min, max, step, allowNegative],
    );

    const handleStep = useCallback(
      (multiplier: number) => {
        if (disabled) return;

        const current = value ?? 0;
        const newValue = parseValue((current + step * multiplier).toString(), true);

        if (newValue !== null) {
          if (max !== undefined && newValue > max) return;
          if (min !== undefined && newValue < min) return;

          onChange?.(newValue);
          setDisplayValue(formatValue(newValue));
        }
      },
      [value, step, parseValue, onChange, formatValue, disabled, min, max],
    );

    const handleKeyDown = useCallback(
      (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (disabled) return;

        switch (e.key) {
          case "ArrowUp":
            e.preventDefault();
            handleStep(1);
            break;
          case "ArrowDown":
            e.preventDefault();
            handleStep(-1);
            break;
        }
      },
      [disabled, handleStep],
    );

    const handleWheel = useCallback(
      (e: WheelEvent<HTMLInputElement>) => {
        if (!isFocused || disabled) return;

        e.preventDefault();
        const delta = -Math.sign(e.deltaY);
        handleStep(delta);
      },
      [isFocused, disabled, handleStep],
    );

    const handleChange = useCallback(
      (e: ChangeEvent<HTMLInputElement>) => {
        const inputValue = e.target.value;
        if (inputValue === "") {
          setDisplayValue("");
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
      [parseValue, onChange],
    );

    const handleFocus = useCallback(
      (e: FocusEvent<HTMLInputElement>) => {
        setIsFocused(true);
        setDisplayValue(value?.toString() ?? "");
        onFocus?.(e);
      },
      [value, onFocus],
    );

    const handleBlur = useCallback(
      (e: FocusEvent<HTMLInputElement>) => {
        setIsFocused(false);
        if (value === null) {
          setDisplayValue("");
        } else {
          setDisplayValue(formatValue(value));
        }
        onBlur?.(e);
      },
      [formatValue, value, onBlur],
    );

    // Synchronize displayValue with external value changes when not focused
    useEffect(() => {
      if (!isFocused) {
        setDisplayValue(formatValue(value));
      }
    }, [value, isFocused, formatValue]);

    // Update display value when not focused and value changes
    const displayValueToUse = isFocused ? displayValue : formatValue(value);

    return (
      <div className="relative">
        <Input
          ref={(node) => {
            inputRef.current = node;
            if (typeof ref === "function") {
              ref(node);
            } else if (ref) {
              ref.current = node;
            }
          }}
          type="text"
          inputMode="decimal"
          value={displayValueToUse}
          onChange={handleChange}
          onFocus={handleFocus}
          onBlur={handleBlur}
          onKeyDown={handleKeyDown}
          onWheel={handleWheel}
          min={min}
          max={max}
          step={step}
          disabled={disabled}
          placeholder={placeholder}
          density={density}
          className={`${className} ${!isValid ? "border-[var(--color-destructive)]" : ""}`}
          data-testid={dataTestId}
          {...props}
        />
      </div>
    );
  },
);

NumberInput.displayName = "NumberInput";

export default NumberInput;
