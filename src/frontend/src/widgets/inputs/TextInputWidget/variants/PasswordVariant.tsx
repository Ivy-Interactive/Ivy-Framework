import React, { useState, useCallback, useRef, useEffect } from "react";
import { Input } from "@/components/ui/input";
import { EyeIcon, EyeOffIcon, X } from "lucide-react";
import { cn } from "@/lib/utils";
import { getWidth, inputStyles } from "@/lib/styles";
import { InvalidIcon } from "@/components/InvalidIcon";
import { Densities } from "@/types/density";
import {
  textInputAffixCellClasses,
  textInputSizeVariant,
  eyeIconVariant,
  xIconVariant,
} from "@/components/ui/input/text-input-variant";
import { TextInputWidgetProps } from "../types";
import {
  useCursorPosition,
  useEnterKeyBlur,
  usePasteHandler,
  formatShortcutForDisplay,
} from "../hooks";

interface PasswordVariantProps {
  props: Omit<TextInputWidgetProps, "variant">;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onBlur: (e: React.FocusEvent<HTMLInputElement>) => void;
  onFocus: (e: React.FocusEvent<HTMLInputElement>) => void;
  onClear: (e: React.MouseEvent) => void;
  onSubmit?: () => void;
  width?: string;
  inputRef?: React.RefObject<HTMLInputElement | HTMLTextAreaElement | null>;
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

  const shortcutDisplay = formatShortcutForDisplay(props.shortcutKey);
  const hasValue = props.value && props.value.toString().trim() !== "";
  const showClear = props.nullable && !props.disabled && hasValue;
  const ghostTight = Boolean(props.ghost);
  const prefixContent = props.slots?.Prefix;
  const suffixContent = props.slots?.Suffix;
  const hasPrefix = (prefixContent?.length ?? 0) > 0;
  const hasSuffix = (suffixContent?.length ?? 0) > 0;
  const hasAffixes = hasPrefix || hasSuffix;
  const trailingBesideSuffix = hasSuffix;

  const trailingControls = !hasLastPass && (
    <div
      className={cn(
        "pointer-events-none absolute top-1/2 flex h-6 -translate-y-1/2 flex-row items-center",
        trailingBesideSuffix
          ? "right-0 gap-1 pr-0.5"
          : ghostTight
            ? "right-0 gap-1 pr-0.5"
            : "right-2 gap-1",
      )}
    >
      <div className="pointer-events-auto flex h-6 items-center">
        <button
          type="button"
          className={cn(
            "flex cursor-pointer items-center rounded hover:bg-accent focus:outline-none",
            ghostTight ? "p-0.5" : "p-1",
          )}
          onClick={togglePassword}
          aria-label={showPassword ? "Hide password" : "Show password"}
        >
          {showPassword ? (
            <EyeOffIcon className={cn("text-muted-foreground", eyeIconVariant({ density }))} />
          ) : (
            <EyeIcon className={cn("text-muted-foreground", eyeIconVariant({ density }))} />
          )}
        </button>
      </div>
      {showClear && (
        <button
          type="button"
          tabIndex={-1}
          aria-label="Clear"
          onClick={onClear}
          className="pointer-events-auto flex h-6 cursor-pointer items-center rounded p-1 hover:bg-accent focus:outline-none"
        >
          <X className={xIconVariant({ density })} />
        </button>
      )}
      {props.shortcutKey && !hasValue && !showClear && !props.invalid && (
        <div className="pointer-events-auto flex h-6 items-center">
          <kbd
            className={cn(
              "rounded-field border border-border bg-muted px-1 py-0.5 text-xs font-medium text-foreground",
              !ghostTight && !trailingBesideSuffix && "ml-2",
            )}
          >
            {shortcutDisplay}
          </kbd>
        </div>
      )}
      {props.invalid && (
        <div
          className={cn("flex h-6 items-center", !ghostTight && !trailingBesideSuffix && "ml-2")}
        >
          <InvalidIcon message={props.invalid} />
        </div>
      )}
    </div>
  );

  const inputField = (
    <div className={cn("relative flex-1", trailingBesideSuffix && "min-w-0")}>
      <div
        className={cn(
          !hasAffixes &&
            "rounded-field border border-input bg-transparent shadow-sm dark:bg-white/5 dark:border-white/10",
          props.ghost &&
            "border-transparent shadow-none bg-transparent dark:border-transparent dark:bg-transparent",
        )}
      >
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
            "border-0 shadow-none dark:bg-transparent",
            "[&::-ms-reveal]:hidden [&::-ms-clear]:hidden",
            props.invalid && inputStyles.invalidInput,
            trailingBesideSuffix && (props.invalid || showClear) && "pr-2",
            !trailingBesideSuffix && (props.invalid || showClear) && "pr-14",
            !trailingBesideSuffix && !props.invalid && !showClear && "pr-8",
            hasLastPass && "pr-3",
            !trailingBesideSuffix &&
              props.shortcutKey &&
              !hasLastPass &&
              !hasValue &&
              !showClear &&
              !props.invalid &&
              "pr-24",
            !trailingBesideSuffix && showClear && props.invalid && !hasLastPass && "pr-20",
            !hasValue && props.nullable && "placeholder:text-muted-foreground",
            hasPrefix && "rounded-l-none",
            hasSuffix && "rounded-r-none",
            hasAffixes && "rounded-none",
          )}
          data-testid={props["data-testid"]}
        />
      </div>
      {trailingControls}
    </div>
  );

  return (
    <div className="relative w-full select-none" style={styles} ref={containerRef}>
      {hasAffixes ? (
        <div
          className={cn(
            "relative flex items-stretch rounded-field border bg-transparent shadow-sm transition-colors dark:bg-white/5",
            "border-input dark:border-white/10",
            props.invalid && "border-destructive",
            props.disabled && "cursor-not-allowed opacity-50",
            props.ghost &&
              "border-transparent shadow-none bg-transparent dark:border-transparent dark:bg-transparent",
          )}
        >
          {hasPrefix && (
            <div className={textInputAffixCellClasses("prefix", density)}>{prefixContent}</div>
          )}
          {inputField}
          {hasSuffix && (
            <div className={textInputAffixCellClasses("suffix", density)}>{suffixContent}</div>
          )}
        </div>
      ) : (
        inputField
      )}
    </div>
  );
};
