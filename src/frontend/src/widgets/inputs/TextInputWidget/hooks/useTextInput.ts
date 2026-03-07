import React, { useRef, useEffect, useCallback } from 'react';

export { parseShortcut, formatShortcutForDisplay } from '../utils/shortcut';

export const useCursorPosition = (
  value?: string,
  externalRef?: React.RefObject<HTMLInputElement | HTMLTextAreaElement | null>
): {
  elementRef: React.RefObject<HTMLInputElement | HTMLTextAreaElement | null>;
  savePosition: () => void;
} => {
  const internalRef = useRef<HTMLInputElement | HTMLTextAreaElement | null>(
    null
  );
  const elementRefRef = externalRef || internalRef;
  const cursorPositionRef = useRef<number | null>(null);

  const savePosition = () => {
    if (elementRefRef.current) {
      cursorPositionRef.current = elementRefRef.current.selectionStart;
    }
  };

  useEffect(() => {
    if (elementRefRef.current && cursorPositionRef.current !== null) {
      elementRefRef.current.setSelectionRange(
        cursorPositionRef.current,
        cursorPositionRef.current
      );
    }
  }, [value, elementRefRef]);

  return { elementRef: elementRefRef, savePosition };
};

export const useEnterKeyBlur = (
  onSubmit?: () => void
): ((
  e: React.KeyboardEvent<HTMLInputElement | HTMLTextAreaElement>
) => void) => {
  return useCallback(
    (e: React.KeyboardEvent<HTMLInputElement | HTMLTextAreaElement>) => {
      if (e.key === 'Enter') {
        onSubmit?.();
        e.currentTarget.blur();
        e.preventDefault();
      }
    },
    [onSubmit]
  );
};

/**
 * Hook to handle paste events with maxLength enforcement.
 * Prevents default paste and manually inserts truncated text if needed.
 */
export const usePasteHandler = (
  maxLength?: number,
  onChange?: (value: string) => void
): ((
  e: React.ClipboardEvent<HTMLInputElement | HTMLTextAreaElement>
) => void) => {
  return useCallback(
    (e: React.ClipboardEvent<HTMLInputElement | HTMLTextAreaElement>) => {
      if (!maxLength) return;

      const target = e.currentTarget;
      const pastedText = e.clipboardData.getData('text');

      // Get current value, selection start and end
      const currentValue = target.value;
      const selectionStart = target.selectionStart ?? 0;
      const selectionEnd = target.selectionEnd ?? 0;

      // Calculate the new value after paste
      const beforeSelection = currentValue.slice(0, selectionStart);
      const afterSelection = currentValue.slice(selectionEnd);
      const newValue = beforeSelection + pastedText + afterSelection;

      // If the new value exceeds maxLength, prevent default and handle manually
      if (newValue.length > maxLength) {
        e.preventDefault();

        // Truncate the pasted text to fit within maxLength
        const availableSpace =
          maxLength - beforeSelection.length - afterSelection.length;
        const truncatedPaste = pastedText.slice(0, Math.max(0, availableSpace));
        const finalValue = beforeSelection + truncatedPaste + afterSelection;

        // Update the value
        target.value = finalValue;

        // Set cursor position after the pasted content
        const newCursorPos = beforeSelection.length + truncatedPaste.length;
        target.setSelectionRange(newCursorPos, newCursorPos);

        // Trigger onChange
        if (onChange) {
          onChange(finalValue);
        }
      }
    },
    [maxLength, onChange]
  );
};
