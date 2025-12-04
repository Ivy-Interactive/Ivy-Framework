import React, {
  useState,
  useCallback,
  useMemo,
  useRef,
  useEffect,
} from 'react';
import { useEventHandler } from '@/components/event-handler';
import { Scales } from '@/types/scale';
import { TextInputWidgetProps } from './types';
import { parseShortcut } from './hooks';
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
  variant,
  disabled,
  invalid,
  width,
  height,
  events,
  shortcutKey,
  scale = Scales.Medium,
  prefix,
  suffix,
  maxLength,
  'data-testid': dataTestId,
}) => {
  const eventHandler = useEventHandler();
  const [localValue, setLocalValue] = useState(value);
  const [isFocused, setIsFocused] = useState(false);
  const inputRef = useRef<HTMLInputElement | HTMLTextAreaElement | null>(null);

  // Update local value when server value changes and control is not focused
  useEffect(() => {
    if (!isFocused && value !== localValue) {
      queueMicrotask(() => setLocalValue(value));
    }
  }, [value, isFocused, localValue]);

  // Handle keyboard shortcut
  useEffect(() => {
    if (!shortcutKey) return;

    const shortcutObj = parseShortcut(shortcutKey);
    if (!shortcutObj) return;

    const handleKeyDown = (event: KeyboardEvent) => {
      // Check if the required modifier keys match exactly what was defined in the shortcut
      const modifierMatch =
        (shortcutObj.meta && event.metaKey) ||
        (shortcutObj.ctrl && event.ctrlKey) ||
        (!shortcutObj.meta &&
          !shortcutObj.ctrl &&
          !event.metaKey &&
          !event.ctrlKey);

      const isShortcutPressed =
        modifierMatch &&
        event.shiftKey === shortcutObj.shift &&
        event.altKey === shortcutObj.alt &&
        event.key.toLowerCase() === shortcutObj.key.toLowerCase();
      if (isShortcutPressed) {
        event.preventDefault();
        if (inputRef.current) {
          inputRef.current.focus();
          setIsFocused(true);
          if (events.includes('OnFocus')) eventHandler('OnFocus', id, []);
        }
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => {
      window.removeEventListener('keydown', handleKeyDown);
    };
  }, [shortcutKey, id, events, eventHandler]);

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

  const commonProps = useMemo(
    () => ({
      id,
      placeholder,
      value: localValue,
      disabled,
      invalid,
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
    case 'Password':
      return (
        <PasswordVariant
          props={commonProps}
          onChange={handleChange}
          onBlur={handleBlur}
          onFocus={handleFocus}
          inputRef={inputRef}
          scale={scale}
        />
      );
    case 'Textarea':
      return (
        <TextareaVariant
          props={commonProps}
          onChange={handleChange}
          onBlur={handleBlur}
          onFocus={handleFocus}
          inputRef={inputRef}
          isFocused={isFocused}
          scale={scale}
        />
      );
    case 'Search':
      return (
        <SearchVariant
          props={commonProps}
          onChange={handleChange}
          onBlur={handleBlur}
          onFocus={handleFocus}
          inputRef={inputRef}
          isFocused={isFocused}
          scale={scale}
        />
      );
    default:
      return (
        <DefaultVariant
          type={
            variant.toLowerCase() as Lowercase<TextInputWidgetProps['variant']>
          }
          props={commonProps}
          onChange={handleChange}
          onBlur={handleBlur}
          onFocus={handleFocus}
          inputRef={inputRef}
          isFocused={isFocused}
          scale={scale}
        />
      );
  }
};
