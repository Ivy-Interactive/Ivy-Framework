import React from "react";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import { getWidth, inputStyles } from "@/lib/styles";
import { InvalidIcon } from "@/components/InvalidIcon";
import { Densities } from "@/types/density";
import {
  textInputAffixCellClasses,
  textInputSizeVariant,
  xIconVariant,
} from "@/components/ui/input/text-input-variant";
import { TextInputWidgetProps } from "../types";
import {
  useCursorPosition,
  useEnterKeyBlur,
  usePasteHandler,
  formatShortcutForDisplay,
} from "../hooks";
import { Mic, X } from "lucide-react";

interface DefaultVariantProps {
  type: Lowercase<TextInputWidgetProps["variant"]>;
  props: Omit<TextInputWidgetProps, "variant"> & {
    dictation?: boolean;
    isRecording?: boolean;
    onDictationToggle?: () => void;
  };
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

  const handlePaste = usePasteHandler(props.maxLength, (value) => {
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
  const hasValue = props.value && props.value.toString().trim() !== "";
  const prefixContent = props.slots?.Prefix;
  const suffixContent = props.slots?.Suffix;
  const hasPrefix = (prefixContent?.length ?? 0) > 0;
  const hasSuffix = (suffixContent?.length ?? 0) > 0;
  const hasAffixes = hasPrefix || hasSuffix;
  const ghostAffixChrome = Boolean(props.ghost && hasAffixes);
  const showClear = props.nullable && !props.disabled && hasValue;
  const ghostTrailingTight = Boolean(props.ghost && hasSuffix);

  return (
    <div className="relative w-full select-none" style={styles}>
      <div
        className={cn(
          "relative flex items-stretch rounded-field border bg-transparent shadow-sm transition-colors dark:bg-white/5",
          isFocused
            ? "border-ring outline-none dark:border-ring"
            : "border-input dark:border-white/10",
          props.invalid && "border-destructive",
          props.disabled && "cursor-not-allowed opacity-50",
          props.ghost &&
            "border-transparent shadow-none bg-transparent dark:border-transparent dark:bg-transparent",
        )}
      >
        {/* Prefix with background and separator */}
        {hasPrefix && (
          <div className={textInputAffixCellClasses("prefix", isFocused, ghostAffixChrome)}>
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
              (props.invalid || showClear) && "pr-8",
              props.shortcutKey &&
                !isFocused &&
                !hasValue &&
                !showClear &&
                !props.invalid &&
                "pr-16",
              showClear && props.invalid && "pr-16",
              !hasValue && props.nullable && "placeholder:text-muted-foreground",
              "border-0 shadow-none focus-visible:ring-0 focus-visible:ring-offset-0 dark:bg-transparent",
              hasPrefix && "rounded-l-none",
              hasSuffix && "rounded-r-none",
              !hasAffixes && "rounded-field",
            )}
            data-testid={props["data-testid"]}
          />

          {/* Right side container: shortcut (if any), clear (if nullable), then invalid (if any) */}
          {(props.shortcutKey || showClear || props.invalid) && (
            <div
              className={cn(
                "pointer-events-none absolute top-1/2 flex -translate-y-1/2 flex-row items-center",
                ghostTrailingTight ? "right-0 gap-0 pr-0" : "right-2 gap-1",
              )}
            >
              {props.shortcutKey && !isFocused && !hasValue && !showClear && !props.invalid && (
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

        {/* Dictation mic button */}
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
              "flex items-center justify-center px-2 border-l hover:bg-accent focus:outline-none cursor-pointer transition-colors",
              props.ghost ? "border-border/30" : "border-input",
              props.isRecording && "bg-destructive/10 text-destructive",
            )}
          >
            <Mic className={cn("h-4 w-4", props.isRecording && "animate-pulse text-destructive")} />
          </button>
        )}

        {/* Suffix with background and separator */}
        {hasSuffix && (
          <div className={textInputAffixCellClasses("suffix", isFocused, ghostAffixChrome)}>
            {suffixContent}
          </div>
        )}
      </div>
    </div>
  );
};
