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
  nullable = false
}) => {
  const eventHandler = useEventHandler();

  const isBooleanType = useMemo(() => {
    return typeof value === 'boolean';
  }, [value]);

  // Convert value to number for rating components
  const numericValue = useMemo(() => {
    if (value === null || value === undefined) return 0;
    if (isBooleanType) {
      if (variant === 'Thumbs') {
        return value ? ThumbsEnum.Up : ThumbsEnum.Down;
      }
      return value ? 1 : 0;
    }
    return value as number;
  }, [value, variant, isBooleanType]);

  const handleChange = useCallback((e: number) => {
    if(!events.includes("OnChange")) return;
    if(disabled) return;
    
    // Convert number back to original type
    let convertedValue: number | boolean | null;
    if (isBooleanType) {
      if (variant === 'Thumbs') {
        // For boolean types with ThumbsRating
        if (e === ThumbsEnum.None) {
          // Deselection - only allow null for nullable types
          convertedValue = nullable ? null : false;
        } else if (e === numericValue) {
          // Clicking the same thumb
          if (nullable) {
            // For nullable types, deselect (set to null)
            convertedValue = null;
          } else {
            // For non-nullable types, toggle to the opposite value
            convertedValue = !value;
          }
        } else {
          // Clicking different thumb - set new value
          convertedValue = e === ThumbsEnum.Up;
        }
      } else {
        convertedValue = e === 1;
      }
    } else {
      // Numeric type handling (including nullable numeric types)
      if (e === 0) {
        convertedValue = nullable ? null : 0;
      } else {
        convertedValue = e;
      }
    }
    
    eventHandler("OnChange", id, [convertedValue]);
  }, [id, disabled, value, variant, numericValue, events, eventHandler, nullable, isBooleanType]);

  if(variant === 'Thumbs') {
    return (
      <ThumbsRating disabled={disabled} value={numericValue} onRate={handleChange} invalid={invalid}/>
    );
  }

  if(variant === 'Emojis') {
    return (
      <EmojiRating disabled={disabled} value={numericValue} onRate={handleChange} invalid={invalid}/>
    );
  }

  if(variant === 'Stars') {
    return (
      <StarRating disabled={disabled} value={numericValue} onRate={handleChange} invalid={invalid}/>
    );
  }

};