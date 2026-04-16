import { EmojiRating } from "@/components/EmojiRating";
import { useEventHandler } from "@/components/event-handler";
import { StarRating } from "@/components/StarRating";
import { ThumbsEnum, ThumbsRating } from "@/components/ui/thumbs-rating";
import React, { useCallback, useMemo } from "react";
import { inputStyles } from "@/lib/styles";
import { cn } from "@/lib/utils";
import { useOptimisticValue } from "./shared/useOptimisticValue";
import { Densities } from "@/types/density";
import { EMPTY_ARRAY } from "@/lib/constants";

interface FeedbackInputWidgetProps {
  id: string;
  value: number | boolean | null;
  variant: "Thumbs" | "Emojis" | "Stars";
  disabled: boolean;
  invalid?: string;
  events: string[];
  nullable?: boolean;
  allowHalf?: boolean;
  max?: number;
  density?: Densities;
  slots?: { Prefix?: React.ReactNode[]; Suffix?: React.ReactNode[] };
}

export const FeedbackInputWidget: React.FC<FeedbackInputWidgetProps> = ({
  id,
  value = null,
  variant = "Stars",
  disabled = false,
  invalid,
  events = EMPTY_ARRAY,
  nullable = false,
  allowHalf = false,
  max = 5,
  density = Densities.Medium,
  slots,
}) => {
  const eventHandler = useEventHandler();

  type FeedbackValue = number | boolean | null;

  const [localValue, setLocalValue] = useOptimisticValue<FeedbackValue>(value, false);

  const isBooleanType = useMemo(() => {
    // If variant is Thumbs and nullable is true, treat as bool?
    if (variant === "Thumbs" && nullable) return true;
    return typeof value === "boolean";
  }, [value, variant, nullable]);

  // Convert value to number for rating components
  const numericValue = useMemo(() => {
    if (localValue === null || localValue === undefined) return ThumbsEnum.None;
    if (isBooleanType) {
      if (variant === "Thumbs") {
        if (nullable) {
          // For nullable boolean types: null -> None(0), false -> Down(1), true -> Up(2)
          return localValue ? ThumbsEnum.Up : ThumbsEnum.Down;
        } else {
          // For non-nullable boolean types: false -> Down(1), true -> Up(2)
          return localValue ? ThumbsEnum.Up : ThumbsEnum.Down;
        }
      }
      return localValue ? 1 : 0;
    }
    return localValue as number;
  }, [localValue, variant, isBooleanType, nullable]);

  const handleChange = useCallback(
    (e: number) => {
      if (!events.includes("OnChange")) return;
      if (disabled) return;

      // Convert number back to original type
      let convertedValue: number | boolean | null;
      if (isBooleanType) {
        if (variant === "Thumbs") {
          if (nullable) {
            // For nullable boolean types
            if (e === ThumbsEnum.None) {
              convertedValue = null;
            } else if (e === ThumbsEnum.Down) {
              convertedValue = false;
            } else if (e === ThumbsEnum.Up) {
              convertedValue = true;
            } else {
              // Fallback - shouldn't happen
              convertedValue = e === ThumbsEnum.Up;
            }
          } else {
            // For non-nullable boolean types
            if (e === ThumbsEnum.None) {
              // For non-nullable types, toggle to the opposite value
              convertedValue = !localValue;
            } else if (e === numericValue) {
              // Clicking the same thumb - toggle to the opposite value
              convertedValue = !localValue;
            } else {
              // Clicking different thumb - set new value
              convertedValue = e === ThumbsEnum.Up;
            }
          }
        } else {
          convertedValue = e === 1;
        }
      } else {
        // Numeric type handling (including nullable numeric types)
        if (e === ThumbsEnum.None) {
          convertedValue = nullable ? null : ThumbsEnum.None;
        } else {
          convertedValue = e;
        }
      }
      setLocalValue(convertedValue);
      eventHandler("OnChange", id, [convertedValue]);
    },
    [
      id,
      disabled,
      localValue,
      variant,
      numericValue,
      events,
      eventHandler,
      nullable,
      isBooleanType,
      setLocalValue,
    ],
  );

  const handleBlur = useCallback(() => {
    if (disabled) return;
    if (events.includes("OnBlur")) eventHandler("OnBlur", id, []);
  }, [disabled, eventHandler, id, events]);

  const handleFocus = useCallback(() => {
    if (disabled) return;
    if (events.includes("OnFocus")) eventHandler("OnFocus", id, []);
  }, [disabled, eventHandler, id, events]);

  const ratingComponent = useMemo(() => {
    if (variant === "Thumbs") {
      return (
        <ThumbsRating
          disabled={disabled}
          value={numericValue}
          onRate={handleChange}
          invalid={invalid}
          density={density}
        />
      );
    }

    if (variant === "Emojis") {
      return (
        <EmojiRating
          disabled={disabled}
          value={numericValue}
          onRate={handleChange}
          invalid={invalid}
          allowHalf={allowHalf}
          totalEmojis={max}
          density={density}
        />
      );
    }

    if (variant === "Stars") {
      return (
        <StarRating
          disabled={disabled}
          value={numericValue}
          onRate={handleChange}
          invalid={invalid}
          allowHalf={allowHalf}
          totalStars={max}
          density={density}
        />
      );
    }
    return null;
  }, [variant, disabled, numericValue, handleChange, invalid, allowHalf, max, density]);

  const prefixContent = slots?.Prefix;
  const suffixContent = slots?.Suffix;
  const hasPrefix = (prefixContent?.length ?? 0) > 0;
  const hasSuffix = (suffixContent?.length ?? 0) > 0;
  const hasAffixes = hasPrefix || hasSuffix;

  const feedbackContent = (
    <div
      onBlur={(e) => {
        if (!e.currentTarget.contains(e.relatedTarget)) {
          handleBlur();
        }
      }}
      onFocus={(e) => {
        if (!e.currentTarget.contains(e.relatedTarget)) {
          handleFocus();
        }
      }}
      tabIndex={disabled ? -1 : 0}
      className={cn(
        "outline-none focus:outline-none focus:ring-1 focus:ring-ring rounded-md p-1",
        disabled && "opacity-50 cursor-not-allowed",
        invalid && inputStyles.invalidInput,
      )}
    >
      {ratingComponent}
    </div>
  );

  if (!hasAffixes) return feedbackContent;

  return (
    <div
      className={cn(
        "relative flex items-stretch rounded-field border border-input bg-transparent shadow-sm transition-colors dark:bg-white/5 dark:border-white/10",
        invalid && "border-destructive",
        disabled && "cursor-not-allowed opacity-50",
      )}
    >
      {hasPrefix && (
        <div className="flex items-center px-3 bg-muted text-muted-foreground border-r border-input rounded-tl-[var(--radius-fields)] rounded-bl-[var(--radius-fields)]">
          {prefixContent}
        </div>
      )}
      <div className="flex-1 px-3 py-2">{feedbackContent}</div>
      {hasSuffix && (
        <div className="flex items-center px-3 bg-muted text-muted-foreground border-l border-input rounded-tr-[var(--radius-fields)] rounded-br-[var(--radius-fields)]">
          {suffixContent}
        </div>
      )}
    </div>
  );
};
