import React, { useState, useCallback, useRef, useEffect } from 'react';
import { Input } from '@/components/ui/input';
import { EyeIcon, EyeOffIcon } from 'lucide-react';
import { cn } from '@/lib/utils';
import { getWidth, inputStyles } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';
import { Scales } from '@/types/scale';
import {
  textInputSizeVariants,
  eyeIconVariants,
} from '@/components/ui/input/text-input-variants';
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
  width?: string;
  inputRef?: React.RefObject<HTMLInputElement | HTMLTextAreaElement | null>;
  scale?: Scales;
}

export const PasswordVariant: React.FC<PasswordVariantProps> = ({
  props,
  onChange,
  onBlur,
  onFocus,
  inputRef,
  scale = Scales.Medium,
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
        maxLength={props.maxLength}
        onChange={handleChange}
        onBlur={onBlur}
        onFocus={onFocus}
        onKeyDown={handleKeyDown}
        onPaste={handlePaste}
        className={cn(
          textInputSizeVariants({ scale }),
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
              className={eyeIconVariants({ scale })}
              onClick={togglePassword}
            >
              {showPassword ? (
                <EyeOffIcon className={eyeIconVariants({ scale })} />
              ) : (
                <EyeIcon className={eyeIconVariants({ scale })} />
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
