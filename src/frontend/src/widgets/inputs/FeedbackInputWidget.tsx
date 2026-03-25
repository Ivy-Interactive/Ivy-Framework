import { EmojiRating } from "@/components/EmojiRating";
import { useEventHandler } from "@/components/event-handler";
import { StarRating } from "@/components/StarRating";
import { ThumbsEnum, ThumbsRating } from "@/components/ui/thumbs-rating";
import React, { useCallback, useMemo } from "react";
import { useOptimisticValue } from "./shared/useOptimisticValue";
import { Densities } from "@/types/density";

const EMPTY_ARRAY: never[] = [];

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
};
