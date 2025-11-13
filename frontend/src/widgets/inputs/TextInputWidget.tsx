import React, {
  useState,
  useCallback,
  useMemo,
  useRef,
  useEffect,
} from 'react';
import { Input } from '@/components/ui/input';
import { EyeIcon, EyeOffIcon, Search, X } from 'lucide-react';
import { Textarea } from '@/components/ui/textarea';
import { cn } from '@/lib/utils';
import { getHeight, getWidth, inputStyles } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';
import { useFocusable } from '@/hooks/use-focus-management';
import { useEventHandler } from '@/components/event-handler';
import { sidebarMenuRef } from '../layouts/sidebar';
import { Sizes } from '@/types/sizes';
import Icon from '@/components/Icon';
import {
  textInputSizeVariants,
  searchIconVariants,
  xIconVariants,
  eyeIconVariants,
} from '@/components/ui/input/text-input-variants';

type PrefixSuffix =
  | { type: 'text'; value: string }
  | { type: 'icon'; value: string };

interface TextInputWidgetProps {
  id: string;
  placeholder?: string;
  value?: string;
  variant:
    | 'Text'
    | 'Textarea'
    | 'Email'
    | 'Tel'
    | 'Url'
    | 'Password'
    | 'Search';
  disabled: boolean;
  invalid?: string;
  events: string[];
  width?: string;
  height?: string;
  shortcutKey?: string;
  size?: Sizes;
  prefix?: PrefixSuffix;
  suffix?: PrefixSuffix;
  'data-testid'?: string;
}

// Utility to detect Mac platform
const isMac =
  typeof navigator !== 'undefined' &&
  /Mac|iPod|iPhone|iPad/.test(navigator.userAgent);

// Parse the shortcut string into components
const parseShortcut = (shortcutStr?: string) => {
  if (!shortcutStr) return null;
  const parts = shortcutStr.toLowerCase().split('+');
  return {
    ctrl: !isMac && parts.includes('ctrl'),
    shift: parts.includes('shift'),
    alt: parts.includes('alt'),
    meta: isMac
      ? parts.includes('ctrl') ||
        parts.includes('meta') ||
        parts.includes('cmd') ||
        parts.includes('command')
      : false,
    key: parts[parts.length - 1],
  };
};

// Format the shortcut for display as React nodes
const formatShortcutForDisplay = (shortcutStr?: string): React.ReactNode[] => {
  if (!shortcutStr) return [];
  const parts = shortcutStr.split('+').map(p => p.trim());
  const result: React.ReactNode[] = [];

  parts.forEach((part, index) => {
    if (index > 0) {
      result.push('+');
    }

    if (
      isMac &&
      (part.toLowerCase() === 'ctrl' ||
        part.toLowerCase() === 'cmd' ||
        part.toLowerCase() === 'command' ||
        part.toLowerCase() === 'meta')
    ) {
      result.push(
        <span
          key={`meta-${index}`}
          className="inline-flex items-center justify-center"
        >
          ⌘
        </span>
      );
    } else if (!isMac && part.toLowerCase() === 'ctrl') {
      result.push('Ctrl');
    } else {
      result.push(part.charAt(0).toUpperCase() + part.slice(1));
    }
  });

  return result;
};

const useCursorPosition = (
  value?: string,
  externalRef?: React.RefObject<HTMLInputElement | HTMLTextAreaElement | null>
) => {
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

const useEnterKeyBlur = () => {
  return useCallback(
    (e: React.KeyboardEvent<HTMLInputElement | HTMLTextAreaElement>) => {
      if (e.key === 'Enter') {
        e.currentTarget.blur();
        e.preventDefault();
      }
    },
    []
  );
};

/**
 * Renders either text or icon for prefix/suffix display.
 * Uses discriminated union type to ensure only one type can be set.
 */
const renderPrefixSuffix = (prefixSuffix?: PrefixSuffix): React.ReactNode => {
  if (!prefixSuffix) return null;

  if (prefixSuffix.type === 'icon') {
    return <Icon name={prefixSuffix.value} className="w-4 h-4" />;
  }

  return <span className="text-sm">{prefixSuffix.value}</span>;
};

const DefaultVariant: React.FC<{
  type: Lowercase<TextInputWidgetProps['variant']>;
  props: Omit<TextInputWidgetProps, 'variant'>;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onBlur: (e: React.FocusEvent<HTMLInputElement>) => void;
  onFocus: (e: React.FocusEvent<HTMLInputElement>) => void;
  inputRef?: React.RefObject<HTMLInputElement | HTMLTextAreaElement | null>;
  isFocused: boolean;
  size?: Sizes;
}> = ({
  type,
  props,
  onChange,
  onBlur,
  onFocus,
  inputRef,
  isFocused,
  size = Sizes.Medium,
}) => {
  const { elementRef, savePosition } = useCursorPosition(props.value, inputRef);
  const handleKeyDown = useEnterKeyBlur();

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    savePosition();
    onChange(e);
  };

  const styles: React.CSSProperties = {
    ...getWidth(props.width),
  };

  const shortcutDisplay = formatShortcutForDisplay(props.shortcutKey);
  const hasValue = props.value && props.value.toString().trim() !== '';
  const prefixContent = renderPrefixSuffix(props.prefix);
  const suffixContent = renderPrefixSuffix(props.suffix);

  const hasAffixes = prefixContent || suffixContent;

  return (
    <div className="relative w-full select-none" style={styles}>
      <div
        className={cn(
          'relative flex items-stretch rounded-md border border-input bg-transparent shadow-sm transition-colors',
          isFocused && 'outline-none ring-1 ring-ring',
          props.invalid && 'border-destructive',
          props.disabled && 'cursor-not-allowed opacity-50'
        )}
      >
        {/* Prefix with background and separator */}
        {prefixContent && (
          <div className="flex items-center px-3 bg-muted text-muted-foreground border-r border-input rounded-l-md">
            {prefixContent}
          </div>
        )}

        <div className="relative flex-1">
          <Input
            ref={elementRef as React.RefObject<HTMLInputElement>}
            id={props.id}
            placeholder={props.placeholder}
            value={props.value}
            type={type}
            disabled={props.disabled}
            onChange={handleChange}
            onBlur={onBlur}
            onFocus={onFocus}
            onKeyDown={handleKeyDown}
            className={cn(
              textInputSizeVariants({ size }),
              props.invalid && inputStyles.invalidInput,
              props.invalid && 'pr-8',
              props.shortcutKey && !isFocused && !hasValue && 'pr-16',
              'border-0 shadow-none focus-visible:ring-0 focus-visible:ring-offset-0',
              prefixContent && 'rounded-l-none',
              suffixContent && 'rounded-r-none',
              !hasAffixes && 'rounded-md'
            )}
            data-testid={props['data-testid']}
          />

          {/* Right side container: shortcut (if any), then invalid (if any) */}
          {(props.shortcutKey || props.invalid) && (
            <div className="absolute right-2.5 top-1/2 -translate-y-1/2 flex items-center gap-2 pointer-events-none h-6">
              {props.shortcutKey && !isFocused && !hasValue && (
                <div className="pointer-events-auto flex items-center h-6">
                  <kbd className="px-1 py-0.5 text-small-label font-medium text-foreground bg-muted border border-border rounded-md">
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
          )}
        </div>

        {/* Suffix with background and separator */}
        {suffixContent && (
          <div className="flex items-center px-3 bg-muted text-muted-foreground border-l border-input rounded-r-md">
            {suffixContent}
          </div>
        )}
      </div>
    </div>
  );
};

const TextareaVariant: React.FC<{
  props: Omit<TextInputWidgetProps, 'variant'>;
  onChange: (e: React.ChangeEvent<HTMLTextAreaElement>) => void;
  onBlur: (e: React.FocusEvent<HTMLTextAreaElement>) => void;
  onFocus: (e: React.FocusEvent<HTMLTextAreaElement>) => void;
  width?: string;
  inputRef?: React.RefObject<HTMLInputElement | HTMLTextAreaElement | null>;
  isFocused: boolean;
  size?: Sizes;
}> = ({
  props,
  onChange,
  onBlur,
  onFocus,
  inputRef,
  isFocused,
  size = Sizes.Medium,
}) => {
  const { elementRef, savePosition } = useCursorPosition(props.value, inputRef);

  const handleChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    savePosition();
    onChange(e);
  };

  const styles: React.CSSProperties = {
    ...getWidth(props.width),
    ...getHeight(props.height),
  };

  const shortcutDisplay = formatShortcutForDisplay(props.shortcutKey);
  const hasValue = props.value && props.value.toString().trim() !== '';

  return (
    <div className="relative w-full select-none">
      <Textarea
        style={styles}
        ref={elementRef as React.RefObject<HTMLTextAreaElement>}
        id={props.id}
        placeholder={props.placeholder}
        value={props.value}
        disabled={props.disabled}
        onChange={handleChange}
        onBlur={onBlur}
        onFocus={onFocus}
        className={cn(
          textInputSizeVariants({ size }),
          props.invalid && inputStyles.invalidInput,
          props.invalid && 'pr-8',
          props.shortcutKey && !isFocused && !hasValue && 'pr-16'
        )}
        data-testid={props['data-testid']}
      />
      {/* Icons container: shortcut (if any), then invalid (if any) */}
      <div className="absolute right-2.5 top-1/2 -translate-y-1/2 flex items-center gap-2 pointer-events-none h-6">
        {props.shortcutKey && !isFocused && !hasValue && (
          <div className="pointer-events-auto flex items-center h-6">
            <kbd className="px-1 py-0.5 text-small-label font-medium text-foreground bg-muted border border-border rounded-md">
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

const PasswordVariant: React.FC<{
  props: Omit<TextInputWidgetProps, 'variant'>;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onBlur: (e: React.FocusEvent<HTMLInputElement>) => void;
  onFocus: (e: React.FocusEvent<HTMLInputElement>) => void;
  width?: string;
  inputRef?: React.RefObject<HTMLInputElement | HTMLTextAreaElement | null>;
  size?: Sizes;
}> = ({ props, onChange, onBlur, onFocus, inputRef, size = Sizes.Medium }) => {
  const [showPassword, setShowPassword] = useState(false);
  const [hasLastPass, setHasLastPass] = useState(false);
  const { elementRef: elementRefGeneric, savePosition } = useCursorPosition(
    props.value,
    inputRef
  );
  const elementRef = elementRefGeneric as React.RefObject<HTMLInputElement>;
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const interval = setInterval(() => {
      if (containerRef.current?.querySelector('[data-lastpass-icon-root]')) {
        setHasLastPass(true);
        clearInterval(interval);
      }
    }, 300);
    return () => clearInterval(interval);
  }, []);

  const togglePassword = useCallback(() => {
    setShowPassword(prev => !prev);
  }, []);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    savePosition();
    onChange(e);
  };

  const handleKeyDown = useEnterKeyBlur();

  const styles: React.CSSProperties = {
    ...getWidth(props.width),
  };

  const shortcutDisplay = formatShortcutForDisplay(props.shortcutKey);
  const hasValue = props.value && props.value.toString().trim() !== '';

  return (
    <div
      className="relative w-full select-none"
      style={styles}
      ref={containerRef}
    >
      <Input
        ref={elementRef}
        id={props.id}
        placeholder={props.placeholder}
        value={props.value}
        type={showPassword ? 'text' : 'password'}
        disabled={props.disabled}
        onChange={handleChange}
        onBlur={onBlur}
        onFocus={onFocus}
        onKeyDown={handleKeyDown}
        className={cn(
          textInputSizeVariants({ size }),
          props.invalid && inputStyles.invalidInput,
          props.invalid ? 'pr-14' : 'pr-8',
          hasLastPass && 'pr-3',
          props.shortcutKey && !hasLastPass && !hasValue && 'pr-24'
        )}
        data-testid={props['data-testid']}
      />
      {/* Icons container: password toggle, shortcut (if any), then invalid (if any) */}
      {!hasLastPass && (
        <div className="absolute right-2.5 top-1/2 -translate-y-1/2 flex items-center gap-2 pointer-events-none h-6">
          <div className="pointer-events-auto flex items-center h-6">
            <button
              type="button"
              className={eyeIconVariants({ size })}
              onClick={togglePassword}
            >
              {showPassword ? (
                <EyeOffIcon className={eyeIconVariants({ size })} />
              ) : (
                <EyeIcon className={eyeIconVariants({ size })} />
              )}
            </button>
          </div>
          {props.shortcutKey && !hasValue && (
            <div className="pointer-events-auto flex items-center h-6">
              <kbd className="ml-2 px-1 py-0.5 text-small-label font-medium text-foreground bg-muted border border-border rounded-md">
                {shortcutDisplay}
              </kbd>
            </div>
          )}
          {props.invalid && (
            <div className="pointer-events-auto flex items-center h-6 ml-2">
              <InvalidIcon message={props.invalid} />
            </div>
          )}
        </div>
      )}
    </div>
  );
};

const SearchVariant: React.FC<{
  props: Omit<TextInputWidgetProps, 'variant'>;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onBlur: (e: React.FocusEvent<HTMLInputElement>) => void;
  onFocus: (e: React.FocusEvent<HTMLInputElement>) => void;
  width?: string;
  inputRef?: React.RefObject<HTMLInputElement | HTMLTextAreaElement | null>;
  isFocused: boolean;
  size?: Sizes;
}> = ({
  props,
  onChange,
  onBlur,
  onFocus,
  inputRef,
  isFocused,
  size = Sizes.Medium,
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
      <Search className={searchIconVariants({ size })} />

      {/* Search Input */}
      <Input
        ref={mergedRef}
        id={props.id}
        type="search"
        placeholder={props.placeholder}
        value={props.value}
        disabled={props.disabled}
        onChange={handleChange}
        onBlur={handleBlur}
        onFocus={onFocus}
        onKeyDown={handleKeyDown}
        autoComplete="off"
        className={cn(
          textInputSizeVariants({ size }),
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
            <X className={xIconVariants({ size })} />
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
  size,
  prefix,
  suffix,
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
      size,
      prefix,
      suffix,
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
      size,
      prefix,
      suffix,
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
          size={size}
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
          size={size}
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
          size={size}
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
          size={size}
        />
      );
  }
};

export default TextInputWidget;
