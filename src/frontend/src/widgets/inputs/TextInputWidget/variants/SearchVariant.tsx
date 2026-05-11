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
  textInputAffixCellClasses,
  textInputSizeVariant,
  searchIconVariant,
  xIconVariant,
} from "@/components/ui/input/text-input-variant";
import { TextInputWidgetProps } from "../types";
import { useCursorPosition, usePasteHandler, formatShortcutForDisplay } from "../hooks";

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

  const shortcutDisplay = formatShortcutForDisplay(props.shortcutKey);
  const hasValue = Boolean(props.value && String(props.value).trim() !== "");
  const prefixContent = props.slots?.Prefix;
  const suffixContent = props.slots?.Suffix;
  const hasPrefix = (prefixContent?.length ?? 0) > 0;
  const hasSuffix = (suffixContent?.length ?? 0) > 0;
  const hasAffixes = hasPrefix || hasSuffix;
  const ghostAffixChrome = Boolean(props.ghost && hasAffixes);
  const ghostSuffixLayout = Boolean(props.ghost && hasSuffix);
  const showClear = ghostSuffixLayout
    ? !props.disabled && hasValue
    : props.nullable && !props.disabled && hasValue;
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

  const kbd = (
    <kbd className="rounded-selector border border-border bg-muted px-1 py-0.25 text-xs text-foreground">
      {shortcutDisplay}
    </kbd>
  );

  const trailingCluster = (overlay: boolean) => (
    <>
      {showClear && (
        <button
          type="button"
          tabIndex={-1}
          aria-label="Clear search"
          onClick={onClear}
          className={cn(
            "flex h-6 shrink-0 items-center rounded hover:bg-accent focus:outline-none cursor-pointer",
            overlay ? "p-1 pointer-events-auto" : "p-0.5",
          )}
        >
          <X className={xIconVariant({ density })} />
        </button>
      )}
      {showShortcut && (
        <div className={cn("flex h-4 shrink-0 items-center", overlay && "pointer-events-auto")}>
          {kbd}
        </div>
      )}
      {props.invalid && (
        <div className={cn("flex h-6 shrink-0 items-center", overlay && "pointer-events-auto")}>
          <InvalidIcon message={props.invalid} />
        </div>
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
          <div className={textInputAffixCellClasses("prefix", isFocused, ghostAffixChrome)}>
            {prefixContent}
          </div>
        )}

        <div className={cn("relative flex-1", ghostSuffixLayout && "min-w-0")}>
          <Search className={searchIconVariant({ density })} />
          <Input
            ref={mergedRef}
            id={props.id}
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
              "pl-8 cursor-pointer border-0 shadow-none focus-visible:ring-0 focus-visible:ring-offset-0 dark:bg-transparent",
              props.invalid && inputStyles.invalidInput,
              ghostSuffixLayout && showTrailing && "pr-2",
              !ghostSuffixLayout && (props.invalid || showClear) && "pr-8",
              !ghostSuffixLayout &&
                props.shortcutKey &&
                !isFocused &&
                !hasValue &&
                !showClear &&
                !props.invalid &&
                "pr-16",
              !ghostSuffixLayout && showClear && props.invalid && "pr-16",
              !hasValue && props.nullable && "placeholder:text-muted-foreground",
              "[&::-webkit-search-cancel-button]:appearance-none [&::-webkit-search-cancel-button]:hidden",
              hasPrefix && "rounded-l-none",
              hasSuffix && "rounded-r-none",
              !hasAffixes && "rounded-field",
            )}
            data-testid={props["data-testid"]}
          />
          {!ghostSuffixLayout && showTrailing && (
            <div className="pointer-events-none absolute inset-y-0 left-0 right-0 z-10 flex items-center justify-end gap-2 pr-2.5">
              {trailingCluster(true)}
            </div>
          )}
        </div>

        {ghostSuffixLayout && showTrailing && (
          <div className="relative z-10 flex shrink-0 items-center gap-0 self-stretch bg-transparent px-0 text-muted-foreground">
            {trailingCluster(false)}
          </div>
        )}

        {hasSuffix && (
          <div className={textInputAffixCellClasses("suffix", isFocused, ghostAffixChrome)}>
            {suffixContent}
          </div>
        )}
      </div>
    </div>
  );
};
