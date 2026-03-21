import React from 'react';
import { Input } from '@/components/ui/input';
import { cn } from '@/lib/utils';
import { getWidth, inputStyles } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';
import { Densities } from '@/types/density';
import {
  textInputSizeVariant,
  xIconVariant,
} from '@/components/ui/input/text-input-variant';
import { TextInputWidgetProps } from '../types';
import { renderAffix } from '../utils/renderAffix';
import {
  useCursorPosition,
  useEnterKeyBlur,
  usePasteHandler,
  formatShortcutForDisplay,
} from '../hooks';
import { X } from 'lucide-react';

interface DefaultVariantProps {
  type: Lowercase<TextInputWidgetProps['variant']>;
  props: Omit<TextInputWidgetProps, 'variant'>;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onBlur: (e: React.FocusEvent<HTMLInputElement>) => void;
  onFocus: (e: React.FocusEvent<HTMLInputElement>) => void;
  onClear: (e: React.MouseEvent) => void;
  onSubmit?: () => void;
  inputRef?: React.RefObject<HTMLInputElement | HTMLTextAreaElement | null>;
  isFocused: boolean;
  density?: Densities;
}

export const DefaultVariant: React.FC<DefaultVariantProps> = ({
  type,
  props,
  onChange,
  onBlur,
  onFocus,
  onClear,
  onSubmit,
  inputRef,
  isFocused,
  density = Densities.Medium,
}) => {
  const { elementRef, savePosition } = useCursorPosition(props.value, inputRef);
  const handleKeyDown = useEnterKeyBlur(onSubmit);

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

  const styles: React.CSSProperties = {
    ...getWidth(props.width),
  };

  const shortcutDisplay = formatShortcutForDisplay(props.shortcutKey);
  const hasValue = props.value && props.value.toString().trim() !== '';
  const prefixContent = renderAffix(props.prefix);
  const suffixContent = renderAffix(props.suffix);
  const hasAffixes = prefixContent || suffixContent;
  const showClear = props.nullable && !props.disabled && hasValue;

  return (
    <div className="relative w-full select-none" style={styles}>
      <div
        className={cn(
          'relative flex items-stretch rounded-field border border-input bg-transparent shadow-sm transition-colors dark:bg-white/5 dark:border-white/10',
          isFocused && 'outline-none ring-1 ring-ring',
          props.invalid && 'border-destructive',
          props.disabled && 'cursor-not-allowed opacity-50'
        )}
      >
        {/* Prefix with background and separator */}
        {prefixContent && (
          <div className="flex items-center px-3 bg-muted text-muted-foreground border-r border-input rounded-tl-[var(--radius-fields)] rounded-bl-[var(--radius-fields)]">
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
            maxLength={props.maxLength}
            minLength={props.minLength}
            pattern={props.pattern}
            onChange={handleChange}
            onBlur={onBlur}
            onFocus={onFocus}
            onKeyDown={handleKeyDown}
            onPaste={handlePaste}
            className={cn(
              textInputSizeVariant({ density }),
              props.invalid && inputStyles.invalidInput,
              (props.invalid || showClear) && 'pr-8',
              props.shortcutKey &&
                !isFocused &&
                !hasValue &&
                !showClear &&
                !props.invalid &&
                'pr-16',
              showClear && props.invalid && 'pr-16',
              !hasValue &&
                props.nullable &&
                'placeholder:text-muted-foreground',
              'border-0 shadow-none focus-visible:ring-0 focus-visible:ring-offset-0 dark:bg-transparent',
              prefixContent && 'rounded-l-none',
              suffixContent && 'rounded-r-none',
              !hasAffixes && 'rounded-field'
            )}
            data-testid={props['data-testid']}
          />

          {/* Right side container: shortcut (if any), clear (if nullable), then invalid (if any) */}
          {(props.shortcutKey || showClear || props.invalid) && (
            <div className="absolute right-2 top-1/2 -translate-y-1/2 flex flex-row items-center gap-1 pointer-events-none">
              {props.shortcutKey &&
                !isFocused &&
                !hasValue &&
                !showClear &&
                !props.invalid && (
                  <div className="pointer-events-auto flex items-center h-6">
                    <kbd className="px-1 py-0.5 text-xs font-medium text-foreground bg-muted border border-border rounded-selector">
                      {shortcutDisplay}
                    </kbd>
                  </div>
                )}
              {showClear && (
                <button
                  type="button"
                  tabIndex={-1}
                  aria-label="Clear"
                  onClick={onClear}
                  className="pointer-events-auto p-1 rounded hover:bg-accent focus:outline-none cursor-pointer"
                >
                  <X className={xIconVariant({ density })} />
                </button>
              )}
              {/* Invalid icon - rightmost */}
              {props.invalid && (
                <div className="flex items-center h-6">
                  <InvalidIcon message={props.invalid} />
                </div>
              )}
            </div>
          )}
        </div>

        {/* Suffix with background and separator */}
        {suffixContent && (
          <div className="flex items-center px-3 bg-muted text-muted-foreground border-l border-input rounded-tr-[var(--radius-fields)] rounded-br-[var(--radius-fields)]">
            {suffixContent}
          </div>
        )}
      </div>
    </div>
  );
};
