import React, { useState, useCallback, useRef, useEffect } from 'react';
import { Input } from '@/components/ui/input';
import { EyeIcon, EyeOffIcon, X } from 'lucide-react';
import { cn } from '@/lib/utils';
import { getWidth, inputStyles } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';
import { Densities } from '@/types/density';
import {
  textInputSizeVariant,
  eyeIconVariant,
  xIconVariant,
} from '@/components/ui/input/text-input-variant';
import { TextInputWidgetProps } from '../types';
import {
  useCursorPosition,
  useEnterKeyBlur,
  usePasteHandler,
  formatShortcutForDisplay,
} from '../hooks';

interface PasswordVariantProps {
  props: Omit<TextInputWidgetProps, 'variant'>;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onBlur: (e: React.FocusEvent<HTMLInputElement>) => void;
  onFocus: (e: React.FocusEvent<HTMLInputElement>) => void;
  onClear: (e: React.MouseEvent) => void;
  onSubmit?: () => void;
  width?: string;
  inputRef?: React.RefObject<HTMLInputElement | HTMLTextAreaElement | null>;
  density?: Densities;
}

export const PasswordVariant: React.FC<PasswordVariantProps> = ({
  props,
  onChange,
  onBlur,
  onFocus,
  onClear,
  onSubmit,
  inputRef,
  density = Densities.Medium,
}) => {
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

  const handlePaste = usePasteHandler(props.maxLength, value => {
    const syntheticEvent = {
      target: { value },
      currentTarget: { value },
    } as React.ChangeEvent<HTMLInputElement>;
    onChange(syntheticEvent);
  });

  const handleKeyDown = useEnterKeyBlur(onSubmit);

  const styles: React.CSSProperties = {
    ...getWidth(props.width),
  };

  const shortcutDisplay = formatShortcutForDisplay(props.shortcutKey);
  const hasValue = props.value && props.value.toString().trim() !== '';
  const showClear = props.nullable && !props.disabled && hasValue;

  return (
    <div
      className="relative w-full select-none"
      style={styles}
      ref={containerRef}
    >
      <div className="rounded-field border border-input bg-transparent shadow-sm dark:bg-white/5 dark:border-white/10">
        <Input
          ref={elementRef}
          id={props.id}
          placeholder={props.placeholder}
          value={props.value}
          type={showPassword ? 'text' : 'password'}
          disabled={props.disabled}
          maxLength={props.maxLength}
          minLength={props.minLength}
          onChange={handleChange}
          onBlur={onBlur}
          onFocus={onFocus}
          onKeyDown={handleKeyDown}
          onPaste={handlePaste}
          className={cn(
            textInputSizeVariant({ density }),
            'border-0 shadow-none dark:bg-transparent',
            props.invalid && inputStyles.invalidInput,
            props.invalid || showClear ? 'pr-14' : 'pr-8',
            hasLastPass && 'pr-3',
            props.shortcutKey &&
              !hasLastPass &&
              !hasValue &&
              !showClear &&
              !props.invalid &&
              'pr-24',
            showClear && props.invalid && !hasLastPass && 'pr-20',
            !hasValue && props.nullable && 'placeholder:text-muted-foreground'
          )}
          data-testid={props['data-testid']}
        />
      </div>
      {!hasLastPass && (
        <div className="absolute right-2 top-1/2 -translate-y-1/2 flex flex-row items-center gap-1 pointer-events-none h-6">
          <div className="pointer-events-auto flex items-center h-6">
            <button
              type="button"
              className={eyeIconVariant({ density })}
              onClick={togglePassword}
            >
              {showPassword ? (
                <EyeOffIcon className={eyeIconVariant({ density })} />
              ) : (
                <EyeIcon className={eyeIconVariant({ density })} />
              )}
            </button>
          </div>
          {showClear && (
            <button
              type="button"
              tabIndex={-1}
              aria-label="Clear"
              onClick={onClear}
              className="pointer-events-auto p-1 rounded hover:bg-accent focus:outline-none cursor-pointer flex items-center h-6"
            >
              <X className={xIconVariant({ density })} />
            </button>
          )}
          {props.shortcutKey && !hasValue && !showClear && !props.invalid && (
            <div className="pointer-events-auto flex items-center h-6">
              <kbd className="ml-2 px-1 py-0.5 text-xs font-medium text-foreground bg-muted border border-border rounded-field">
                {shortcutDisplay}
              </kbd>
            </div>
          )}
          {/* Invalid icon - rightmost */}
          {props.invalid && (
            <div className="flex items-center h-6 ml-2">
              <InvalidIcon message={props.invalid} />
            </div>
          )}
        </div>
      )}
    </div>
  );
};
