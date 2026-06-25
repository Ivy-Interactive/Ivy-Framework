import React from "react";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import { getWidth, inputStyles } from "@/lib/styles";
import { InvalidIcon } from "@/components/InvalidIcon";
import { Densities } from "@/types/density";
import {
  textInputAffixInputColumnClasses,
  textInputAffixPrefixCellClasses,
  textInputAffixSuffixCellClasses,
  textInputEmbeddedInputClasses,
  textInputFieldShellClasses,
  textInputSizeVariant,
  textInputTrailingIconButtonClasses,
  textInputTrailingIconSizeVariant,
  textInputTrailingInvalidSlotClasses,
  textInputSuffixGlyphSlotClasses,
  textInputTrailingOverlayClasses,
  textInputTrailingShortcutWrapperClasses,
} from "@/components/ui/input/text-input-variant";
import { TextInputWidgetProps } from "../types";
import { useCursorPosition, useEnterKeyBlur, usePasteHandler } from "../hooks";
import { ShortcutKeys } from "@/components/Kbd";
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
        className={textInputFieldShellClasses({
          focused: isFocused,
          invalid: props.invalid,
          disabled: props.disabled,
          ghost: props.ghost,
        })}
      >
        {hasPrefix && (
          <div className={textInputAffixPrefixCellClasses(density, prefixContent)}>
            {prefixContent}
          </div>
        )}

        <div className={textInputAffixInputColumnClasses({ trailingBesideSuffix })}>
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
              textInputEmbeddedInputClasses(hasAffixes, density),
              props.invalid && inputStyles.invalidInput,
              trailingBesideSuffix && showTrailing && "pr-2",
              !trailingBesideSuffix && (props.invalid || showClear) && "!pr-8",
              !trailingBesideSuffix && showShortcut && "!pr-16",
              !trailingBesideSuffix && showClear && props.invalid && "!pr-16",
              !hasValue && props.nullable && "placeholder:text-muted-foreground",
            )}
            data-testid={props["data-testid"]}
          />

          {!trailingBesideSuffix && showTrailing && (
            <div className={textInputTrailingOverlayClasses(density)}>
              {showShortcut && (
                <div
                  className={cn(
                    "pointer-events-auto",
                    textInputTrailingShortcutWrapperClasses(density),
                  )}
                >
                  <ShortcutKeys shortcut={props.shortcutKey ?? ""} />
                </div>
              )}
              {showClear && (
                <button
                  type="button"
                  tabIndex={-1}
                  aria-label="Clear"
                  onClick={onClear}
                  className={textInputTrailingIconButtonClasses(true, density)}
                >
                  <X className={textInputTrailingIconSizeVariant({ density })} />
                </button>
              )}
              {props.invalid && (
                <InvalidIcon
                  message={props.invalid}
                  className={textInputTrailingInvalidSlotClasses(true, density)}
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
            className={textInputAffixSuffixCellClasses(density, suffixContent, {
              showTrailing: trailingBesideSuffix && showTrailing,
            })}
          >
            {trailingBesideSuffix && showTrailing && (
              <>
                {showShortcut && <ShortcutKeys shortcut={props.shortcutKey ?? ""} />}
                {showClear && (
                  <button
                    type="button"
                    tabIndex={-1}
                    aria-label="Clear"
                    onClick={onClear}
                    className={textInputTrailingIconButtonClasses(false, density)}
                  >
                    <X className={textInputTrailingIconSizeVariant({ density })} />
                  </button>
                )}
                {props.invalid && (
                  <InvalidIcon
                    message={props.invalid}
                    className={textInputTrailingInvalidSlotClasses(false, density)}
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
