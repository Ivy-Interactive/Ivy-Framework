import React from "react";
import { X } from "lucide-react";
import { cn } from "@/lib/utils";
import { InvalidIcon } from "@/components/InvalidIcon";
import { Densities } from "@/types/density";
import {
  normalizeInputDensity,
  textInputAffixCellClasses,
  textInputAffixControlColumnClasses,
  textInputAffixPrefixCellClasses,
  textInputAffixSuffixCellClasses,
  textInputSuffixWithTrailingClusterClasses,
  textInputFieldShellClasses,
  textInputSuffixGlyphSlotClasses,
  textInputTrailingIconButtonClasses,
  textInputTrailingIconSizeVariant,
  textInputTrailingInvalidSlotClasses,
} from "@/components/ui/input/text-input-variant";

const dateAffixCellChrome = "relative z-10 shrink-0 overflow-visible";

/** Affix strip aligned with shared input affix helpers. */
export function dateInputAffixCellClasses(
  side: "prefix" | "suffix",
  density: Densities,
  content?: React.ReactNode[],
  options: { showTrailing?: boolean } = {},
): string {
  if (side === "prefix") {
    return cn(textInputAffixPrefixCellClasses(density, content), dateAffixCellChrome);
  }
  return cn(
    textInputAffixSuffixCellClasses(density, content, { showTrailing: options.showTrailing }),
    dateAffixCellChrome,
  );
}

/** Invalid on inner trigger only when trailing is not in the suffix cluster. */
export function dateInputControlInvalid(
  inAffixShell?: boolean,
  trailingBesideSuffix?: boolean,
  showClear = false,
  invalid?: string,
): string | undefined {
  if (inAffixShell && trailingBesideSuffix && (showClear || invalid)) {
    return undefined;
  }
  return invalid;
}

/** Reserve space for clear/invalid overlay on the trigger when not in affix shell. */
export function dateInputTriggerTrailingPadding(
  inAffixShell?: boolean,
  trailingBesideSuffix?: boolean,
  showClear = false,
  invalid?: string,
): string {
  if (inAffixShell) {
    if (trailingBesideSuffix && (showClear || invalid)) return "pr-2";
    return "";
  }
  if (showClear && invalid) return "pr-16";
  if (showClear || invalid) return "pr-8";
  return "";
}

export interface DateInputAffixLayoutProps {
  inAffixShell?: boolean;
  trailingBesideSuffix?: boolean;
}

/** Outer field chrome — background fill lives here, not on inner trigger/input. */
export function dateInputFieldShellClasses(options: {
  focused?: boolean;
  invalid?: string;
  disabled?: boolean;
}): string {
  return cn(
    textInputFieldShellClasses({
      focused: options.focused,
      invalid: options.invalid,
      disabled: options.disabled,
    }),
    options.focused && "ring-1 ring-ring",
  );
}

/** Strip inner control chrome; zero horizontal padding when the affix column owns the seam gap. */
export const dateInputEmbeddedControlClasses =
  "[&_button]:border-0 [&_button]:bg-transparent [&_button]:!px-0 [&_button]:shadow-none [&_button]:dark:bg-transparent [&_button]:hover:bg-transparent [&_button]:dark:hover:bg-transparent [&_button]:focus-visible:ring-0 [&_button]:focus-visible:ring-offset-0 [&_input]:border-0 [&_input]:bg-transparent [&_input]:!px-0 [&_input]:shadow-none [&_input]:dark:bg-transparent [&_input]:focus-visible:ring-0";

interface DateInputAffixShellProps {
  density: Densities;
  invalid?: string;
  disabled?: boolean;
  focused?: boolean;
  hasPrefix: boolean;
  hasSuffix: boolean;
  prefixContent?: React.ReactNode[];
  suffixContent?: React.ReactNode[];
  showClear: boolean;
  onClear: (e?: React.MouseEvent) => void;
  children: React.ReactNode;
}

export function DateInputAffixShell({
  density,
  invalid,
  disabled,
  focused,
  hasPrefix,
  hasSuffix,
  prefixContent,
  suffixContent,
  showClear,
  onClear,
  children,
}: DateInputAffixShellProps) {
  const densityKey = normalizeInputDensity(density);
  const showTrailing = showClear || Boolean(invalid);
  const trailingBesideSuffix = hasSuffix;
  const trailingInAffixCell = !trailingBesideSuffix && showTrailing;
  const trailingControlCount = [showClear, Boolean(invalid)].filter(Boolean).length;

  const trailingCluster = () => (
    <>
      {showClear && (
        <button
          type="button"
          tabIndex={-1}
          aria-label="Clear"
          onClick={onClear}
          onPointerDown={(e) => e.stopPropagation()}
          className={textInputTrailingIconButtonClasses(false, density)}
        >
          <X className={textInputTrailingIconSizeVariant({ density: densityKey })} />
        </button>
      )}
      {invalid && (
        <InvalidIcon
          message={invalid}
          className={textInputTrailingInvalidSlotClasses(false, density)}
          iconClassName={textInputTrailingIconSizeVariant({ density: densityKey })}
        />
      )}
    </>
  );

  return (
    <div
      className={dateInputFieldShellClasses({
        focused,
        invalid,
        disabled,
      })}
    >
      {hasPrefix && (
        <div className={dateInputAffixCellClasses("prefix", density, prefixContent)}>
          {prefixContent}
        </div>
      )}
      <div className={textInputAffixControlColumnClasses(density)}>{children}</div>
      {hasSuffix && (
        <div
          className={dateInputAffixCellClasses("suffix", density, suffixContent, {
            showTrailing: trailingBesideSuffix && showTrailing,
          })}
        >
          {trailingBesideSuffix && showTrailing && trailingCluster()}
          {trailingBesideSuffix && showTrailing ? (
            <span className={textInputSuffixGlyphSlotClasses(density)}>{suffixContent}</span>
          ) : (
            suffixContent
          )}
        </div>
      )}
      {trailingInAffixCell && (
        <div
          className={cn(
            textInputAffixCellClasses("suffix", density),
            dateAffixCellChrome,
            trailingControlCount > 1 && textInputSuffixWithTrailingClusterClasses(density),
          )}
        >
          {trailingCluster()}
        </div>
      )}
    </div>
  );
}
