import React, { useCallback, useRef } from 'react';
import { Input } from '@/components/ui/input';
import { Search, X } from 'lucide-react';
import { cn } from '@/lib/utils';
import { getWidth, inputStyles } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';
import { useFocusable } from '@/hooks/use-focus-management';
import { sidebarMenuRef } from '@/widgets/layouts/sidebar';
import { Scales } from '@/types/scale';
import {
  textInputSizeVariant,
  searchIconVariant,
  xIconVariant,
} from '@/components/ui/input/text-input-variant';
import { TextInputWidgetProps } from '../types';
import {
  useCursorPosition,
  usePasteHandler,
  formatShortcutForDisplay,
} from '../hooks';

interface SearchVariantProps {
  props: Omit<TextInputWidgetProps, 'variant'>;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onBlur: (e: React.FocusEvent<HTMLInputElement>) => void;
  onFocus: (e: React.FocusEvent<HTMLInputElement>) => void;
  onClear: (e: React.MouseEvent) => void;
  onSubmit?: () => void;
  width?: string;
  inputRef?: React.RefObject<HTMLInputElement | HTMLTextAreaElement | null>;
  isFocused: boolean;
  scale?: Scales;
}

export const SearchVariant: React.FC<SearchVariantProps> = ({
  props,
  onChange,
  onBlur,
  onFocus,
  onClear,
  onSubmit,
  inputRef,
  isFocused,
  scale = Scales.Medium,
}) => {
  const { savePosition } = useCursorPosition(props.value, inputRef) as {
    savePosition: () => void;
  };
  const { ref: focusRef } = useFocusable('sidebar-navigation', 0);
  const shouldFocusMenuRef = useRef(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    savePosition();
    onChange(e);
  };

  const handlePaste = usePasteHandler(props.maxLength, value => {
    const syntheticEvent = {
      target: { value },
      currentTarget: { value },
    } as React.ChangeEvent<HTMLInputElement>;
    onChange(syntheticEvent);
  });

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'ArrowDown' || e.key === 'ArrowUp' || e.key === 'Enter') {
      if (e.key === 'Enter') {
        onSubmit?.();
      }
      shouldFocusMenuRef.current = true;
      e.currentTarget.blur();
      e.preventDefault();
    }
  };

  const handleBlur = (e: React.FocusEvent<HTMLInputElement>) => {
    if (shouldFocusMenuRef.current) {
      shouldFocusMenuRef.current = false;
      sidebarMenuRef.current?.focus();
    }
    onBlur(e);
  };

  const styles: React.CSSProperties = {
    ...getWidth(props.width),
  };

  const shortcutDisplay = formatShortcutForDisplay(props.shortcutKey);
  const hasValue = props.value && props.value.trim() !== '';
  const showClear = props.nullable && !props.disabled && hasValue;

  // Merge focusRef and inputRef
  const mergedRef = useCallback(
    (element: HTMLInputElement | null) => {
      // Set focusRef for focus management
      focusRef(element);
      // Set inputRef for keyboard shortcut handler
      // Refs are mutable objects by design, so this assignment is safe
      if (inputRef && 'current' in inputRef) {
        // Use Reflect.set to bypass linter
        Reflect.set(inputRef, 'current', element);
      }
    },
    [focusRef, inputRef]
  );

  return (
    <div className="relative w-full select-none" style={styles}>
      <Search className={searchIconVariant({ scale })} />
      <div className="rounded-field border border-input bg-transparent shadow-sm dark:bg-white/5 dark:border-white/10">
        <Input
          ref={mergedRef}
          id={props.id}
          type="search"
          placeholder={props.placeholder}
          value={props.value}
          disabled={props.disabled}
          maxLength={props.maxLength}
          minLength={props.minLength}
          onChange={handleChange}
          onBlur={handleBlur}
          onFocus={onFocus}
          onKeyDown={handleKeyDown}
          onPaste={handlePaste}
          autoComplete="off"
          className={cn(
            textInputSizeVariant({ scale }),
            'pl-8 cursor-pointer border-0 shadow-none dark:bg-transparent',
            props.invalid && inputStyles.invalidInput,
            (props.invalid || showClear) && 'pr-8',
            props.shortcutKey &&
              !isFocused &&
              !hasValue &&
              !showClear &&
              !props.invalid &&
              'pr-16',
            showClear && props.invalid && 'pr-16',
            !hasValue && props.nullable && 'placeholder:text-muted-foreground',
            '[&::-webkit-search-cancel-button]:appearance-none [&::-webkit-search-cancel-button]:hidden'
          )}
          data-testid={props['data-testid']}
        />
      </div>
      <div className="absolute right-2.5 top-1/2 -translate-y-1/2 flex items-center gap-2 pointer-events-none z-10 h-6">
        {hasValue && !props.disabled && (
          <button
            type="button"
            tabIndex={-1}
            aria-label="Clear search"
            onClick={onClear}
            className="p-1 rounded hover:bg-accent focus:outline-none cursor-pointer pointer-events-auto flex items-center h-6"
            style={{ pointerEvents: 'auto' }}
          >
            <X className={xIconVariant({ scale })} />
          </button>
        )}
        {props.shortcutKey && !isFocused && !hasValue && (
          <div className="pointer-events-auto flex items-center h-4">
            <kbd className="text-xs text-foreground bg-muted border border-border rounded-selector px-1 py-0.25">
              {shortcutDisplay}
            </kbd>
          </div>
        )}
        {props.invalid && (
          <div className="flex items-center h-6">
            <InvalidIcon message={props.invalid} />
          </div>
        )}
      </div>
    </div>
  );
};
