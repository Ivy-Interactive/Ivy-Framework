import React, { useState, useCallback, useRef, useEffect } from "react";
import { Input } from "@/components/ui/input";
import { EyeIcon, EyeOffIcon, X } from "lucide-react";
import { cn } from "@/lib/utils";
import { getWidth, inputStyles } from "@/lib/styles";
import { InvalidIcon } from "@/components/InvalidIcon";
import { Densities } from "@/types/density";
import {
  textInputAffixInputColumnClasses,
  textInputAffixPrefixCellClasses,
  textInputAffixSuffixCellClasses,
  textInputEmbeddedInputClasses,
  textInputSizeVariant,
  textInputSuffixGlyphSlotClasses,
  textInputTrailingIconButtonClasses,
  textInputTrailingIconSizeVariant,
  textInputTrailingInvalidSlotClasses,
  textInputTrailingOverlayClasses,
  textInputTrailingShortcutWrapperClasses,
} from "@/components/ui/input/text-input-variant";
import { TextInputWidgetProps } from "../types";
import { useCursorPosition, useEnterKeyBlur, usePasteHandler } from "../hooks";
import { ShortcutKeys } from "@/components/Kbd";

interface PasswordVariantProps {
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

export const PasswordVariant: React.FC<PasswordVariantProps> = ({
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
  const [showPassword, setShowPassword] = useState(false);
  const [hasLastPass, setHasLastPass] = useState(false);
  const { elementRef: elementRefGeneric, savePosition } = useCursorPosition(props.value, inputRef);
  const elementRef = elementRefGeneric as React.RefObject<HTMLInputElement>;
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const interval = setInterval(() => {
      if (containerRef.current?.querySelector("[data-lastpass-icon-root]")) {
        setHasLastPass(true);
        clearInterval(interval);
      }
    }, 300);
    return () => clearInterval(interval);
  }, []);

  const togglePassword = useCallback(() => {
    setShowPassword((prev) => !prev);
  }, []);

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

  const handleKeyDown = useEnterKeyBlur(onSubmit);

  const styles: React.CSSProperties = {
    ...getWidth(props.width),
  };

  const hasValue = props.value && props.value.toString().trim() !== "";
  const showClear = props.nullable && !props.disabled && hasValue;
  const ghostTight = Boolean(props.ghost);
  const prefixContent = props.slots?.Prefix;
  const suffixContent = props.slots?.Suffix;
  const hasPrefix = (prefixContent?.length ?? 0) > 0;
  const hasSuffix = (suffixContent?.length ?? 0) > 0;
  const hasAffixes = hasPrefix || hasSuffix;
  const trailingBesideSuffix = hasSuffix;
  const showPasswordToggle = !hasLastPass;
  const showShortcut = Boolean(props.shortcutKey && !hasValue && !showClear && !props.invalid);
  const showTrailing = showPasswordToggle || showClear || showShortcut || Boolean(props.invalid);

  const trailingCluster = (overlay: boolean) => (
    <>
      {showPasswordToggle && (
        <button
          type="button"
          className={textInputTrailingIconButtonClasses(overlay, density)}
          onClick={togglePassword}
          aria-label={showPassword ? "Hide password" : "Show password"}
        >
          {showPassword ? (
            <EyeOffIcon className={textInputTrailingIconSizeVariant({ density })} />
          ) : (
            <EyeIcon className={textInputTrailingIconSizeVariant({ density })} />
          )}
        </button>
      )}
      {showClear && (
        <button
          type="button"
          tabIndex={-1}
          aria-label="Clear"
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
          <ShortcutKeys
            shortcut={props.shortcutKey ?? ""}
            className={cn(!ghostTight && overlay && "ml-2")}
          />
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
    <div className="relative w-full select-none" style={styles} ref={containerRef}>
      <div
        className={cn(
          "relative flex items-stretch rounded-field border bg-transparent shadow-sm transition-colors dark:bg-white/5",
          isFocused
            ? "border-ring outline-none dark:border-ring"
            : "border-input dark:border-white/10",
          props.invalid && "border-destructive",
          props.disabled && "cursor-not-allowed opacity-50",
          props.ghost &&
            "border-transparent bg-transparent shadow-none dark:border-transparent dark:bg-transparent",
        )}
      >
        {hasPrefix && (
          <div className={textInputAffixPrefixCellClasses(density, prefixContent)}>
            {prefixContent}
          </div>
        )}

        <div className={textInputAffixInputColumnClasses({ trailingBesideSuffix })}>
          <Input
            ref={elementRef}
            id={props.id}
            density={density}
            placeholder={props.placeholder}
            value={props.value}
            type={showPassword ? "text" : "password"}
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
              !trailingBesideSuffix && (props.invalid || showClear) && "!pr-14",
              !trailingBesideSuffix &&
                !props.invalid &&
                !showClear &&
                showPasswordToggle &&
                "!pr-8",
              hasLastPass && "!pr-3",
              !trailingBesideSuffix &&
                showShortcut &&
                !hasLastPass &&
                !hasValue &&
                !showClear &&
                !props.invalid &&
                "!pr-24",
              !trailingBesideSuffix && showClear && props.invalid && !hasLastPass && "!pr-20",
              !hasValue && props.nullable && "placeholder:text-muted-foreground",
              "[&::-ms-reveal]:hidden [&::-ms-clear]:hidden",
            )}
            data-testid={props["data-testid"]}
          />

          {!trailingBesideSuffix && showTrailing && (
            <div className={textInputTrailingOverlayClasses(density)}>{trailingCluster(true)}</div>
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
