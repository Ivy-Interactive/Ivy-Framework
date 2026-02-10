import React from 'react';
import { Textarea } from '@/components/ui/textarea';
import { cn } from '@/lib/utils';
import { getWidth, getHeight, inputStyles } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';
import { Scales } from '@/types/scale';
import {
  textAreaSizeVariants,
  xIconVariants,
} from '@/components/ui/input/text-input-variants';
import { TextInputWidgetProps } from '../types';
import {
  useCursorPosition,
  usePasteHandler,
  formatShortcutForDisplay,
} from '../hooks';
import { X } from 'lucide-react';

interface TextareaVariantProps {
  props: Omit<TextInputWidgetProps, 'variant'>;
  onChange: (e: React.ChangeEvent<HTMLTextAreaElement>) => void;
  onBlur: (e: React.FocusEvent<HTMLTextAreaElement>) => void;
  onFocus: (e: React.FocusEvent<HTMLTextAreaElement>) => void;
  onClear: (e: React.MouseEvent) => void;
  width?: string;
  inputRef?: React.RefObject<HTMLInputElement | HTMLTextAreaElement | null>;
  isFocused: boolean;
  nullable?: boolean;
  scale?: Scales;
}

export const TextareaVariant: React.FC<TextareaVariantProps> = ({
  props,
  onChange,
  onBlur,
  onFocus,
  onClear,
  inputRef,
  isFocused,
  scale = Scales.Medium,
}) => {
  const { elementRef, savePosition } = useCursorPosition(props.value, inputRef);

  const handleChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    savePosition();
    onChange(e);
  };

  const handlePaste = usePasteHandler(props.maxLength, value => {
    const syntheticEvent = {
      target: { value },
      currentTarget: { value },
    } as React.ChangeEvent<HTMLTextAreaElement>;
    onChange(syntheticEvent);
  });

  const styles: React.CSSProperties = {
    ...getWidth(props.width),
    ...getHeight(props.height),
  };

  const shortcutDisplay = formatShortcutForDisplay(props.shortcutKey);
  const hasValue = props.value && props.value.toString().trim() !== '';
  const showClear = props.nullable && !props.disabled && hasValue;

  return (
    <div className="relative w-full select-none">
      <div
        className="rounded-field border border-input bg-transparent shadow-sm dark:bg-white/5 dark:border-white/10"
        style={styles}
      >
        <Textarea
          ref={elementRef as React.RefObject<HTMLTextAreaElement>}
          id={props.id}
          placeholder={props.placeholder}
          value={props.value}
          disabled={props.disabled}
          maxLength={props.maxLength}
          onChange={handleChange}
          onBlur={onBlur}
          onFocus={onFocus}
          onPaste={handlePaste}
          className={cn(
            textAreaSizeVariants({ scale }),
            'border-0 shadow-none dark:bg-transparent h-full',
            props.invalid && inputStyles.invalidInput,
            (props.invalid || showClear) && 'pr-8',
            props.shortcutKey &&
              !isFocused &&
              !hasValue &&
              !showClear &&
              !props.invalid &&
              'pr-16',
            showClear && props.invalid && 'pr-16',
            !hasValue && props.nullable && 'placeholder:text-muted-foreground'
          )}
          data-testid={props['data-testid']}
        />
      </div>
      <div className="absolute right-2.5 top-2 flex items-start gap-2 pointer-events-none z-10">
        {showClear && (
          <button
            type="button"
            tabIndex={-1}
            aria-label="Clear text"
            onClick={onClear}
            className="p-1 rounded hover:bg-accent focus:outline-none cursor-pointer pointer-events-auto flex items-center"
            style={{ pointerEvents: 'auto' }}
          >
            <X className={xIconVariants({ scale })} />
          </button>
        )}
        {props.shortcutKey && !isFocused && !hasValue && (
          <div className="pointer-events-auto flex items-center">
            <kbd className="px-1 py-0.5 text-xs font-medium text-foreground bg-muted border border-border rounded-field">
              {shortcutDisplay}
            </kbd>
          </div>
        )}
        {props.invalid && (
          <div className="flex items-center">
            <InvalidIcon message={props.invalid} />
          </div>
        )}
      </div>
    </div>
  );
};
