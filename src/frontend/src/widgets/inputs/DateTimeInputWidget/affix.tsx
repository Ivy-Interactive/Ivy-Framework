import React from "react";
import { X } from "lucide-react";
import { cn } from "@/lib/utils";
import { InvalidIcon } from "@/components/InvalidIcon";
import { Densities } from "@/types/density";
import { boolInputRowMinHeightVariant } from "@/components/ui/input/bool-input-variant";
import {
  normalizeInputDensity,
  textInputAffixCellClasses,
  textInputAffixIconOnlyPaddingVariant,
  textInputEmbeddedContentPaddingClasses,
  textInputSuffixGlyphSlotClasses,
  textInputSuffixWithTrailingClusterClasses,
  textInputTrailingIconButtonClasses,
  textInputTrailingIconSizeVariant,
  textInputTrailingInvalidSlotClasses,
} from "@/components/ui/input/text-input-variant";

/** Affix strip aligned with number/select/icon inputs. */
export function dateInputAffixCellClasses(
  side: "prefix" | "suffix",
  density: Densities,
  densityKey: ReturnType<typeof normalizeInputDensity>,
  options: { withTrailingCluster: boolean; iconOnlyPadding: boolean },
): string {
  return cn(
    textInputAffixCellClasses(side, density),
    "relative z-10 shrink-0 overflow-visible",
    options.withTrailingCluster && textInputSuffixWithTrailingClusterClasses(density),
    options.iconOnlyPadding && textInputAffixIconOnlyPaddingVariant({ density: densityKey }),
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
    "relative flex w-full min-w-0 select-none items-stretch rounded-field border bg-transparent shadow-sm transition-colors dark:bg-white/5",
    options.focused
      ? "border-ring outline-none dark:border-ring"
      : "border-input dark:border-white/10",
    options.invalid && "border-destructive",
    options.disabled && "cursor-not-allowed opacity-50",
  );
}

/** Strip inner control chrome so the shell owns border and fill. */
export const dateInputEmbeddedControlClasses =
  "[&_button]:border-0 [&_button]:bg-transparent [&_button]:shadow-none [&_button]:dark:bg-transparent [&_button]:hover:bg-transparent [&_button]:dark:hover:bg-transparent [&_button]:focus-visible:ring-0 [&_button]:focus-visible:ring-offset-0 [&_input]:border-0 [&_input]:bg-transparent [&_input]:shadow-none [&_input]:dark:bg-transparent [&_input]:focus-visible:ring-0";

interface DateInputAffixShellProps {
  density: Densities;
  invalid?: string;
  disabled?: boolean;
  focused?: boolean;
  hasPrefix: boolean;
  hasSuffix: boolean;
  prefixContent?: React.ReactNode;
  suffixContent?: React.ReactNode;
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
      className={cn(
        "relative flex w-full items-center rounded-field border bg-transparent shadow-sm transition-colors dark:bg-white/5",
        focused
          ? "border-ring ring-1 ring-ring dark:border-ring"
          : "border-input dark:border-white/10",
        invalid && "border-destructive",
        disabled && "cursor-not-allowed opacity-50",
      )}
    >
      {hasPrefix && (
        <div
          className={dateInputAffixCellClasses("prefix", density, densityKey, {
            withTrailingCluster: false,
            iconOnlyPadding: true,
          })}
        >
          {prefixContent}
        </div>
      )}
      <div
        className={cn(
          "relative z-0 isolate flex min-w-0 flex-1 items-center",
          boolInputRowMinHeightVariant({ density: densityKey }),
          textInputEmbeddedContentPaddingClasses(density),
        )}
      >
        {children}
      </div>
      {hasSuffix && (
        <div
          className={dateInputAffixCellClasses("suffix", density, densityKey, {
            withTrailingCluster: trailingBesideSuffix && showTrailing,
            iconOnlyPadding: !showTrailing,
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
          className={dateInputAffixCellClasses("suffix", density, densityKey, {
            withTrailingCluster: trailingControlCount > 1,
            iconOnlyPadding: trailingControlCount === 1,
          })}
        >
          {trailingCluster()}
        </div>
      )}
    </div>
  );
}
