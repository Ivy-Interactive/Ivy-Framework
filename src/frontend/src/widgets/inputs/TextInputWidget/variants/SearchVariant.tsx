import React, { useCallback, useRef } from "react";
import { Input } from "@/components/ui/input";
import { Search, X } from "lucide-react";
import { cn } from "@/lib/utils";
import { getWidth, inputStyles } from "@/lib/styles";
import { InvalidIcon } from "@/components/InvalidIcon";
import { useFocusable } from "@/hooks/use-focus-management";
import { sidebarMenuRef } from "@/widgets/layouts/sidebar";
import { Densities } from "@/types/density";
import {
  textInputAffixInputColumnClasses,
  textInputAffixPrefixCellClasses,
  textInputAffixSuffixCellClasses,
  textInputEmbeddedInputClasses,
  textInputSizeVariant,
  textInputTrailingIconButtonClasses,
  textInputTrailingIconSizeVariant,
  textInputTrailingInvalidSlotClasses,
  textInputSuffixGlyphSlotClasses,
  textInputTrailingShortcutWrapperClasses,
  searchIconVariant,
  searchInputPaddingVariant,
} from "@/components/ui/input/text-input-variant";
import { TextInputWidgetProps } from "../types";
import { useCursorPosition, usePasteHandler } from "../hooks";
import { ShortcutKeys } from "@/components/Kbd";

interface SearchVariantProps {
  props: Omit<TextInputWidgetProps, "variant">;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onBlur: (e: React.FocusEvent<HTMLInputElement>) => void;
  onFocus: (e: React.FocusEvent<HTMLInputElement>) => void;
  onClear: (e: React.MouseEvent) => void;
  onSubmit?: () => void;
  width?: string;
  inputRef?: React.RefObject<HTMLInputElement | HTMLTextAreaElement | null>;
  isFocused: boolean;
  density?: Densities;
}

export const SearchVariant: React.FC<SearchVariantProps> = ({
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
  const { savePosition } = useCursorPosition(props.value, inputRef) as {
    savePosition: () => void;
  };
  const { ref: focusRef } = useFocusable("sidebar-navigation", 0);
  const shouldFocusMenuRef = useRef(false);

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

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "ArrowDown" || e.key === "ArrowUp" || e.key === "Enter") {
      if (e.key === "Enter") {
        onSubmit?.();
      }
      shouldFocusMenuRef.current = true;
      e.currentTarget.blur();
      e.preventDefault();
    }
  };

  const handleBlur = (e: React.FocusEvent<HTMLInputElement>) => {
    if (shouldFocusMenuRef.current) {
      shouldFocusMenuRef.current = false;
      sidebarMenuRef.current?.focus();
    }
    onBlur(e);
  };

  const styles: React.CSSProperties = {
    ...getWidth(props.width),
  };

  const hasValue = Boolean(props.value && String(props.value).trim() !== "");
  const prefixContent = props.slots?.Prefix;
  const suffixContent = props.slots?.Suffix;
  const hasPrefix = (prefixContent?.length ?? 0) > 0;
  const hasSuffix = (suffixContent?.length ?? 0) > 0;
  const hasAffixes = hasPrefix || hasSuffix;
  /** Trailing controls sit beside the suffix affix, not inside padded input text. */
  const trailingBesideSuffix = hasSuffix;
  const showBuiltinSearchIcon = !hasPrefix;
  const showClear = !props.disabled && hasValue;
  const showShortcut =
    Boolean(props.shortcutKey) && !isFocused && !hasValue && !showClear && !props.invalid;
  const showTrailing = showClear || showShortcut || Boolean(props.invalid);

  const mergedRef = useCallback(
    (element: HTMLInputElement | null) => {
      focusRef(element);
      if (inputRef && "current" in inputRef) {
        Reflect.set(inputRef, "current", element);
      }
    },
    [focusRef, inputRef],
  );

  const kbd = props.shortcutKey ? <ShortcutKeys shortcut={props.shortcutKey} /> : null;

  const trailingCluster = (overlay: boolean) => (
    <>
      {showClear && (
        <button
          type="button"
          tabIndex={-1}
          aria-label="Clear search"
          onClick={onClear}
          className={textInputTrailingIconButtonClasses(overlay, density)}
        >
          <X className={textInputTrailingIconSizeVariant({ density })} />
        </button>
      )}
      {showShortcut && (
        <div
          className={cn(
            textInputTrailingShortcutWrapperClasses(density),
            overlay && "pointer-events-auto",
          )}
        >
          {kbd}
        </div>
      )}
      {props.invalid && (
        <InvalidIcon
          message={props.invalid}
          className={textInputTrailingInvalidSlotClasses(overlay, density)}
          iconClassName={textInputTrailingIconSizeVariant({ density })}
        />
      )}
    </>
  );

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
          <div className={textInputAffixPrefixCellClasses(density, prefixContent)}>
            {prefixContent}
          </div>
        )}

        <div className={textInputAffixInputColumnClasses({ trailingBesideSuffix })}>
          {showBuiltinSearchIcon && <Search className={searchIconVariant({ density })} />}
          <Input
            ref={mergedRef}
            id={props.id}
            density={density}
            type="search"
            placeholder={props.placeholder}
            value={props.value}
            disabled={props.disabled}
            maxLength={props.maxLength}
            minLength={props.minLength}
            pattern={props.pattern}
            onChange={handleChange}
            onBlur={handleBlur}
            onFocus={onFocus}
            onKeyDown={handleKeyDown}
            onPaste={handlePaste}
            autoComplete="off"
            className={cn(
              textInputSizeVariant({ density }),
              "cursor-pointer",
              textInputEmbeddedInputClasses(hasAffixes, density),
              showBuiltinSearchIcon && searchInputPaddingVariant({ density }),
              props.invalid && inputStyles.invalidInput,
              trailingBesideSuffix && showTrailing && "pr-2",
              !trailingBesideSuffix && (props.invalid || showClear) && "!pr-8",
              !trailingBesideSuffix && showShortcut && "!pr-16",
              !trailingBesideSuffix && showClear && props.invalid && "!pr-16",
              !hasValue && props.nullable && "placeholder:text-muted-foreground",
              "[&::-webkit-search-cancel-button]:appearance-none [&::-webkit-search-cancel-button]:hidden",
            )}
            data-testid={props["data-testid"]}
          />
          {!trailingBesideSuffix && showTrailing && (
            <div className="pointer-events-none absolute inset-y-0 left-0 right-0 z-10 flex items-center justify-end gap-2 pr-2.5">
              {trailingCluster(true)}
            </div>
          )}
        </div>

        {hasSuffix && (
          <div
            className={textInputAffixSuffixCellClasses(density, suffixContent, {
              showTrailing: trailingBesideSuffix && showTrailing,
            })}
          >
            {trailingBesideSuffix && showTrailing && trailingCluster(false)}
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
