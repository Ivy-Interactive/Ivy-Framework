import React, {
  useState,
  useCallback,
  useMemo,
  useRef,
  useEffect,
} from 'react';
import { Input } from '@/components/ui/input';
import { EyeIcon, EyeOffIcon, Search, X } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { cn } from '@/lib/utils';
import { getHeight, getWidth, inputStyles } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';
import { useFocusable } from '@/hooks/use-focus-management';
import { useEventHandler } from '@/components/event-handler';
import { sidebarMenuRef } from '../layouts/sidebar';

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
  const elementRef = externalRef || internalRef;
  const cursorPositionRef = useRef<number | null>(null);

  const savePosition = () => {
    if (elementRef.current) {
      cursorPositionRef.current = elementRef.current.selectionStart;
    }
  };

  useEffect(() => {
    if (elementRef.current && cursorPositionRef.current !== null) {
      elementRef.current.setSelectionRange(
        cursorPositionRef.current,
        cursorPositionRef.current
      );
    }
  }, [value, elementRef]);

  return { elementRef, savePosition };
};

const DefaultVariant: React.FC<{
  type: Lowercase<TextInputWidgetProps['variant']>;
  props: Omit<TextInputWidgetProps, 'variant'>;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onBlur: (e: React.FocusEvent<HTMLInputElement>) => void;
  onFocus: (e: React.FocusEvent<HTMLInputElement>) => void;
  inputRef?: React.RefObject<HTMLInputElement | HTMLTextAreaElement | null>;
  isFocused: boolean;
}> = ({ type, props, onChange, onBlur, onFocus, inputRef, isFocused }) => {
  const { elementRef, savePosition } = useCursorPosition(props.value, inputRef);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    savePosition();
    onChange(e);
  };

  const styles: React.CSSProperties = {
    ...getWidth(props.width),
  };

  const shortcutDisplay = formatShortcutForDisplay(props.shortcutKey);

  return (
    <div className="relative w-full select-none" style={styles}>
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
        className={cn(
          'w-full',
          props.invalid && inputStyles.invalidInput,
          props.invalid && 'pr-8',
          props.shortcutKey && !isFocused && 'pr-16'
        )}
        data-testid={props['data-testid']}
      />
      {/* Icons container: shortcut (if any), then invalid (if any) */}
      <div className="absolute right-2.5 top-1/2 -translate-y-1/2 flex items-center gap-2 pointer-events-none h-6">
        {props.shortcutKey && !isFocused && (
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

const TextareaVariant: React.FC<{
  props: Omit<TextInputWidgetProps, 'variant'>;
  onChange: (e: React.ChangeEvent<HTMLTextAreaElement>) => void;
  onBlur: (e: React.FocusEvent<HTMLTextAreaElement>) => void;
  onFocus: (e: React.FocusEvent<HTMLTextAreaElement>) => void;
  width?: string;
  inputRef?: React.RefObject<HTMLInputElement | HTMLTextAreaElement | null>;
  isFocused: boolean;
}> = ({ props, onChange, onBlur, onFocus, inputRef, isFocused }) => {
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
          'w-full',
          props.invalid && inputStyles.invalidInput,
          props.invalid && 'pr-8',
          props.shortcutKey && !isFocused && 'pr-16'
        )}
        data-testid={props['data-testid']}
      />
      {/* Icons container: shortcut (if any), then invalid (if any) */}
      <div className="absolute right-2.5 top-1/2 -translate-y-1/2 flex items-center gap-2 pointer-events-none h-6">
        {props.shortcutKey && !isFocused && (
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
}> = ({ props, onChange, onBlur, onFocus, inputRef }) => {
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

  const styles: React.CSSProperties = {
    ...getWidth(props.width),
  };

  const shortcutDisplay = formatShortcutForDisplay(props.shortcutKey);

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
        className={cn(
          'w-full',
          props.invalid && inputStyles.invalidInput,
          props.invalid ? 'pr-14' : 'pr-8',
          hasLastPass && 'pr-3',
          props.shortcutKey && !hasLastPass && 'pr-24'
        )}
        data-testid={props['data-testid']}
      />
      {/* Icons container: password toggle, shortcut (if any), then invalid (if any) */}
      {!hasLastPass && (
        <div className="absolute right-2.5 top-1/2 -translate-y-1/2 flex items-center gap-2 pointer-events-none h-6">
          <div className="pointer-events-auto flex items-center h-6">
            <Button
              type="button"
              variant="ghost"
              size="sm"
              className="hover:bg-transparent p-0 m-0 h-4 w-4 cursor-pointer"
              onClick={togglePassword}
            >
              {showPassword ? (
                <EyeOffIcon className="h-4 w-4" />
              ) : (
                <EyeIcon className="h-4 w-4" />
              )}
            </Button>
          </div>
          {props.shortcutKey && (
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
}> = ({ props, onChange, onBlur, onFocus, inputRef, isFocused }) => {
  const { elementRef, savePosition } = useCursorPosition(
    props.value,
    inputRef
  ) as {
    elementRef: React.RefObject<HTMLInputElement>;
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

  return (
    <div className="relative w-full select-none" style={styles}>
      {/* Search Icon */}
      <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />

      {/* Search Input */}
      <Input
        ref={el => {
          // Handle both refs
          if (el) {
            (
              elementRef as React.MutableRefObject<HTMLInputElement | null>
            ).current = el;
            focusRef(el);
          }
        }}
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
          'w-full pl-8 cursor-pointer',
          props.invalid && inputStyles.invalidInput,
          props.invalid && 'pr-8',
          hasValue && 'pr-8',
          props.shortcutKey && !isFocused && 'pr-16',
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
            <X className="h-4 w-4 text-muted-foreground hover:text-foreground" />
          </button>
        )}
        {props.shortcutKey && !isFocused && (
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
  'data-testid': dataTestId,
}) => {
  const eventHandler = useEventHandler();
  const [localValue, setLocalValue] = useState(value);
  const [isFocused, setIsFocused] = useState(false);
  const inputRef = useRef<HTMLInputElement | HTMLTextAreaElement | null>(null);

  // Update local value when server value changes and control is not focused
  useEffect(() => {
    if (!isFocused && value !== localValue) {
      setLocalValue(value);
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
        />
      );
  }
};

export default TextInputWidget;
