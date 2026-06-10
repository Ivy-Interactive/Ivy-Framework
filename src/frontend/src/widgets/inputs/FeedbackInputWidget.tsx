import { EmojiRating } from "@/components/EmojiRating";
import { useEventHandler } from "@/components/event-handler";
import { InvalidIcon } from "@/components/InvalidIcon";
import { StarRating } from "@/components/StarRating";
import { ThumbsEnum, ThumbsRating } from "@/components/ui/thumbs-rating";
import React, { useCallback, useMemo } from "react";
import { cn } from "@/lib/utils";
import { useOptimisticValue } from "./shared/useOptimisticValue";
import { Densities } from "@/types/density";
import { EMPTY_ARRAY } from "@/lib/constants";
import { boolInputRowMinHeightVariant } from "@/components/ui/input/bool-input-variant";
import {
  normalizeInputDensity,
  textInputAffixCellClasses,
  textInputAffixInvalidIconClasses,
  textInputFieldShellClasses,
  textInputSizeVariant,
  textInputSuffixGlyphSlotClasses,
  textInputSuffixWithTrailingClusterClasses,
  textInputTrailingIconSizeVariant,
} from "@/components/ui/input/text-input-variant";

interface FeedbackInputWidgetProps {
  id: string;
  value: number | boolean | null;
  variant: "Thumbs" | "Emojis" | "Stars";
  disabled: boolean;
  invalid?: string;
  events: string[];
  nullable?: boolean;
  allowHalf?: boolean;
  max?: number;
  density?: Densities;
  slots?: { Prefix?: React.ReactNode[]; Suffix?: React.ReactNode[] };
}

export const FeedbackInputWidget: React.FC<FeedbackInputWidgetProps> = ({
  id,
  value = null,
  variant = "Stars",
  disabled = false,
  invalid,
  events = EMPTY_ARRAY,
  nullable = false,
  allowHalf = false,
  max = 5,
  density = Densities.Medium,
  slots,
}) => {
  const eventHandler = useEventHandler();

  type FeedbackValue = number | boolean | null;

  const [localValue, setLocalValue] = useOptimisticValue<FeedbackValue>(value, false);

  const prefixContent = slots?.Prefix;
  const suffixContent = slots?.Suffix;
  const hasPrefix = (prefixContent?.length ?? 0) > 0;
  const hasSuffix = (suffixContent?.length ?? 0) > 0;
  const hasAffixes = hasPrefix || hasSuffix;
  const trailingBesideSuffix = hasSuffix;
  const showTrailing = Boolean(invalid);
  const controlInvalid = trailingBesideSuffix && showTrailing ? undefined : invalid;
  const densityKey = normalizeInputDensity(density);

  const isBooleanType = useMemo(() => {
    if (variant === "Thumbs" && nullable) return true;
    return typeof value === "boolean";
  }, [value, variant, nullable]);

  const numericValue = useMemo(() => {
    if (localValue === null || localValue === undefined) return ThumbsEnum.None;
    if (isBooleanType) {
      if (variant === "Thumbs") {
        if (nullable) {
          return localValue ? ThumbsEnum.Up : ThumbsEnum.Down;
        }
        return localValue ? ThumbsEnum.Up : ThumbsEnum.Down;
      }
      return localValue ? 1 : 0;
    }
    return localValue as number;
  }, [localValue, variant, isBooleanType, nullable]);

  const handleChange = useCallback(
    (e: number) => {
      if (!events.includes("OnChange")) return;
      if (disabled) return;

      let convertedValue: number | boolean | null = null;

      if (!isBooleanType) {
        convertedValue = e === ThumbsEnum.None && nullable ? null : e;
        setLocalValue(convertedValue);
        eventHandler("OnChange", id, [convertedValue]);
        return;
      }

      if (variant !== "Thumbs") {
        convertedValue = e === 1;
        setLocalValue(convertedValue);
        eventHandler("OnChange", id, [convertedValue]);
        return;
      }

      if (nullable) {
        if (e === ThumbsEnum.None) convertedValue = null;
        else if (e === ThumbsEnum.Down) convertedValue = false;
        else convertedValue = true;

        setLocalValue(convertedValue);
        eventHandler("OnChange", id, [convertedValue]);
        return;
      }

      if (e === ThumbsEnum.None || e === numericValue) {
        convertedValue = !localValue;
      } else {
        convertedValue = e === ThumbsEnum.Up;
      }

      setLocalValue(convertedValue);
      eventHandler("OnChange", id, [convertedValue]);
    },
    [
      id,
      disabled,
      localValue,
      variant,
      numericValue,
      events,
      eventHandler,
      nullable,
      isBooleanType,
      setLocalValue,
    ],
  );

  const handleBlur = useCallback(() => {
    if (disabled) return;
    if (events.includes("OnBlur")) eventHandler("OnBlur", id, []);
  }, [disabled, eventHandler, id, events]);

  const handleFocus = useCallback(() => {
    if (disabled) return;
    if (events.includes("OnFocus")) eventHandler("OnFocus", id, []);
  }, [disabled, eventHandler, id, events]);

  const ratingComponent = useMemo(() => {
    if (variant === "Thumbs") {
      return (
        <ThumbsRating
          disabled={disabled}
          value={numericValue}
          onRate={handleChange}
          invalid={controlInvalid}
          density={density}
        />
      );
    }

    if (variant === "Emojis") {
      return (
        <EmojiRating
          disabled={disabled}
          value={numericValue}
          onRate={handleChange}
          invalid={controlInvalid}
          allowHalf={allowHalf}
          totalEmojis={max}
          density={density}
        />
      );
    }

    if (variant === "Stars") {
      return (
        <StarRating
          disabled={disabled}
          value={numericValue}
          onRate={handleChange}
          invalid={controlInvalid}
          allowHalf={allowHalf}
          totalStars={max}
          density={density}
        />
      );
    }
    return null;
  }, [variant, disabled, numericValue, handleChange, controlInvalid, allowHalf, max, density]);

  const feedbackControl = (inAffixShell: boolean) => (
    <div
      onBlur={(e) => {
        if (!e.currentTarget.contains(e.relatedTarget)) {
          handleBlur();
        }
      }}
      onFocus={(e) => {
        if (!e.currentTarget.contains(e.relatedTarget)) {
          handleFocus();
        }
      }}
      tabIndex={disabled ? -1 : 0}
      className={cn(
        "outline-none focus:outline-none focus:ring-1 focus:ring-ring",
        inAffixShell && "min-w-0 w-full overflow-hidden",
        !inAffixShell && "rounded-md p-1",
        disabled && "opacity-50 cursor-not-allowed",
      )}
    >
      {ratingComponent}
    </div>
  );

  if (!hasAffixes) {
    return feedbackControl(false);
  }

  return (
    <div
      className="relative w-full select-none"
      onBlur={(e) => {
        if (!e.currentTarget.contains(e.relatedTarget)) {
          handleBlur();
        }
      }}
      onFocus={(e) => {
        if (!e.currentTarget.contains(e.relatedTarget)) {
          handleFocus();
        }
      }}
    >
      <div
        className={textInputFieldShellClasses({
          invalid,
          disabled,
        })}
      >
        {hasPrefix && (
          <div className={textInputAffixCellClasses("prefix", density)}>{prefixContent}</div>
        )}
        <div
          className={cn(
            "relative flex-1 overflow-hidden",
            trailingBesideSuffix && "min-w-0",
            textInputSizeVariant({ density: densityKey }),
            boolInputRowMinHeightVariant({ density: densityKey }),
            "w-auto",
          )}
        >
          {feedbackControl(true)}
        </div>
        {hasSuffix && (
          <div
            className={cn(
              textInputAffixCellClasses("suffix", density),
              trailingBesideSuffix &&
                showTrailing &&
                textInputSuffixWithTrailingClusterClasses(density),
            )}
          >
            {trailingBesideSuffix && showTrailing && invalid && (
              <InvalidIcon
                message={invalid}
                className={textInputAffixInvalidIconClasses()}
                iconClassName={textInputTrailingIconSizeVariant({ density: densityKey })}
              />
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
