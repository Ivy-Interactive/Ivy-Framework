import React, { useState, useCallback, useMemo, useRef, useEffect } from 'react';
import { Input } from "@/components/ui/input";
import { EyeIcon, EyeOffIcon, Search } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { cn, getTestId } from '@/lib/utils';
import { getHeight, getWidth, inputStyles } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';
import { useFocusable } from '@/hooks/use-focus-management';
import { useEventHandler } from '@/components/EventHandlerContext';
import { sidebarMenuRef } from '../layouts/SidebarLayoutWidget';

interface TextInputWidgetProps {
  id: string;
  placeholder?: string;
  value?: string;
  variant: "Text" | "Textarea" | "Email" | "Tel" | "Url" | "Password" | "Search";
  disabled: boolean;
  invalid?: string;
  events: string[];
  width?: string;
  height?: string;
  shortcutKey?: string;
}

// Utility to detect Mac platform
const isMac = typeof navigator !== 'undefined' && /Mac|iPod|iPhone|iPad/.test(navigator.userAgent);

// Parse the shortcut string into components
const parseShortcut = (shortcutStr?: string) => {
  if (!shortcutStr) return null;
  const parts = shortcutStr.toLowerCase().split('+');
  return {
    ctrl: !isMac && parts.includes('ctrl'),
    shift: parts.includes('shift'),
    alt: parts.includes('alt'),
    meta: isMac ? (parts.includes('ctrl') || parts.includes('meta') || parts.includes('cmd') || parts.includes('command')) : false,
    key: parts[parts.length - 1]
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
    
    if (isMac && (part.toLowerCase() === 'ctrl' || part.toLowerCase() === 'cmd' || part.toLowerCase() === 'command' || part.toLowerCase() === 'meta')) {
      result.push(
        <span key={`meta-${index}`} style={{ fontSize: '1.5em', lineHeight: 1, verticalAlign: 'middle' }}>
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

const useCursorPosition = (value?: string, externalRef?: React.RefObject<HTMLInputElement | HTMLTextAreaElement>) => {
  const internalRef = useRef<HTMLInputElement | HTMLTextAreaElement | null>(null);
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
  inputRef?: React.RefObject<HTMLInputElement | HTMLTextAreaElement>;
  isFocused: boolean;
}> = ({ type, props, onChange, onBlur, onFocus, inputRef, isFocused }) => {
  const { elementRef, savePosition } = useCursorPosition(props.value, inputRef);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    savePosition();
    onChange(e);
  };

  const styles:React.CSSProperties = {
    ...getWidth(props.width)
  };

  const shortcutDisplay = formatShortcutForDisplay(props.shortcutKey);

  return (
    <div className="relative" style={styles}>
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
          (props.invalid && inputStyles.invalid),   
          (props.invalid && "pr-8"),
          (props.shortcutKey && !isFocused && "pr-16"),
        )}
      />
      {props.invalid && (
        <div className="absolute right-2.5 top-2.5">
          <InvalidIcon message={props.invalid} />
        </div>
      )}
      {props.shortcutKey && !isFocused && (
        <div className="absolute right-2.5 top-1/2 -translate-y-1/2 flex items-center">
          <kbd className="px-1 py-0.5 text-xs font-medium text-gray-800 bg-gray-100 border border-gray-200 rounded-md">
            {shortcutDisplay}
          </kbd>
        </div>
      )}
    </div>
  );
};

const TextareaVariant: React.FC<{
  props: Omit<TextInputWidgetProps, 'variant'>;
  onChange: (e: React.ChangeEvent<HTMLTextAreaElement>) => void;
  onBlur: (e: React.FocusEvent<HTMLTextAreaElement>) => void;
  onFocus: (e: React.FocusEvent<HTMLTextAreaElement>) => void;
  width?: string;
  inputRef?: React.RefObject<HTMLInputElement | HTMLTextAreaElement>;
  isFocused: boolean;
}> = ({ props, onChange, onBlur, onFocus, inputRef, isFocused }) => {
  const { elementRef, savePosition } = useCursorPosition(props.value, inputRef);

  const handleChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    savePosition();
    onChange(e);
  };

  const styles:React.CSSProperties = {
    ...getWidth(props.width),
    ...getHeight(props.height)
  };

  const shortcutDisplay = formatShortcutForDisplay(props.shortcutKey);

  return (
    <div className="relative">
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
          (props.invalid && inputStyles.invalid),
          (props.invalid && "pr-8"),
          (props.shortcutKey && !isFocused && "pr-16"),
        )}
      />
      {props.invalid && (
        <div className="absolute right-2.5 top-2.5 h-4 w-4">
          <InvalidIcon message={props.invalid} />
        </div>
      )}
      {props.shortcutKey && !isFocused && (
        <div className="absolute right-2.5 top-2.5 flex items-center">
          <kbd className="px-1 py-0.5 text-xs font-medium text-gray-800 bg-gray-100 border border-gray-200 rounded-md">
            {shortcutDisplay}
          </kbd>
        </div>
      )}
    </div>
  );
};

const PasswordVariant: React.FC<{
  props: Omit<TextInputWidgetProps, 'variant'>;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onBlur: (e: React.FocusEvent<HTMLInputElement>) => void;
  onFocus: (e: React.FocusEvent<HTMLInputElement>) => void;
  width?: string;
  inputRef?: React.RefObject<HTMLInputElement | HTMLTextAreaElement>;
}> = ({ props, onChange, onBlur, onFocus, inputRef }) => {
  const [showPassword, setShowPassword] = useState(false);
  const [hasLastPass, setHasLastPass] = useState(false);
  const { elementRef: elementRefGeneric, savePosition } = useCursorPosition(props.value, inputRef);
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

  const styles:React.CSSProperties = {
    ...getWidth(props.width)
  };

  const shortcutDisplay = formatShortcutForDisplay(props.shortcutKey);

  return (
    <div className="relative" style={styles} ref={containerRef}>
      <Input 
        ref={elementRef}
        id={props.id}
        placeholder={props.placeholder}
        value={props.value}
        type={showPassword ? "text" : "password"}
        disabled={props.disabled}
        onChange={handleChange}
        onBlur={onBlur}
        onFocus={onFocus}
        className={cn(
          (props.invalid && inputStyles.invalid),   
          (props.invalid ? "pr-14" : "pr-8"),
          (hasLastPass && "pr-3"),
          (props.shortcutKey && !hasLastPass && "pr-24")
        )}
      />
    
      {!hasLastPass && <div className={cn(
        "absolute right-2.5 top-1/2 -translate-y-1/2 flex items-center"
        )}>
        <Button
          type="button"
          variant="ghost"
          size="sm"
          className="hover:bg-transparent p-0 m-0 h-4 w-4"
          onClick={togglePassword}>
          {showPassword ? (
            <EyeOffIcon className="h-4 w-4" />
          ) : (
            <EyeIcon className="h-4 w-4" />
          )}
        </Button>
        {props.invalid && (
            <InvalidIcon message={props.invalid} className="ml-2"/>
        )}
        {props.shortcutKey && (
          <kbd className="ml-2 px-1 py-0.5 text-xs font-medium text-gray-800 bg-gray-100 border border-gray-200 rounded-md">
            {shortcutDisplay}
          </kbd>
        )}
      </div>}
          
    </div>
  );
};

const SearchVariant: React.FC<{
  props: Omit<TextInputWidgetProps, 'variant'>;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onBlur: (e: React.FocusEvent<HTMLInputElement>) => void;
  onFocus: (e: React.FocusEvent<HTMLInputElement>) => void;
  width?: string;
  inputRef?: React.RefObject<HTMLInputElement | HTMLTextAreaElement>;
  isFocused: boolean;
}> = ({ props, onChange, onBlur, onFocus, inputRef, isFocused }) => {
  const { elementRef, savePosition } = useCursorPosition(props.value, inputRef) as { elementRef: React.RefObject<HTMLInputElement>, savePosition: () => void };
  const { ref: focusRef } = useFocusable('sidebar-navigation', 0);
  const shouldFocusMenuRef = useRef(false);

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

  const styles: React.CSSProperties = {
    ...getWidth(props.width)
  };

  const shortcutDisplay = formatShortcutForDisplay(props.shortcutKey);

  return (
    <div className="relative" style={styles}>
      {/* Search Icon */}
      <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-gray-500" />

      {/* Search Input */}
      <Input
        ref={(el) => {
          // Handle both refs
          if (el) {
            (elementRef as React.MutableRefObject<HTMLInputElement | null>).current = el;
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
        className={cn(
          "pl-8",
          props.invalid && inputStyles.invalid,
          props.invalid && "pr-8",
          props.shortcutKey && !isFocused && "pr-16"
        )}
        data-testid={getTestId("sidebar-search")}
      />

      {/* Error Icon */}
      {props.invalid && (
        <div className="absolute right-2.5 top-2.5 h-4 w-4">
          <InvalidIcon message={props.invalid} />
        </div>
      )}

      {/* Shortcut Display */}
      {props.shortcutKey && !isFocused && (
        <div className="absolute right-2.5 top-1/2 -translate-y-1/2 flex items-center">
          <kbd className="px-1 py-0.5 text-xs font-medium text-gray-800 bg-gray-100 border border-gray-200 rounded-md">
            {shortcutDisplay}
          </kbd>
        </div>
      )}
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
  shortcutKey
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
        (!shortcutObj.meta && !shortcutObj.ctrl && !event.metaKey && !event.ctrlKey);
      
      const isShortcutPressed =
        modifierMatch &&
        (event.shiftKey === shortcutObj.shift) &&
        (event.altKey === shortcutObj.alt) &&
        (event.key.toLowerCase() === shortcutObj.key.toLowerCase());
      if (isShortcutPressed) {
        event.preventDefault();
        if (inputRef.current) {
          inputRef.current.focus();
          setIsFocused(true);
          if(events.includes("OnFocus")) eventHandler("OnFocus", id, []);
        }
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => {
      window.removeEventListener('keydown', handleKeyDown);
    };
  }, [shortcutKey, id, events, eventHandler]);

  const handleChange = useCallback((e: React.ChangeEvent<HTMLTextAreaElement | HTMLInputElement>) => {
    setLocalValue(e.target.value);
    if(events.includes("OnChange")) eventHandler("OnChange", id, [e.target.value]);
  }, [eventHandler, id, events]);

  const handleBlur = useCallback(() => {
    setIsFocused(false);
    if(events.includes("OnBlur")) eventHandler("OnBlur", id, []);
  }, [eventHandler, id, events]);

  const handleFocus = useCallback(() => {
    setIsFocused(true);
    if(events.includes("OnFocus")) eventHandler("OnFocus", id, []);
  }, [eventHandler, id, events]);

  const commonProps = useMemo(() => ({
    id,
    placeholder,
    value: localValue,
    disabled,
    invalid,
    width,  
    height,
    events,
    shortcutKey
  }), [id, placeholder, localValue, disabled, invalid, events, width, height, shortcutKey]);

  switch (variant) {
    case "Password":
      return <PasswordVariant props={commonProps} onChange={handleChange} onBlur={handleBlur} onFocus={handleFocus} inputRef={inputRef}/>;
    case "Textarea":
      return <TextareaVariant props={commonProps} onChange={handleChange} onBlur={handleBlur} onFocus={handleFocus} inputRef={inputRef} isFocused={isFocused}/>;
    case "Search":
      return <SearchVariant props={commonProps} onChange={handleChange} onBlur={handleBlur} onFocus={handleFocus} inputRef={inputRef} isFocused={isFocused}/>;
    default:
      return <DefaultVariant 
        type={variant.toLowerCase() as Lowercase<TextInputWidgetProps['variant']>}
        props={commonProps}
        onChange={handleChange}
        onBlur={handleBlur}
        onFocus={handleFocus}
        inputRef={inputRef}
        isFocused={isFocused}
      />;
  }
};

export default TextInputWidget;


