import React from 'react';
import { Textarea } from '@/components/ui/textarea';
import { cn } from '@/lib/utils';
import { getWidth, getHeight, inputStyles } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';
import { Scales } from '@/types/scale';
import { textInputSizeVariants } from '@/components/ui/input/text-input-variants';
import { TextInputWidgetProps } from '../types';
import {
  useCursorPosition,
  usePasteHandler,
  formatShortcutForDisplay,
} from '../hooks';

interface TextareaVariantProps {
  props: Omit<TextInputWidgetProps, 'variant'>;
  onChange: (e: React.ChangeEvent<HTMLTextAreaElement>) => void;
  onBlur: (e: React.FocusEvent<HTMLTextAreaElement>) => void;
  onFocus: (e: React.FocusEvent<HTMLTextAreaElement>) => void;
  width?: string;
  inputRef?: React.RefObject<HTMLInputElement | HTMLTextAreaElement | null>;
  isFocused: boolean;
  scale?: Scales;
}

export const TextareaVariant: React.FC<TextareaVariantProps> = ({
  props,
  onChange,
  onBlur,
  onFocus,
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

  return (
    <div className="relative w-full select-none">
      <Textarea
        style={styles}
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
          textInputSizeVariants({ scale }),
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
