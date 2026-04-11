import React from "react";
import { Textarea } from "@/components/ui/textarea";
import { cn } from "@/lib/utils";
import { getWidth, getHeight, inputStyles } from "@/lib/styles";
import { InvalidIcon } from "@/components/InvalidIcon";
import { Densities } from "@/types/density";
import { textareaSizeVariant, xIconVariant } from "@/components/ui/input/text-input-variant";
import { TextInputWidgetProps } from "../types";
import { useCursorPosition, usePasteHandler, formatShortcutForDisplay } from "../hooks";
import { Mic, X } from "lucide-react";

interface TextareaVariantProps {
  props: Omit<TextInputWidgetProps, "variant"> & {
    dictation?: boolean;
    isRecording?: boolean;
    onDictationToggle?: () => void;
  };
  onChange: (e: React.ChangeEvent<HTMLTextAreaElement>) => void;
  onBlur: (e: React.FocusEvent<HTMLTextAreaElement>) => void;
  onFocus: (e: React.FocusEvent<HTMLTextAreaElement>) => void;
  onClear: (e: React.MouseEvent) => void;
  onSubmit?: () => void;
  width?: string;
  inputRef?: React.RefObject<HTMLInputElement | HTMLTextAreaElement | null>;
  isFocused: boolean;
  nullable?: boolean;
  density?: Densities;
}

export const TextareaVariant: React.FC<TextareaVariantProps> = ({
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
  const handleChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    savePosition();
    onChange(e);
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if ((e.metaKey || e.ctrlKey) && e.key === "Enter") {
      e.preventDefault();
      onSubmit?.();
      e.currentTarget.blur();
    }
  };

  const handlePaste = usePasteHandler(props.maxLength, (value) => {
    const syntheticEvent = {
      target: { value },
      currentTarget: { value },
    } as React.ChangeEvent<HTMLTextAreaElement>;
    onChange(syntheticEvent);
  });

  const wrapperStyles: React.CSSProperties = {
    ...getWidth(props.width),
  };

  const textareaStyles: React.CSSProperties = {
    ...getHeight(props.height),
  };

  const shortcutDisplay = formatShortcutForDisplay(props.shortcutKey);
  const hasValue = props.value && props.value.toString().trim() !== "";
  const showClear = props.nullable && !props.disabled && hasValue;

  return (
    <div className="relative w-full select-none">
      <div
        className={cn(
          "rounded-field border border-input bg-transparent shadow-sm dark:bg-white/5 dark:border-white/10",
          props.ghost &&
            "border-transparent shadow-none bg-transparent dark:border-transparent dark:bg-transparent",
        )}
        style={wrapperStyles}
      >
        <Textarea
          ref={elementRef as React.RefObject<HTMLTextAreaElement>}
          id={props.id}
          placeholder={props.placeholder}
          value={props.value}
          disabled={props.disabled}
          maxLength={props.maxLength}
          minLength={props.minLength}
          rows={props.rows}
          onChange={handleChange}
          onBlur={onBlur}
          onFocus={onFocus}
          onKeyDown={handleKeyDown}
          onPaste={handlePaste}
          style={textareaStyles}
          className={cn(
            textareaSizeVariant({ density }),
            "border-0 shadow-none dark:bg-transparent",
            !props.height && "h-full",
            props.invalid && inputStyles.invalidInput,
            (props.invalid || showClear) && "pr-8",
            props.shortcutKey && !isFocused && !hasValue && !showClear && !props.invalid && "pr-16",
            showClear && props.invalid && "pr-16",
            !hasValue && props.nullable && "placeholder:text-muted-foreground",
          )}
          data-testid={props["data-testid"]}
        />
      </div>
      <div className="absolute right-2.5 top-2 flex items-start gap-2 pointer-events-none z-10">
        {props.dictation && !props.disabled && (
          <button
            type="button"
            tabIndex={-1}
            aria-label={props.isRecording ? "Stop dictation" : "Start dictation"}
            onClick={(e) => {
              e.preventDefault();
              e.stopPropagation();
              props.onDictationToggle?.();
            }}
            className={cn(
              "p-1 rounded hover:bg-accent focus:outline-none cursor-pointer pointer-events-auto flex items-center transition-colors",
              props.isRecording && "bg-destructive/10 text-destructive",
            )}
          >
            <Mic className={cn("h-4 w-4", props.isRecording && "animate-pulse text-destructive")} />
          </button>
        )}
        {showClear && (
          <button
            type="button"
            tabIndex={-1}
            aria-label="Clear text"
            onClick={onClear}
            className="p-1 rounded hover:bg-accent focus:outline-none cursor-pointer pointer-events-auto flex items-center"
            style={{ pointerEvents: "auto" }}
          >
            <X className={xIconVariant({ density })} />
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
