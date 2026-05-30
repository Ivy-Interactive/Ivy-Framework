import React from "react";
import { Textarea } from "@/components/ui/textarea";
import { cn } from "@/lib/utils";
import { getWidth, getHeight, inputStyles } from "@/lib/styles";
import { InvalidIcon } from "@/components/InvalidIcon";
import { Densities } from "@/types/density";
import {
  textareaSizeVariant,
  textareaTrailingOverlayClasses,
  textareaSuffixWithTrailingClusterClasses,
  textInputAffixCellClasses,
  textInputSuffixGlyphSlotClasses,
  textInputTrailingIconButtonClasses,
  textInputTrailingIconSizeVariant,
  textInputTrailingInvalidSlotClasses,
} from "@/components/ui/input/text-input-variant";
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

const textareaAffixAlignClasses = (density: Densities) =>
  cn(
    "self-start",
    density === Densities.Small && "pt-2",
    density === Densities.Medium && "pt-2",
    density === Densities.Large && "pt-3",
  );

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
  const prefixContent = props.slots?.Prefix;
  const suffixContent = props.slots?.Suffix;
  const hasPrefix = (prefixContent?.length ?? 0) > 0;
  const hasSuffix = (suffixContent?.length ?? 0) > 0;
  const hasAffixes = hasPrefix || hasSuffix;
  const trailingBesideSuffix = hasSuffix;
  const showShortcut = Boolean(
    props.shortcutKey && !isFocused && !hasValue && !showClear && !props.invalid,
  );
  const showTrailing =
    showShortcut || showClear || Boolean(props.invalid) || Boolean(props.dictation);

  const trailingCluster = (overlay: boolean) => (
    <>
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
            textInputTrailingIconButtonClasses(overlay),
            props.isRecording && "bg-destructive/10 text-destructive",
          )}
        >
          <Mic className={cn("size-4", props.isRecording && "animate-pulse text-destructive")} />
        </button>
      )}
      {showClear && (
        <button
          type="button"
          tabIndex={-1}
          aria-label="Clear text"
          onClick={onClear}
          className={textInputTrailingIconButtonClasses(overlay)}
        >
          <X className={textInputTrailingIconSizeVariant({ density })} />
        </button>
      )}
      {showShortcut && (
        <div className={cn("flex shrink-0 items-center", overlay && "pointer-events-auto")}>
          <kbd className="rounded-field border border-border bg-muted px-1 py-0.5 text-xs font-medium text-foreground">
            {shortcutDisplay}
          </kbd>
        </div>
      )}
      {props.invalid && (
        <InvalidIcon
          message={props.invalid}
          className={textInputTrailingInvalidSlotClasses(overlay)}
          iconClassName={textInputTrailingIconSizeVariant({ density })}
        />
      )}
    </>
  );

  const textareaElement = (
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
        !trailingBesideSuffix && (props.invalid || showClear) && "pr-8",
        !trailingBesideSuffix && showShortcut && "pr-16",
        !trailingBesideSuffix && showClear && props.invalid && "pr-16",
        trailingBesideSuffix && showTrailing && "pr-2",
        !hasValue && props.nullable && "placeholder:text-muted-foreground",
        hasPrefix && "rounded-l-none",
        hasSuffix && "rounded-r-none",
        hasAffixes && "rounded-none",
        !hasAffixes && "rounded-field",
      )}
      data-testid={props["data-testid"]}
    />
  );

  const fieldShell = (
    <div className={cn("relative min-w-0 flex-1", trailingBesideSuffix && "min-w-0")}>
      <div
        className={cn(
          !hasAffixes &&
            "rounded-field border border-input bg-transparent shadow-sm dark:border-white/10 dark:bg-white/5",
          props.ghost &&
            "border-transparent bg-transparent shadow-none dark:border-transparent dark:bg-transparent",
        )}
      >
        {textareaElement}
      </div>
      {!trailingBesideSuffix && showTrailing && (
        <div className={textareaTrailingOverlayClasses}>{trailingCluster(true)}</div>
      )}
    </div>
  );

  if (!hasAffixes) {
    return (
      <div className="relative w-full select-none" style={wrapperStyles}>
        {fieldShell}
      </div>
    );
  }

  return (
    <div className="relative w-full select-none" style={wrapperStyles}>
      <div
        className={cn(
          "relative flex items-stretch rounded-field border border-input bg-transparent shadow-sm transition-colors dark:border-white/10 dark:bg-white/5",
          props.invalid && "border-destructive",
          props.disabled && "cursor-not-allowed opacity-50",
          props.ghost &&
            "border-transparent bg-transparent shadow-none dark:border-transparent dark:bg-transparent",
        )}
      >
        {hasPrefix && (
          <div
            className={cn(
              textInputAffixCellClasses("prefix", density),
              textareaAffixAlignClasses(density),
            )}
          >
            {prefixContent}
          </div>
        )}
        {fieldShell}
        {hasSuffix && (
          <div
            className={cn(
              textInputAffixCellClasses("suffix", density),
              textareaAffixAlignClasses(density),
              trailingBesideSuffix && showTrailing && textareaSuffixWithTrailingClusterClasses,
            )}
          >
            {trailingBesideSuffix && showTrailing && (
              <div className="flex flex-col items-center gap-1.5">{trailingCluster(false)}</div>
            )}
            {trailingBesideSuffix && showTrailing ? (
              <span className={textInputSuffixGlyphSlotClasses(density)}>{suffixContent}</span>
            ) : (
              suffixContent
            )}
          </div>
        )}
      </div>
    </div>
  );
};
