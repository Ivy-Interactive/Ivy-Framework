import React, { memo, useCallback, useMemo } from "react";
import { useEventHandler, EventHandler } from "@/components/event-handler";
import { useOptimisticValue } from "./shared/useOptimisticValue";
import * as SliderPrimitive from "@radix-ui/react-slider";
import { cn } from "@/lib/utils";
import { inputStyles } from "@/lib/styles";
import { InvalidIcon } from "@/components/InvalidIcon";
import { X } from "lucide-react";
import { Densities } from "@/types/density";
import Icon from "@/components/Icon";
import { formatBytes } from "@/lib/formatters";
import { EMPTY_ARRAY } from "@/lib/constants";

interface Affix {
  icon?: string;
  text?: string;
}

const renderAffix = (affix?: Affix): React.ReactNode => {
  if (!affix) return null;

  if (affix.icon) {
    return React.createElement(Icon, {
      name: affix.icon,
      className: "w-4 h-4",
    });
  }

  if (affix.text) {
    return React.createElement("span", { className: "text-sm" }, affix.text);
  }

  return null;
};

const formatStyleMap = {
  Decimal: "decimal",
  Currency: "currency",
  Percent: "percent",
  Compact: "compact",
  Scientific: "scientific",
  Engineering: "engineering",
  Accounting: "accounting",
  Bytes: "bytes",
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

interface NumberRangeInputWidgetProps {
  id: string;
  lowerValue: number | null;
  upperValue: number | null;
  min?: number;
  max?: number;
  step?: number;
  precision?: number;
  formatStyle?: FormatStyle;
  currency?: string;
  disabled?: boolean;
  invalid?: string;
  nullable?: boolean;
  prefix?: Affix;
  suffix?: Affix;
  noGrouping?: boolean;
  targetType?: string;
  density?: Densities;
  events: string[];
  "data-testid"?: string;
}

// Function to validate and cap values based on target type
const validateAndCapValue = (value: number | null, targetType?: string): number | null => {
  if (value === null) return null;
  if (!targetType) return value;

  const limits = TYPE_LIMITS[targetType as keyof typeof TYPE_LIMITS];
  if (!limits) return value;

  // Cap the value to the type limits
  const cappedValue = Math.min(Math.max(value, limits.min), limits.max);

  // For integer types, ensure we don't send fractional values
  if (["byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong"].includes(targetType)) {
    return Math.floor(cappedValue);
  }

  return cappedValue;
};

// Format a number according to the specified format style
const formatNumber = (
  value: number | null,
  formatStyle: FormatStyle,
  precision: number,
  currency?: string,
  noGrouping?: boolean,
): string => {
  if (value === null) return "";

  if (formatStyle === "Bytes") {
    return formatBytes(value, precision);
  }

  try {
    const config: Intl.NumberFormatOptions = {
      minimumFractionDigits: 0,
      maximumFractionDigits: precision,
      useGrouping: !(noGrouping ?? false),
      notation: "standard",
    };

    if (formatStyle === "Compact") {
      config.notation = "compact";
      config.compactDisplay = "short";
    } else if (formatStyle === "Scientific") {
      config.notation = "scientific";
    } else if (formatStyle === "Engineering") {
      config.notation = "engineering";
    } else if (formatStyle === "Accounting") {
      config.style = "currency";
      config.currencySign = "accounting";
      config.currency = currency || "USD";
    } else {
      config.style = formatStyleMap[formatStyle] as Intl.NumberFormatOptions["style"];
      if (formatStyle === "Currency" && currency) {
        config.currency = currency;
      }
    }

    return new Intl.NumberFormat("en-US", config).format(value);
  } catch {
    return String(value);
  }
};

// Size variants
const sizeVariant: Record<string, { track: string; thumb: string; tooltip: string; text: string }> =
  {
    Small: {
      track: "h-1",
      thumb: "h-3 w-3",
      tooltip: "text-xs -top-6",
      text: "text-xs",
    },
    Medium: {
      track: "h-1.5",
      thumb: "h-4 w-4",
      tooltip: "text-sm -top-7",
      text: "text-sm font-normal",
    },
    Large: {
      track: "h-2",
      thumb: "h-5 w-5",
      tooltip: "text-ml -top-8",
      text: "text-ml font-medium",
    },
  };

export const NumberRangeInputWidget = memo(
  ({
    id,
    lowerValue,
    upperValue,
    min = 0,
    max = 100,
    step = 1,
    precision = 2,
    formatStyle = "Decimal",
    currency,
    disabled = false,
    invalid,
    nullable = false,
    prefix,
    suffix,
    noGrouping,
    targetType,
    density = Densities.Medium,
    events = EMPTY_ARRAY,
    "data-testid": dataTestId,
  }: NumberRangeInputWidgetProps) => {
    const eventHandler = useEventHandler() as EventHandler;

    // Normalize null values to min/max for slider display
    const normalizedLower = lowerValue ?? min;
    const normalizedUpper = upperValue ?? max;

    type RangeTuple = { lower: number; upper: number };
    const serverRange: RangeTuple = {
      lower: normalizedLower,
      upper: normalizedUpper,
    };

    const rangeEqual = (a: RangeTuple, b: RangeTuple): boolean =>
      a.lower === b.lower && a.upper === b.upper;

    const [localRange, setLocalRange] = useOptimisticValue(serverRange, false, rangeEqual);

    // Maintains local state for lower/upper for instant feedback
    const [localLower, setLocalLower] = useOptimisticValue(localRange.lower, false);
    const [localUpper, setLocalUpper] = useOptimisticValue(localRange.upper, false);

    // Only update local state on drag
    const handleSliderChange = useCallback(
      (values: number[]) => {
        const [newLower, newUpper] = values;
        if (typeof newLower === "number" && typeof newUpper === "number") {
          setLocalLower(newLower);
          setLocalUpper(newUpper);
        }
      },
      [setLocalLower, setLocalUpper],
    );

    // Only call eventHandler when drag ends
    const handleSliderCommit = useCallback(
      (values: number[]) => {
        if (!events.includes("OnChange") || disabled) return;

        let [newLower, newUpper] = values;

        // Apply bounds
        if (min !== undefined) {
          newLower = Math.max(newLower, min);
          newUpper = Math.max(newUpper, min);
        }
        if (max !== undefined) {
          newLower = Math.min(newLower, max);
          newUpper = Math.min(newUpper, max);
        }

        // Ensure lower <= upper
        if (newLower > newUpper) {
          [newLower, newUpper] = [newUpper, newLower];
        }

        // Apply type validation
        const validatedLower = validateAndCapValue(newLower, targetType);
        const validatedUpper = validateAndCapValue(newUpper, targetType);

        // Optimistic update for committed values
        const newRange = {
          lower: validatedLower ?? min,
          upper: validatedUpper ?? max,
        };
        setLocalRange(newRange);

        // Send as tuple
        eventHandler("OnChange", id, [{ item1: validatedLower, item2: validatedUpper }]);
      },
      [eventHandler, id, min, max, targetType, disabled, events, setLocalRange],
    );

    const handleClear = useCallback(
      (e: React.MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();
        if (!events.includes("OnChange") || disabled) return;

        eventHandler("OnChange", id, [{ item1: null, item2: null }]);
      },
      [eventHandler, id, disabled, events],
    );

    const variant = sizeVariant[String(density)];

    const formattedLower = useMemo(
      () => formatNumber(localLower, formatStyle, precision, currency, noGrouping),
      [localLower, formatStyle, precision, currency, noGrouping],
    );

    const formattedUpper = useMemo(
      () => formatNumber(localUpper, formatStyle, precision, currency, noGrouping),
      [localUpper, formatStyle, precision, currency, noGrouping],
    );

    const showClear = nullable && !disabled && (lowerValue !== null || upperValue !== null);

    const prefixContent = renderAffix(prefix);
    const suffixContent = renderAffix(suffix);

    return (
      <div
        className={cn(
          "relative w-full px-3 py-2",
          disabled && "opacity-50 cursor-not-allowed",
          invalid && inputStyles.invalidInput,
        )}
        onBlur={(e) => {
          if (disabled) return;
          if (!e.currentTarget.contains(e.relatedTarget)) {
            if (events.includes("OnBlur")) eventHandler("OnBlur", id, []);
          }
        }}
        onFocus={(e) => {
          if (disabled) return;
          if (!e.currentTarget.contains(e.relatedTarget)) {
            if (events.includes("OnFocus")) eventHandler("OnFocus", id, []);
          }
        }}
      >
        {/* Prefix/Suffix labels */}
        {(prefixContent || suffixContent) && (
          <div className="flex items-center justify-between mb-2">
            {prefixContent && (
              <div className="flex items-center gap-1 text-muted-foreground">{prefixContent}</div>
            )}
            {suffixContent && (
              <div className="flex items-center gap-1 text-muted-foreground">{suffixContent}</div>
            )}
          </div>
        )}

        {/* Range slider */}
        <div className="relative w-full flex flex-col gap-1 pt-6 pb-2">
          <SliderPrimitive.Root
            className={cn(
              "relative flex w-full touch-none select-none items-center",
              invalid && inputStyles.invalidInput,
            )}
            value={[localLower, localUpper]}
            onValueChange={handleSliderChange}
            onValueCommit={handleSliderCommit}
            min={min}
            max={max}
            step={step}
            disabled={disabled}
            data-testid={dataTestId}
          >
            <SliderPrimitive.Track
              className={cn(
                "relative w-full grow overflow-hidden rounded-full bg-muted",
                variant.track,
              )}
            >
              <SliderPrimitive.Range className="absolute h-full bg-primary" />
            </SliderPrimitive.Track>

            {/* Lower thumb */}
            <SliderPrimitive.Thumb
              className={cn(
                "relative block rounded-full border bg-background shadow transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-50 cursor-pointer",
                variant.thumb,
              )}
            >
              <div
                className={cn(
                  "absolute left-1/2 transform -translate-x-1/2 bg-popover text-foreground p-1 rounded shadow whitespace-nowrap",
                  variant.tooltip,
                )}
              >
                {formattedLower}
              </div>
            </SliderPrimitive.Thumb>

            {/* Upper thumb */}
            <SliderPrimitive.Thumb
              className={cn(
                "relative block rounded-full border bg-background shadow transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-50 cursor-pointer",
                variant.thumb,
              )}
            >
              <div
                className={cn(
                  "absolute left-1/2 transform -translate-x-1/2 bg-popover text-foreground p-1 rounded shadow whitespace-nowrap",
                  variant.tooltip,
                )}
              >
                {formattedUpper}
              </div>
            </SliderPrimitive.Thumb>
          </SliderPrimitive.Root>

          {/* Min/Max labels */}
          <span
            className={cn("flex w-full items-center justify-between gap-1", variant.text)}
            aria-hidden="true"
          >
            {min !== undefined && max !== undefined && (
              <>
                <span>{formatNumber(min, formatStyle, precision, currency, noGrouping)}</span>
                <span>{formatNumber(max, formatStyle, precision, currency, noGrouping)}</span>
              </>
            )}
          </span>

          {/* Clear button and invalid icon */}
          {(showClear || invalid) && (
            <div className="absolute right-2.5 -top-1.5 flex items-center gap-2">
              {showClear && (
                <button
                  type="button"
                  tabIndex={-1}
                  aria-label="Clear"
                  onClick={handleClear}
                  className="p-1 rounded hover:bg-accent focus:outline-none cursor-pointer"
                >
                  <X className="h-4 w-4 text-muted-foreground hover:text-foreground" />
                </button>
              )}
              {invalid && <InvalidIcon message={invalid} />}
            </div>
          )}
        </div>
      </div>
    );
  },
);

NumberRangeInputWidget.displayName = "NumberRangeInputWidget";
