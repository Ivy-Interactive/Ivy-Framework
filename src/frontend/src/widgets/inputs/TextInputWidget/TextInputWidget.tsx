import React, { useState, useCallback, useMemo, useRef } from 'react';
import { useEventHandler } from '@/components/event-handler';
import { Scales } from '@/types/scale';
import { TextInputWidgetProps, TextInputVariant } from './types';
import { useSyncServerValue, useShortcutKey } from './hooks';
import {
  DefaultVariant,
  TextareaVariant,
  PasswordVariant,
  SearchVariant,
} from './variants';

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
  events = [],
  shortcutKey,
  scale = Scales.Medium,
  prefix,
  suffix,
  maxLength,
  'data-testid': dataTestId,
}) => {
  const eventHandler = useEventHandler();
  // Normalize null/undefined to empty string for display (HTML inputs can't have null values)
  const [localValue, setLocalValue] = useState(value ?? '');
  const [isFocused, setIsFocused] = useState(false);
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
      setLocalValue(e.target.value);
      if (events.includes('OnChange'))
        eventHandler('OnChange', id, [e.target.value]);
    },
    [eventHandler, id, events]
  );

  const handleBlur = useCallback(() => {
    setIsFocused(false);
    if (events.includes('OnBlur')) eventHandler('OnBlur', id, []);
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

  const commonProps = useMemo(
    () => ({
      id,
      placeholder,
      value: localValue,
      disabled,
      invalid,
      nullable,
      width,
      height,
      events,
      shortcutKey,
      scale,
      prefix,
      suffix,
      maxLength,
      'data-testid': dataTestId,
    }),
    [
      id,
      placeholder,
      localValue,
      disabled,
      invalid,
      nullable,
      events,
      width,
      height,
      shortcutKey,
      scale,
      prefix,
      suffix,
      maxLength,
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
          inputRef={inputRef}
          scale={scale}
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
          scale={scale}
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
          inputRef={inputRef}
          isFocused={isFocused}
          scale={scale}
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
          inputRef={inputRef}
          isFocused={isFocused}
          scale={scale}
        />
      );
  }
};
