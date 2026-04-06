import { useEventHandler } from "@/components/event-handler";
import { InvalidIcon } from "@/components/InvalidIcon";
import { inputStyles } from "@/lib/styles";
import { Input } from "@/components/ui/input";
import { X, Check } from "lucide-react";
import React, { useMemo, useState } from "react";
import { useOptimisticValue } from "./shared/useOptimisticValue";
import { cn } from "@/lib/utils";
import { enumColorsToCssVar, convertToHex, getDisplayColor, parseHexAlpha, combineHexAlpha } from "./color-utils";
import {
  colorInputVariant,
  colorInputPickerVariant,
} from "@/components/ui/input/color-input-variant";
import { Densities } from "@/types/density";
import { xIconVariant } from "@/components/ui/input/text-input-variant";
import { EMPTY_ARRAY } from "@/lib/constants";

interface ColorInputWidgetProps {
  id: string;
  value: string | null;

  disabled?: boolean;
  invalid?: string;
  placeholder?: string;
  nullable?: boolean;
  events?: string[];
  variant?: "Text" | "Picker" | "TextAndPicker" | "Swatch";
  density?: Densities;
  foreground?: boolean;
  ghost?: boolean;
  allowAlpha?: boolean;
  autoFocus?: boolean;
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
              "w-6 h-6 rounded-full border-2 transition-all flex items-center justify-center",
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
                  "w-4 h-4",
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

  const gradientStyle: React.CSSProperties = useMemo(
    () => ({
      background: `linear-gradient(to right, transparent, ${color})`,
    }),
    [color],
  );

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
}) => {
  const eventHandler = useEventHandler();

  const [localValue, setLocalColorValue] = useOptimisticValue(value, false);

  // Use derived state for display and input values
  const displayValue = localValue ?? "";
  const inputValue = localValue ?? "";

  const currentAlpha = displayValue ? parseHexAlpha(convertToHex(displayValue)).alpha : 255;

  const fireColorChange = (newColor: string | null) => {
    setLocalColorValue(newColor);
    eventHandler("OnChange", id, [newColor]);
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

  // --- Variant rendering logic ---
  if (variant === "Text") {
    return (
      <div className="flex items-center space-x-2">
        <div className="relative">
          <Input
            type="text"
            value={inputValue}
            onChange={handleInputChange}
            onBlur={handleInputBlur}
            onFocus={handleInputFocus}
            onKeyDown={handleInputKeyDown}
            autoFocus={autoFocus}
            placeholder={
              placeholder || (allowAlpha ? "Enter color (e.g. #FF0000CC)" : "Enter color")
            }
            disabled={disabled}
            className={cn(
              colorInputVariant({ density }),
              ghost &&
                "border-transparent shadow-none bg-transparent dark:border-transparent dark:bg-transparent",
              invalid && inputStyles.invalidInput,
              (invalid || (nullable && localValue !== null && !disabled)) && "pr-8",
            )}
          />
          {(invalid || (nullable && localValue !== null && !disabled)) && (
            <div
              className="absolute top-1/2 -translate-y-1/2 flex items-center gap-1 right-2"
              style={{ zIndex: 2 }}
            >
              {invalid && (
                <span className="flex items-center">
                  <InvalidIcon message={invalid} />
                </span>
              )}
              {nullable && localValue !== null && !disabled && (
                <button
                  type="button"
                  tabIndex={-1}
                  aria-label="Clear"
                  onClick={handleClear}
                  className="p-1 rounded hover:bg-accent focus:outline-none cursor-pointer"
                >
                  <X className="h-4 w-4 text-muted-foreground hover:text-foreground" />
                </button>
              )}
            </div>
          )}
        </div>
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
      <div className="flex items-center space-x-2">
        <ColorSwatchGrid
          selectedColor={localValue}
          onColorSelect={handleSwatchSelect}
          disabled={disabled}
        />
        {invalid && <InvalidIcon message={invalid} />}
      </div>
    );
  }

  if (variant === "Picker") {
    return (
      <div className="flex items-center space-x-2">
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
    <div className="flex items-center space-x-2">
      <CustomColorPicker
        density={density}
        disabled={disabled}
        invalid={invalid}
        displayColor={getDisplayColor(displayValue)}
        actualColor={convertToHex(displayValue)}
        onChange={handleColorChange}
      />
      <div className="relative">
        <Input
          type="text"
          value={inputValue}
          onChange={handleInputChange}
          onBlur={handleInputBlur}
          onFocus={handleInputFocus}
          onKeyDown={handleInputKeyDown}
          autoFocus={autoFocus}
          placeholder={placeholder || (allowAlpha ? "Enter color (e.g. #FF0000CC)" : "Enter color")}
          disabled={disabled}
          className={cn(
            colorInputVariant({ density }),
            ghost &&
              "border-transparent shadow-none bg-transparent dark:border-transparent dark:bg-transparent",
            invalid && inputStyles.invalidInput,
            (invalid || (nullable && localValue !== null && !disabled)) && "pr-8",
          )}
        />
        {(invalid || (nullable && localValue !== null && !disabled)) && (
          <div
            className="absolute top-1/2 -translate-y-1/2 flex items-center gap-1 right-2"
            style={{ zIndex: 2 }}
          >
            {/* Invalid icon - rightmost */}
            {invalid && <InvalidIcon message={invalid} className="pointer-events-auto" />}
            {nullable && localValue !== null && !disabled && (
              <button
                type="button"
                tabIndex={-1}
                aria-label="Clear"
                onClick={handleClear}
                className="p-1 rounded hover:bg-accent focus:outline-none cursor-pointer"
              >
                <X className={xIconVariant({ density })} />
              </button>
            )}
          </div>
        )}
      </div>
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
