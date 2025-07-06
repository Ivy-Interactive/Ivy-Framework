import { EmojiRating } from '@/components/EmojiRating';
import { useEventHandler } from '@/components/EventHandlerContext';
import { StarRating } from '@/components/StarRating';
import ThumbsRating, { ThumbsEnum } from '@/components/ThumbsRating';
import React, { useCallback, useMemo } from 'react';

interface FeedbackInputWidgetProps {
  id: string;
  value: number | boolean | null;
  variant: 'Thumbs' | 'Emojis' | 'Stars';
  disabled: boolean;
  invalid?: string;
  events: string[];
  nullable?: boolean;
}

export const FeedbackInputWidget: React.FC<FeedbackInputWidgetProps> = ({
  id,
  value,
  variant,
  disabled,
  invalid,
  events,
  nullable = false,
}) => {
  const eventHandler = useEventHandler();

  const isBooleanType = useMemo(() => {
    // If variant is Thumbs and nullable is true, treat as bool?
    if (variant === 'Thumbs' && nullable) return true;
    return typeof value === 'boolean';
  }, [value, variant, nullable]);

  // Convert value to number for rating components
  const numericValue = useMemo(() => {
    if (value === null || value === undefined) return ThumbsEnum.None;
    if (isBooleanType) {
      if (variant === 'Thumbs') {
        if (nullable) {
          // For nullable boolean types: null -> None(0), false -> Down(1), true -> Up(2)
          return value ? ThumbsEnum.Up : ThumbsEnum.Down;
        } else {
          // For non-nullable boolean types: false -> Down(1), true -> Up(2)
          return value ? ThumbsEnum.Up : ThumbsEnum.Down;
        }
      }
      return value ? 1 : 0;
    }
    return value as number;
  }, [value, variant, isBooleanType, nullable]);

  const handleChange = useCallback(
    (e: number) => {
      if (!events.includes('OnChange')) return;
      if (disabled) return;

      // Convert number back to original type
      let convertedValue: number | boolean | null;
      if (isBooleanType) {
        if (variant === 'Thumbs') {
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
              convertedValue = !value;
            } else if (e === numericValue) {
              // Clicking the same thumb - toggle to the opposite value
              convertedValue = !value;
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
      eventHandler('OnChange', id, [convertedValue]);
    },
    [
      id,
      disabled,
      value,
      variant,
      numericValue,
      events,
      eventHandler,
      nullable,
      isBooleanType,
    ]
  );

  if (variant === 'Thumbs') {
    return (
      <ThumbsRating
        disabled={disabled}
        value={numericValue}
        onRate={handleChange}
        invalid={invalid}
      />
    );
  }

  if (variant === 'Emojis') {
    return (
      <EmojiRating
        disabled={disabled}
        value={numericValue}
        onRate={handleChange}
        invalid={invalid}
      />
    );
  }

  if (variant === 'Stars') {
    return (
      <StarRating
        disabled={disabled}
        value={numericValue}
        onRate={handleChange}
        invalid={invalid}
      />
    );
  }
};
