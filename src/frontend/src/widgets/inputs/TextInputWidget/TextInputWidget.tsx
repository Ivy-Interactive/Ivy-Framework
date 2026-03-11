import React, { useState, useCallback, useMemo, useRef } from 'react';
import { useEventHandler } from '@/components/event-handler';
import { Densities } from '@/types/density';
import { TextInputWidgetProps, TextInputVariant } from './types';
import { useSyncServerValue, useShortcutKey } from './hooks';
import {
  DefaultVariant,
  TextareaVariant,
  PasswordVariant,
  SearchVariant,
} from './variants';

const EMPTY_ARRAY: never[] = [];

export const TextInputWidget: React.FC<TextInputWidgetProps> = ({
  id,
  placeholder,
  value,
  variant = 'Text',
  disabled = false,
  invalid,
  nullable = false,
  width,
  height,
  events = EMPTY_ARRAY,
  shortcutKey,
  density = Densities.Medium,
  prefix,
  suffix,
  maxLength,
  minLength,
  rows,
  'data-testid': dataTestId,
}) => {
  const eventHandler = useEventHandler();
  // Normalize null/undefined to empty string for display (HTML inputs can't have null values)
  const [localValue, setLocalValue] = useState(value ?? '');
  const [isFocused, setIsFocused] = useState(false);
  const [minLengthError, setMinLengthError] = useState<string | undefined>(
    undefined
  );
  const inputRef = useRef<HTMLInputElement | HTMLTextAreaElement | null>(null);

  // Wrapper to normalize null/undefined to empty string for useSyncServerValue
  const setLocalValueNormalized = useCallback(
    (val: string | undefined) => setLocalValue(val ?? ''),
    []
  );

  useSyncServerValue(value, localValue, isFocused, setLocalValueNormalized);

  useShortcutKey({
    shortcutKey,
    inputRef,
    setIsFocused,
    id,
    events,
    eventHandler,
  });

  const handleChange = useCallback(
    (e: React.ChangeEvent<HTMLTextAreaElement | HTMLInputElement>) => {
      const newValue = e.target.value;
      setLocalValue(newValue);
      // Clear the minLength error as soon as the value satisfies the constraint
      if (minLength !== undefined && newValue.length >= minLength) {
        setMinLengthError(undefined);
      }
      if (events.includes('OnChange')) eventHandler('OnChange', id, [newValue]);
    },
    [eventHandler, id, events, minLength]
  );

  const handleBlur = useCallback(() => {
    setIsFocused(false);
    // Show validation error if value is non-empty but below the minimum length
    if (
      minLength !== undefined &&
      localValue.length > 0 &&
      localValue.length < minLength
    ) {
      setMinLengthError(`Minimum ${minLength} characters required`);
    }
    if (events.includes('OnBlur')) eventHandler('OnBlur', id, []);
  }, [eventHandler, id, events, minLength, localValue]);

  const handleSubmit = useCallback(() => {
    if (events.includes('OnSubmit')) eventHandler('OnSubmit', id, []);
  }, [eventHandler, id, events]);

  const handleFocus = useCallback(() => {
    setIsFocused(true);
    if (events.includes('OnFocus')) eventHandler('OnFocus', id, []);
  }, [eventHandler, id, events]);

  const handleClear = useCallback(
    (e: React.MouseEvent) => {
      e.preventDefault();
      e.stopPropagation();
      if (!events.includes('OnChange')) return;
      if (disabled) return;
      // For nullable inputs, set to null; otherwise set to empty string
      const clearedValue = nullable ? null : '';
      setLocalValue(clearedValue ?? '');
      eventHandler('OnChange', id, [clearedValue]);
    },
    [eventHandler, id, events, disabled, nullable]
  );

  // Server-provided `invalid` takes precedence; fall back to the local minLength error
  const effectiveInvalid = invalid ?? minLengthError;

  const commonProps = useMemo(
    () => ({
      id,
      placeholder,
      value: localValue,
      disabled,
      invalid: effectiveInvalid,
      nullable,
      width,
      height,
      events,
      shortcutKey,
      density,
      prefix,
      suffix,
      maxLength,
      minLength,
      rows,
      'data-testid': dataTestId,
    }),
    [
      id,
      placeholder,
      localValue,
      disabled,
      effectiveInvalid,
      nullable,
      events,
      width,
      height,
      shortcutKey,
      density,
      prefix,
      suffix,
      maxLength,
      minLength,
      rows,
      dataTestId,
    ]
  );

  switch (variant) {
    case TextInputVariant.Password:
      return (
        <PasswordVariant
          props={commonProps}
          onChange={handleChange}
          onBlur={handleBlur}
          onFocus={handleFocus}
          onClear={handleClear}
          onSubmit={handleSubmit}
          inputRef={inputRef}
          density={density}
        />
      );
    case TextInputVariant.Textarea:
      return (
        <TextareaVariant
          props={commonProps}
          onChange={handleChange}
          onBlur={handleBlur}
          onFocus={handleFocus}
          onClear={handleClear}
          inputRef={inputRef}
          isFocused={isFocused}
          density={density}
        />
      );
    case TextInputVariant.Search:
      return (
        <SearchVariant
          props={commonProps}
          onChange={handleChange}
          onBlur={handleBlur}
          onFocus={handleFocus}
          onClear={handleClear}
          onSubmit={handleSubmit}
          inputRef={inputRef}
          isFocused={isFocused}
          density={density}
        />
      );
    default:
      return (
        <DefaultVariant
          type={variant.toLowerCase() as Lowercase<TextInputVariant>}
          props={commonProps}
          onChange={handleChange}
          onBlur={handleBlur}
          onFocus={handleFocus}
          onClear={handleClear}
          onSubmit={handleSubmit}
          inputRef={inputRef}
          isFocused={isFocused}
          density={density}
        />
      );
  }
};
