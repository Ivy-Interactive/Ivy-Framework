import { useEventHandler } from "@/components/event-handler";
import { InvalidIcon } from "@/components/InvalidIcon";
import { inputStyles } from "@/lib/styles";
import { Input } from "@/components/ui/input";
import { X, Check } from "lucide-react";
import React, { useState } from "react";
import { useOptimisticValue } from "./shared/useOptimisticValue";
import { cn } from "@/lib/utils";
import {
  enumColorsToCssVar,
  convertToHex,
  getDisplayColor,
  parseHexAlpha,
  combineHexAlpha,
} from "./color-utils";
import {
  colorInputRowMinHeightVariant,
  colorInputVariant,
  colorInputPickerVariant,
} from "@/components/ui/input/color-input-variant";
import { Densities } from "@/types/density";
import {
  normalizeInputDensity,
  textInputAffixCellClasses,
  textInputAffixInvalidIconClasses,
  textInputEmbeddedInputClasses,
  textInputFieldShellClasses,
  textInputSizeVariant,
  textInputSuffixGlyphSlotClasses,
  textInputSuffixWithTrailingClusterClasses,
  textInputTrailingIconButtonClasses,
  textInputTrailingIconSizeVariant,
  textInputTrailingInvalidSlotClasses,
  textInputTrailingOverlayClasses,
} from "@/components/ui/input/text-input-variant";
import { EMPTY_ARRAY } from "@/lib/constants";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";

interface ColorInputWidgetProps {
  id: string;
  value: string | null;

  disabled?: boolean;
  invalid?: string;
  placeholder?: string;
  nullable?: boolean;
  events?: string[];
  variant?: "Text" | "Picker" | "TextAndPicker" | "Swatch" | "SwatchPicker";
  density?: Densities;
  foreground?: boolean;
  ghost?: boolean;
  allowAlpha?: boolean;
  autoFocus?: boolean;
  slots?: { Prefix?: React.ReactNode[]; Suffix?: React.ReactNode[] };
}

interface ColorSwatchGridProps {
  selectedColor: string | null;
  onColorSelect: (colorName: string) => void;
  disabled?: boolean;
}

const ColorSwatchGrid: React.FC<ColorSwatchGridProps> = ({
  selectedColor,
  onColorSelect,
  disabled = false,
}) => {
  const colorNames = Object.keys(enumColorsToCssVar);
  const normalizedSelected = selectedColor?.toLowerCase();

  return (
    <div className="grid grid-cols-6 gap-1 p-1">
      {colorNames.map((colorName) => {
        const isSelected = normalizedSelected === colorName;
        const cssVar = enumColorsToCssVar[colorName];

        return (
          <button
            key={colorName}
            type="button"
            disabled={disabled}
            onClick={() => onColorSelect(colorName)}
            className={cn(
              "size-6 rounded-full border-2 transition-all flex items-center justify-center",
              "hover:scale-110 hover:z-10",
              isSelected ? "border-foreground ring-2 ring-foreground/30" : "border-transparent",
              disabled ? "opacity-50 cursor-not-allowed" : "cursor-pointer",
            )}
            style={{ backgroundColor: cssVar }}
            title={colorName}
            aria-label={colorName}
          >
            {isSelected && (
              <Check
                className={cn(
                  "size-4",
                  ["white", "yellow", "lime", "amber", "cyan"].includes(colorName)
                    ? "text-black"
                    : "text-white",
                )}
              />
            )}
          </button>
        );
      })}
    </div>
  );
};

interface AlphaSliderProps {
  color: string;
  alpha: number;
  onChange: (alpha: number) => void;
  disabled?: boolean;
  density?: Densities;
}

const AlphaSlider: React.FC<AlphaSliderProps> = ({
  color,
  alpha,
  onChange,
  disabled = false,
  density = Densities.Medium,
}) => {
  const [localAlpha, setLocalAlpha] = useState<number | null>(null);
  if (localAlpha !== null && alpha === localAlpha) {
    setLocalAlpha(null);
  }
  const displayAlpha = localAlpha ?? alpha;
  const height = density === Densities.Small ? 24 : density === Densities.Large ? 36 : 30;
  const percentage = Math.round((displayAlpha / 255) * 100);

  const gradientStyle: React.CSSProperties = {
    background: `linear-gradient(to right, transparent, ${color})`,
  };

  const handleInput = (e: React.ChangeEvent<HTMLInputElement>) => {
    setLocalAlpha(Number(e.target.value));
  };

  const handleCommit = () => {
    if (localAlpha !== null) {
      onChange(localAlpha);
    }
  };

  return (
    <div className="flex items-center gap-1.5">
      <div
        className={cn(
          "relative rounded-md overflow-hidden border border-input",
          disabled && "opacity-50 cursor-not-allowed",
        )}
        style={{ width: 100, height }}
      >
        <div
          className="absolute inset-0"
          style={{
            backgroundImage:
              "repeating-conic-gradient(hsl(var(--muted)) 0% 25%, transparent 0% 50%)",
            backgroundSize: "12px 12px",
          }}
        />
        <div className="absolute inset-0" style={gradientStyle} />
        <input
          type="range"
          min={0}
          max={255}
          value={displayAlpha}
          disabled={disabled}
          onChange={handleInput}
          onPointerUp={handleCommit}
          onKeyUp={handleCommit}
          className="absolute inset-0 w-full h-full opacity-0 cursor-pointer disabled:cursor-not-allowed"
          aria-label={`Opacity: ${percentage}%`}
          title={`${percentage}%`}
        />
        <div
          className="absolute top-0 bottom-0 w-1 bg-white border border-foreground/40 rounded-sm pointer-events-none"
          style={{ left: `calc(${(displayAlpha / 255) * 100}% - 2px)` }}
        />
      </div>
      <span className="text-xs text-muted-foreground w-8 text-right tabular-nums">
        {percentage}%
      </span>
    </div>
  );
};

interface CustomColorPickerProps {
  density: Densities;
  disabled: boolean;
  invalid?: string;
  displayColor: string;
  actualColor: string;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onBlur?: (e: React.FocusEvent<HTMLInputElement>) => void;
  onFocus?: (e: React.FocusEvent<HTMLInputElement>) => void;
}

interface ColorInputAffixLayoutProps {
  density: Densities;
  invalid?: string;
  ghost?: boolean;
  disabled?: boolean;
  nullable: boolean;
  hasValue: boolean;
  prefixContent?: React.ReactNode[];
  suffixContent?: React.ReactNode[];
  onClear: () => void;
  children: (ctx: {
    trailingBesideSuffix: boolean;
    showTrailing: boolean;
    showClear: boolean;
    fieldInvalid: string | undefined;
  }) => React.ReactNode;
}

const ColorInputAffixLayout: React.FC<ColorInputAffixLayoutProps> = ({
  density,
  invalid,
  ghost,
  disabled,
  nullable,
  hasValue,
  prefixContent,
  suffixContent,
  onClear,
  children,
}) => {
  const densityKey = normalizeInputDensity(density);
  const hasPrefix = (prefixContent?.length ?? 0) > 0;
  const hasSuffix = (suffixContent?.length ?? 0) > 0;
  const trailingBesideSuffix = hasSuffix;
  const showClear = nullable && hasValue && !disabled;
  const showTrailing = showClear || Boolean(invalid);
  const fieldInvalid = trailingBesideSuffix && invalid ? undefined : invalid;

  return (
    <div className="relative w-full select-none">
      <div
        className={textInputFieldShellClasses({
          invalid,
          disabled,
          ghost,
        })}
      >
        {hasPrefix && (
          <div className={textInputAffixCellClasses("prefix", density)}>{prefixContent}</div>
        )}
        <div
          className={cn(
            "relative flex min-w-0 flex-1 items-stretch overflow-hidden",
            textInputSizeVariant({ density: densityKey }),
            colorInputRowMinHeightVariant({ density: densityKey }),
            "w-auto",
          )}
        >
          <div className="flex min-w-0 flex-1 items-center gap-2 overflow-hidden">
            {children({ trailingBesideSuffix, showTrailing, showClear, fieldInvalid })}
          </div>
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
            {trailingBesideSuffix && showTrailing && (
              <>
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
                {invalid && (
                  <InvalidIcon
                    message={invalid}
                    className={textInputAffixInvalidIconClasses()}
                    iconClassName={textInputTrailingIconSizeVariant({ density: densityKey })}
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

const CustomColorPicker: React.FC<CustomColorPickerProps> = ({
  density,
  disabled,
  invalid,
  displayColor,
  actualColor,
  onChange,
  onBlur,
  onFocus,
}) => (
  <div
    className={cn(
      colorInputPickerVariant({ density }),
      "relative shrink-0 rounded-md overflow-hidden bg-transparent border",
      disabled ? "opacity-50 cursor-not-allowed" : "cursor-pointer",
      invalid ? inputStyles.invalidInput : "border-input shadow-sm",
    )}
  >
    <div
      className="absolute inset-0 pointer-events-none"
      style={{
        backgroundImage: "repeating-conic-gradient(hsl(var(--muted)) 0% 25%, transparent 0% 50%)",
        backgroundSize: "12px 12px",
      }}
    />
    <div
      className="absolute inset-0 pointer-events-none"
      style={{ backgroundColor: actualColor || "transparent" }}
    />
    <input
      type="color"
      value={displayColor}
      onChange={onChange}
      onBlur={onBlur}
      onFocus={onFocus}
      disabled={disabled}
      title="Choose color"
      className="absolute w-[200%] h-[200%] top-[-50%] left-[-50%] opacity-0 cursor-pointer disabled:cursor-not-allowed"
    />
  </div>
);

export const ColorInputWidget: React.FC<ColorInputWidgetProps> = ({
  id,
  value,
  disabled = false,
  invalid,
  placeholder,
  nullable = false,
  events = EMPTY_ARRAY,
  variant = "TextAndPicker",
  density = Densities.Medium,
  ghost = false,
  allowAlpha = false,
  autoFocus,
  slots,
}) => {
  const prefixContent = slots?.Prefix;
  const suffixContent = slots?.Suffix;
  const hasPrefix = (prefixContent?.length ?? 0) > 0;
  const hasSuffix = (suffixContent?.length ?? 0) > 0;
  const hasAffixes = hasPrefix || hasSuffix;

  const eventHandler = useEventHandler();
  const inputRef = React.useRef<HTMLInputElement>(null);
  const hasAutoFocusedRef = React.useRef(false);

  React.useEffect(() => {
    if (autoFocus && !disabled && !hasAutoFocusedRef.current) {
      hasAutoFocusedRef.current = true;
      inputRef.current?.focus();
    }
  }, [autoFocus, disabled]);

  const [localValue, setLocalColorValue] = useOptimisticValue(value, false);
  const [swatchPickerOpen, setSwatchPickerOpen] = useState(false);

  // Use derived state for display and input values
  const displayValue = localValue ?? "";
  const inputValue = localValue ?? "";

  const currentAlpha = displayValue ? parseHexAlpha(convertToHex(displayValue)).alpha : 255;

  const fireColorChange = (newColor: string | null) => {
    setLocalColorValue(newColor);
    if (events.includes("OnChange")) eventHandler("OnChange", id, [newColor]);
  };

  const handleColorChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newRGB = e.target.value;
    if (allowAlpha) {
      fireColorChange(combineHexAlpha(newRGB, currentAlpha));
    } else {
      fireColorChange(newRGB);
    }
  };

  const handleAlphaChange = (newAlpha: number) => {
    const baseColor = getDisplayColor(displayValue);
    fireColorChange(combineHexAlpha(baseColor, newAlpha));
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.value;
    fireColorChange(newValue);
  };

  const handleInputBlur = () => {
    const convertedValue = convertToHex(inputValue);
    if (events.includes("OnBlur")) eventHandler("OnBlur", id, [convertedValue]);
  };

  const handleInputFocus = () => {
    if (events.includes("OnFocus")) eventHandler("OnFocus", id, []);
  };

  const handleInputKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter") {
      handleInputBlur();
    }
  };

  const handleClear = () => {
    fireColorChange(null);
  };

  const hasValue = localValue !== null && localValue !== "";
  const densityKey = normalizeInputDensity(density);
  const placeholderText =
    placeholder || (allowAlpha ? "Enter color (e.g. #FF0000CC)" : "Enter color");

  const renderColorTextField = (
    ctx: {
      trailingBesideSuffix: boolean;
      showTrailing: boolean;
      showClear: boolean;
      fieldInvalid: string | undefined;
    },
    inAffixShell: boolean,
  ) => (
    <div className={cn("relative min-w-0", inAffixShell ? "flex-1" : "w-full flex-1")}>
      <Input
        ref={inputRef}
        type="text"
        value={inputValue}
        onChange={handleInputChange}
        onBlur={handleInputBlur}
        onFocus={handleInputFocus}
        onKeyDown={handleInputKeyDown}
        placeholder={placeholderText}
        disabled={disabled}
        density={density}
        className={cn(
          colorInputVariant({ density }),
          inAffixShell && textInputEmbeddedInputClasses(true, density),
          !inAffixShell &&
            ghost &&
            "border-transparent shadow-none bg-transparent dark:border-transparent dark:bg-transparent",
          ctx.fieldInvalid && inputStyles.invalidInput,
          ctx.trailingBesideSuffix && ctx.showTrailing && "pr-2",
          !ctx.trailingBesideSuffix && ctx.showTrailing && "pr-8",
          !ctx.trailingBesideSuffix && ctx.showClear && invalid && "pr-16",
        )}
      />
      {!ctx.trailingBesideSuffix && ctx.showTrailing && (
        <div className={textInputTrailingOverlayClasses(density)}>
          {ctx.showClear && (
            <button
              type="button"
              tabIndex={-1}
              aria-label="Clear"
              onClick={handleClear}
              className={textInputTrailingIconButtonClasses(true, density)}
            >
              <X className={textInputTrailingIconSizeVariant({ density })} />
            </button>
          )}
          {invalid && (
            <InvalidIcon
              message={invalid}
              className={textInputTrailingInvalidSlotClasses(true, density)}
              iconClassName={textInputTrailingIconSizeVariant({ density: densityKey })}
            />
          )}
        </div>
      )}
    </div>
  );

  const wrapWithAffixes = (
    field: (ctx: {
      trailingBesideSuffix: boolean;
      showTrailing: boolean;
      showClear: boolean;
      fieldInvalid: string | undefined;
    }) => React.ReactNode,
  ) => {
    if (!hasAffixes) {
      const showClearStandalone = nullable && hasValue && !disabled;
      return field({
        trailingBesideSuffix: false,
        showTrailing: showClearStandalone || Boolean(invalid),
        showClear: showClearStandalone,
        fieldInvalid: invalid,
      });
    }
    return (
      <ColorInputAffixLayout
        density={density}
        invalid={invalid}
        ghost={ghost}
        disabled={disabled}
        nullable={nullable}
        hasValue={hasValue}
        prefixContent={prefixContent}
        suffixContent={suffixContent}
        onClear={handleClear}
      >
        {(ctx) => field(ctx)}
      </ColorInputAffixLayout>
    );
  };

  // --- Variant rendering logic ---
  if (variant === "Text") {
    return (
      <div className="flex items-center gap-x-2">
        {wrapWithAffixes((ctx) => renderColorTextField(ctx, hasAffixes))}
        {allowAlpha && (
          <AlphaSlider
            color={getDisplayColor(displayValue)}
            alpha={currentAlpha}
            onChange={handleAlphaChange}
            disabled={disabled}
            density={density}
          />
        )}
      </div>
    );
  }

  if (variant === "Swatch") {
    const handleSwatchSelect = (colorName: string) => {
      fireColorChange(colorName);
    };

    return (
      <div className="flex items-center gap-x-2">
        <ColorSwatchGrid
          selectedColor={localValue}
          onColorSelect={handleSwatchSelect}
          disabled={disabled}
        />
        {invalid && <InvalidIcon message={invalid} />}
      </div>
    );
  }

  if (variant === "SwatchPicker") {
    const handleSwatchSelect = (colorName: string) => {
      fireColorChange(colorName);
      setSwatchPickerOpen(false);
    };

    return (
      <div className="flex items-center gap-x-2">
        <Popover open={swatchPickerOpen} onOpenChange={setSwatchPickerOpen}>
          <PopoverTrigger asChild>
            <button
              type="button"
              disabled={disabled}
              aria-label="Choose color"
              title="Choose color"
              className={cn(
                colorInputPickerVariant({ density }),
                "relative shrink-0 rounded-md overflow-hidden bg-transparent border",
                disabled ? "opacity-50 cursor-not-allowed" : "cursor-pointer",
                invalid ? inputStyles.invalidInput : "border-input shadow-sm",
              )}
            >
              <div
                className="absolute inset-0 pointer-events-none"
                style={{
                  backgroundImage:
                    "repeating-conic-gradient(hsl(var(--muted)) 0% 25%, transparent 0% 50%)",
                  backgroundSize: "12px 12px",
                }}
              />
              <div
                className="absolute inset-0 pointer-events-none"
                style={{ backgroundColor: convertToHex(displayValue) || "transparent" }}
              />
            </button>
          </PopoverTrigger>
          <PopoverContent className="w-auto p-0" align="start">
            <ColorSwatchGrid
              selectedColor={localValue}
              onColorSelect={handleSwatchSelect}
              disabled={disabled}
            />
          </PopoverContent>
        </Popover>
        {invalid && <InvalidIcon message={invalid} />}
      </div>
    );
  }

  if (variant === "Picker") {
    return (
      <div className="flex items-center gap-x-2">
        <CustomColorPicker
          density={density}
          disabled={disabled}
          invalid={invalid}
          displayColor={getDisplayColor(displayValue)}
          actualColor={convertToHex(displayValue)}
          onChange={handleColorChange}
          onBlur={handleInputBlur}
          onFocus={handleInputFocus}
        />
        {allowAlpha && (
          <AlphaSlider
            color={getDisplayColor(displayValue)}
            alpha={currentAlpha}
            onChange={handleAlphaChange}
            disabled={disabled}
            density={density}
          />
        )}
      </div>
    );
  }

  // Default: TextAndPicker
  return (
    <div className="flex items-center gap-x-2">
      {wrapWithAffixes((ctx) => (
        <>
          <CustomColorPicker
            density={density}
            disabled={disabled}
            invalid={ctx.fieldInvalid}
            displayColor={getDisplayColor(displayValue)}
            actualColor={convertToHex(displayValue)}
            onChange={handleColorChange}
            onBlur={handleInputBlur}
            onFocus={handleInputFocus}
          />
          {renderColorTextField(ctx, hasAffixes)}
        </>
      ))}
      {allowAlpha && (
        <AlphaSlider
          color={getDisplayColor(displayValue)}
          alpha={currentAlpha}
          onChange={handleAlphaChange}
          disabled={disabled}
          density={density}
        />
      )}
    </div>
  );
};
