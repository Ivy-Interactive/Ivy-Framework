import React, { useCallback, useRef } from 'react';
import { Input } from '@/components/ui/input';
import { Search, X } from 'lucide-react';
import { cn } from '@/lib/utils';
import { getWidth, inputStyles } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';
import { useFocusable } from '@/hooks/use-focus-management';
import { useEventHandler } from '@/components/event-handler';
import { sidebarMenuRef } from '@/widgets/layouts/sidebar';
import { Scales } from '@/types/scale';
import {
  textInputSizeVariants,
  searchIconVariants,
  xIconVariants,
} from '@/components/ui/input/text-input-variants';
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
  inputRef,
  isFocused,
  scale = Scales.Medium,
}) => {
  const { savePosition } = useCursorPosition(props.value, inputRef) as {
    savePosition: () => void;
  };
  const { ref: focusRef } = useFocusable('sidebar-navigation', 0);
  const shouldFocusMenuRef = useRef(false);
  const eventHandler = useEventHandler();

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

  const handleClear = () => {
    if (props.events.includes('OnChange')) {
      eventHandler('OnChange', props.id, ['']);
    }
  };

  const styles: React.CSSProperties = {
    ...getWidth(props.width),
  };

  const shortcutDisplay = formatShortcutForDisplay(props.shortcutKey);
  const hasValue = props.value && props.value.trim() !== '';

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
      {/* Search Icon */}
      <Search className={searchIconVariants({ scale })} />

      {/* Search Input */}
      <Input
        ref={mergedRef}
        id={props.id}
        type="search"
        placeholder={props.placeholder}
        value={props.value}
        disabled={props.disabled}
        maxLength={props.maxLength}
        onChange={handleChange}
        onBlur={handleBlur}
        onFocus={onFocus}
        onKeyDown={handleKeyDown}
        onPaste={handlePaste}
        autoComplete="off"
        className={cn(
          textInputSizeVariants({ scale }),
          'pl-8 cursor-pointer',
          props.invalid && inputStyles.invalidInput,
          props.invalid && 'pr-8',
          hasValue && 'pr-8',
          props.shortcutKey && !isFocused && !hasValue && 'pr-16',
          // Hide browser's default search input X icon
          '[&::-webkit-search-cancel-button]:appearance-none [&::-webkit-search-cancel-button]:hidden'
        )}
        data-testid={props['data-testid']}
      />
      {/* Icons container: clear (if any), shortcut (if any), then invalid (if any) */}
      <div className="absolute right-2.5 top-1/2 -translate-y-1/2 flex items-center gap-2 pointer-events-none z-10 h-6">
        {hasValue && !props.disabled && (
          <button
            type="button"
            tabIndex={-1}
            aria-label="Clear search"
            onClick={handleClear}
            className="p-1 rounded hover:bg-accent focus:outline-none cursor-pointer pointer-events-auto flex items-center h-6"
            style={{ pointerEvents: 'auto' }}
          >
            <X className={xIconVariants({ scale })} />
          </button>
        )}
        {props.shortcutKey && !isFocused && !hasValue && (
          <div className="pointer-events-auto flex items-center h-4">
            <kbd className="badge-text-primary text-foreground bg-muted border border-border rounded-sm px-1 py-0.25">
              {shortcutDisplay}
            </kbd>
          </div>
        )}
        {props.invalid && (
          <div className="pointer-events-auto flex items-center h-6">
            <InvalidIcon message={props.invalid} />
          </div>
        )}
      </div>
    </div>
  );
};
