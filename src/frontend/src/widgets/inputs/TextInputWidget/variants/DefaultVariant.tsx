import React from "react";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import { getWidth, inputStyles } from "@/lib/styles";
import { InvalidIcon } from "@/components/InvalidIcon";
import { Densities } from "@/types/density";
import {
  textInputAffixCellClasses,
  textInputSizeVariant,
  textInputTrailingIconButtonClasses,
  textInputTrailingIconSizeVariant,
  textInputTrailingInvalidSlotClasses,
  textInputSuffixWithTrailingClusterClasses,
  textInputSuffixGlyphSlotClasses,
  textInputTrailingOverlayClasses,
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
  const showClear = props.nullable && !props.disabled && hasValue;
  const trailingBesideSuffix = hasSuffix;
  const showShortcut = Boolean(
    props.shortcutKey && !isFocused && !hasValue && !showClear && !props.invalid,
  );
  const showTrailing = showShortcut || showClear || Boolean(props.invalid);

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
        {hasPrefix && (
          <div className={textInputAffixCellClasses("prefix", density)}>{prefixContent}</div>
        )}

        <div className={cn("relative flex-1", trailingBesideSuffix && "min-w-0")}>
          <Input
            ref={elementRef as React.RefObject<HTMLInputElement>}
            id={props.id}
            density={density}
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
              trailingBesideSuffix && showTrailing && "pr-2",
              !trailingBesideSuffix && (props.invalid || showClear) && "pr-8",
              !trailingBesideSuffix && showShortcut && "pr-16",
              !trailingBesideSuffix && showClear && props.invalid && "pr-16",
              !hasValue && props.nullable && "placeholder:text-muted-foreground",
              "border-0 shadow-none focus-visible:ring-0 focus-visible:ring-offset-0 dark:bg-transparent",
              hasPrefix && "rounded-l-none",
              hasSuffix && "rounded-r-none",
              !hasAffixes && "rounded-field",
            )}
            data-testid={props["data-testid"]}
          />

          {!trailingBesideSuffix && showTrailing && (
            <div className={textInputTrailingOverlayClasses}>
              {showShortcut && (
                <div className="pointer-events-auto flex h-6 items-center">
                  <kbd className="rounded-selector border border-border bg-muted px-1 py-0.5 text-xs font-medium text-foreground">
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
                  className={textInputTrailingIconButtonClasses(true)}
                >
                  <X className={textInputTrailingIconSizeVariant({ density })} />
                </button>
              )}
              {props.invalid && (
                <InvalidIcon
                  message={props.invalid}
                  className={textInputTrailingInvalidSlotClasses(true)}
                  iconClassName={textInputTrailingIconSizeVariant({ density })}
                />
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
            <Mic className={cn("size-4", props.isRecording && "animate-pulse text-destructive")} />
          </button>
        )}

        {hasSuffix && (
          <div
            className={cn(
              textInputAffixCellClasses("suffix", density),
              trailingBesideSuffix && showTrailing && textInputSuffixWithTrailingClusterClasses,
            )}
          >
            {trailingBesideSuffix && showTrailing && (
              <>
                {showShortcut && (
                  <kbd className="rounded-selector border border-border bg-muted px-1 py-0.5 text-xs font-medium text-foreground">
                    {shortcutDisplay}
                  </kbd>
                )}
                {showClear && (
                  <button
                    type="button"
                    tabIndex={-1}
                    aria-label="Clear"
                    onClick={onClear}
                    className={textInputTrailingIconButtonClasses(false)}
                  >
                    <X className={textInputTrailingIconSizeVariant({ density })} />
                  </button>
                )}
                {props.invalid && (
                  <InvalidIcon
                    message={props.invalid}
                    className={textInputTrailingInvalidSlotClasses(false)}
                    iconClassName={textInputTrailingIconSizeVariant({ density })}
                  />
                )}
              </>
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
