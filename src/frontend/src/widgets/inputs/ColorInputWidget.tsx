import { useEventHandler } from '@/components/event-handler';
import { InvalidIcon } from '@/components/InvalidIcon';
import { inputStyles } from '@/lib/styles';
import { Input } from '@/components/ui/input';
import { X, Check } from 'lucide-react';
import React, { useMemo, useState } from 'react';
import { logger } from '@/lib/logger';
import { cn } from '@/lib/utils';
import {
  colorInputVariant,
  colorInputPickerVariant,
} from '@/components/ui/input/color-input-variant';
import { Scales } from '@/types/scale';
import { xIconVariant } from '@/components/ui/input/text-input-variant';

const EMPTY_ARRAY: never[] = [];

interface ColorInputWidgetProps {
  id: string;
  value: string | null;

  disabled?: boolean;
  invalid?: string;
  placeholder?: string;
  nullable?: boolean;
  events?: string[];
  variant?: 'Text' | 'Picker' | 'TextAndPicker' | 'Swatch';
  scale?: Scales;
  foreground?: boolean;
  ghost?: boolean;
  allowAlpha?: boolean;
}

// Hoisted color map for backend Colors enum
const enumColorsToCssVar: Record<string, string> = {
  black: 'var(--color-black)',
  white: 'var(--color-white)',
  slate: 'var(--color-slate)',
  gray: 'var(--color-gray)',
  zinc: 'var(--color-zinc)',
  neutral: 'var(--color-neutral)',
  stone: 'var(--color-stone)',
  red: 'var(--color-red)',
  orange: 'var(--color-orange)',
  amber: 'var(--color-amber)',
  yellow: 'var(--color-yellow)',
  lime: 'var(--color-lime)',
  green: 'var(--color-green)',
  emerald: 'var(--color-emerald)',
  teal: 'var(--color-teal)',
  cyan: 'var(--color-cyan)',
  sky: 'var(--color-sky)',
  blue: 'var(--color-blue)',
  indigo: 'var(--color-indigo)',
  violet: 'var(--color-violet)',
  purple: 'var(--color-purple)',
  fuchsia: 'var(--color-fuchsia)',
  pink: 'var(--color-pink)',
  rose: 'var(--color-rose)',
  primary: 'var(--color-primary)',
  secondary: 'var(--color-secondary)',
  destructive: 'var(--color-destructive)',
  success: 'var(--color-success)',
  warning: 'var(--color-warning)',
  info: 'var(--color-info)',
  muted: 'var(--color-muted)',
};

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
      {colorNames.map(colorName => {
        const isSelected = normalizedSelected === colorName;
        const cssVar = enumColorsToCssVar[colorName];

        return (
          <button
            key={colorName}
            type="button"
            disabled={disabled}
            onClick={() => onColorSelect(colorName)}
            className={cn(
              'w-6 h-6 rounded-full border-2 transition-all flex items-center justify-center',
              'hover:scale-110 hover:z-10',
              isSelected
                ? 'border-foreground ring-2 ring-foreground/30'
                : 'border-transparent',
              disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'
            )}
            style={{ backgroundColor: cssVar }}
            title={colorName}
            aria-label={colorName}
          >
            {isSelected && (
              <Check
                className={cn(
                  'w-4 h-4',
                  ['white', 'yellow', 'lime', 'amber', 'cyan'].includes(
                    colorName
                  )
                    ? 'text-black'
                    : 'text-white'
                )}
              />
            )}
          </button>
        );
      })}
    </div>
  );
};

function parseHexAlpha(hex: string): { rgb: string; alpha: number } {
  if (!hex || !hex.startsWith('#'))
    return { rgb: hex || '#000000', alpha: 255 };
  const clean = hex.slice(1);
  if (clean.length === 8) {
    return {
      rgb: '#' + clean.slice(0, 6),
      alpha: parseInt(clean.slice(6, 8), 16),
    };
  }
  return { rgb: hex.length === 7 ? hex : '#000000', alpha: 255 };
}

function combineHexAlpha(rgb: string, alpha: number): string {
  const base = rgb.startsWith('#') ? rgb : '#' + rgb;
  const hex6 = base.length === 7 ? base : '#000000';
  if (alpha >= 255) return hex6; // fully opaque → keep 6-char hex
  const aa = Math.max(0, Math.min(255, alpha)).toString(16).padStart(2, '0');
  return hex6 + aa;
}

interface AlphaSliderProps {
  color: string;
  alpha: number;
  onChange: (alpha: number) => void;
  disabled?: boolean;
  scale?: Scales;
}

const AlphaSlider: React.FC<AlphaSliderProps> = ({
  color,
  alpha,
  onChange,
  disabled = false,
  scale = Scales.Medium,
}) => {
  const [localAlpha, setLocalAlpha] = useState<number | null>(null);
  if (localAlpha !== null && alpha === localAlpha) {
    setLocalAlpha(null);
  }
  const displayAlpha = localAlpha ?? alpha;
  const height = scale === Scales.Small ? 24 : scale === Scales.Large ? 36 : 30;
  const percentage = Math.round((displayAlpha / 255) * 100);

  const gradientStyle: React.CSSProperties = useMemo(
    () => ({
      background: `linear-gradient(to right, transparent, ${color})`,
    }),
    [color]
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
          'relative rounded-md overflow-hidden border border-input',
          disabled && 'opacity-50 cursor-not-allowed'
        )}
        style={{ width: 100, height }}
      >
        <div
          className="absolute inset-0"
          style={{
            backgroundImage:
              'repeating-conic-gradient(hsl(var(--muted)) 0% 25%, transparent 0% 50%)',
            backgroundSize: '12px 12px',
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
  scale: Scales;
  disabled: boolean;
  invalid?: string;
  displayColor: string;
  actualColor: string;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
}

const CustomColorPicker: React.FC<CustomColorPickerProps> = ({
  scale,
  disabled,
  invalid,
  displayColor,
  actualColor,
  onChange,
}) => (
  <div
    className={cn(
      colorInputPickerVariant({ scale }),
      'relative shrink-0 rounded-md overflow-hidden bg-transparent border',
      disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer',
      invalid ? inputStyles.invalidInput : 'border-input shadow-sm'
    )}
  >
    <div
      className="absolute inset-0 pointer-events-none"
      style={{
        backgroundImage:
          'repeating-conic-gradient(hsl(var(--muted)) 0% 25%, transparent 0% 50%)',
        backgroundSize: '12px 12px',
      }}
    />
    <div
      className="absolute inset-0 pointer-events-none"
      style={{ backgroundColor: actualColor || 'transparent' }}
    />
    <input
      type="color"
      value={displayColor}
      onChange={onChange}
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
  variant = 'TextAndPicker',
  scale = Scales.Medium,
  ghost = false,
  allowAlpha = false,
}) => {
  const eventHandler = useEventHandler();
  // Use derived state for display and input values
  const displayValue = value ?? '';
  const inputValue = value ?? '';

  const getThemeColorHex = (cssVar: string): string | undefined => {
    if (typeof window === 'undefined') return undefined;
    const value = getComputedStyle(document.documentElement)
      .getPropertyValue(cssVar)
      .trim();
    if (/^#[0-9a-fA-F]{6}$/.test(value)) return value;
    return undefined;
  };

  /**
   * Converts various color formats to hex.
   * Supported formats: hex (#rrggbb / #rrggbbaa), rgb(), rgba(), named colors
   * Unsupported formats: oklch() - returns fallback color (#000000)
   */
  const convertToHex = (colorValue: string): string => {
    if (!colorValue) return '';
    if (colorValue.startsWith('#')) {
      return colorValue;
    }
    const rgbaMatch = colorValue.match(
      /rgba\((\d+),\s*(\d+),\s*(\d+),\s*([\d.]+)\)/
    );
    if (rgbaMatch) {
      const r = parseInt(rgbaMatch[1]);
      const g = parseInt(rgbaMatch[2]);
      const b = parseInt(rgbaMatch[3]);
      const a = Math.round(parseFloat(rgbaMatch[4]) * 255);
      const hex = `#${r.toString(16).padStart(2, '0')}${g.toString(16).padStart(2, '0')}${b.toString(16).padStart(2, '0')}`;
      if (a < 255) return hex + a.toString(16).padStart(2, '0');
      return hex;
    }
    const rgbMatch = colorValue.match(/rgb\((\d+),\s*(\d+),\s*(\d+)\)/);
    if (rgbMatch) {
      const r = parseInt(rgbMatch[1]);
      const g = parseInt(rgbMatch[2]);
      const b = parseInt(rgbMatch[3]);
      return `#${r.toString(16).padStart(2, '0')}${g.toString(16).padStart(2, '0')}${b.toString(16).padStart(2, '0')}`;
    }
    const hslMatch = colorValue.match(
      /hsla?\((\d+),\s*(\d+)%?,\s*(\d+)%?(?:,\s*[\d.]+)?\)/
    );
    if (hslMatch) {
      const h = parseInt(hslMatch[1]) / 360;
      const s = parseInt(hslMatch[2]) / 100;
      const l = parseInt(hslMatch[3]) / 100;
      let r, g, b;
      if (s === 0) {
        r = g = b = l; // achromatic
      } else {
        const hue2rgb = (p: number, q: number, t: number) => {
          if (t < 0) t += 1;
          if (t > 1) t -= 1;
          if (t < 1 / 6) return p + (q - p) * 6 * t;
          if (t < 1 / 2) return q;
          if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
          return p;
        };
        const q = l < 0.5 ? l * (1 + s) : l + s - l * s;
        const p = 2 * l - q;
        r = hue2rgb(p, q, h + 1 / 3);
        g = hue2rgb(p, q, h);
        b = hue2rgb(p, q, h - 1 / 3);
      }
      const toHex = (x: number) => {
        const hex = Math.round(x * 255).toString(16);
        return hex.length === 1 ? '0' + hex : hex;
      };
      return `#${toHex(r)}${toHex(g)}${toHex(b)}`;
    }
    // More comprehensive OKLCH detection
    const isOklch = /^oklch\s*\(/i.test(colorValue.trim());
    if (isOklch) {
      logger.warn(`OKLCH color format not supported: ${colorValue}`);
      return '#000000'; // Default fallback
    }
    // Use theme color if available
    const lowerValue = colorValue.toLowerCase();
    if (enumColorsToCssVar[lowerValue]) {
      const cssVar = enumColorsToCssVar[lowerValue]
        .replace('var(', '')
        .replace(')', '');
      const themeHex = getThemeColorHex(cssVar);
      if (themeHex) return themeHex;
    }
    return colorValue;
  };

  const getDisplayColor = (): string => {
    if (!displayValue) return '#000000';
    const hexValue = convertToHex(displayValue);
    if (hexValue.startsWith('var(')) return '#000000';
    if (hexValue.startsWith('#') && hexValue.length === 9) {
      return hexValue.slice(0, 7);
    }
    return hexValue.startsWith('#') ? hexValue : '#000000';
  };

  const currentAlpha = displayValue
    ? parseHexAlpha(convertToHex(displayValue)).alpha
    : 255;

  const handleColorChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newRGB = e.target.value;
    if (allowAlpha) {
      eventHandler('OnChange', id, [combineHexAlpha(newRGB, currentAlpha)]);
    } else {
      eventHandler('OnChange', id, [newRGB]);
    }
  };

  const handleAlphaChange = (newAlpha: number) => {
    const baseColor = getDisplayColor();
    eventHandler('OnChange', id, [combineHexAlpha(baseColor, newAlpha)]);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.value;
    eventHandler('OnChange', id, [newValue]);
  };

  const handleInputBlur = () => {
    const convertedValue = convertToHex(inputValue);
    eventHandler('OnChange', id, [convertedValue]);
    if (events.includes('OnBlur')) eventHandler('OnBlur', id, [convertedValue]);
  };

  const handleInputKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleInputBlur();
    }
  };

  const handleClear = () => {
    eventHandler('OnChange', id, [null]);
  };

  // --- Variant rendering logic ---
  if (variant === 'Text') {
    return (
      <div className="flex items-center space-x-2">
        <div className="relative">
          <Input
            type="text"
            value={inputValue}
            onChange={handleInputChange}
            onBlur={handleInputBlur}
            onKeyDown={handleInputKeyDown}
            placeholder={
              placeholder ||
              (allowAlpha ? 'Enter color (e.g. #FF0000CC)' : 'Enter color')
            }
            disabled={disabled}
            className={cn(
              colorInputVariant({ scale }),
              ghost &&
                'border-transparent shadow-none bg-transparent dark:border-transparent dark:bg-transparent',
              invalid && inputStyles.invalidInput,
              (invalid || (nullable && value !== null && !disabled)) && 'pr-8'
            )}
          />
          {(invalid || (nullable && value !== null && !disabled)) && (
            <div
              className="absolute top-1/2 -translate-y-1/2 flex items-center gap-1 right-2"
              style={{ zIndex: 2 }}
            >
              {invalid && (
                <span className="flex items-center">
                  <InvalidIcon message={invalid} />
                </span>
              )}
              {nullable && value !== null && !disabled && (
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
            color={getDisplayColor()}
            alpha={currentAlpha}
            onChange={handleAlphaChange}
            disabled={disabled}
            scale={scale}
          />
        )}
      </div>
    );
  }

  if (variant === 'Swatch') {
    const handleSwatchSelect = (colorName: string) => {
      eventHandler('OnChange', id, [colorName]);
    };

    return (
      <div className="flex items-center space-x-2">
        <ColorSwatchGrid
          selectedColor={value}
          onColorSelect={handleSwatchSelect}
          disabled={disabled}
        />
        {invalid && <InvalidIcon message={invalid} />}
      </div>
    );
  }

  if (variant === 'Picker') {
    return (
      <div className="flex items-center space-x-2">
        <CustomColorPicker
          scale={scale}
          disabled={disabled}
          invalid={invalid}
          displayColor={getDisplayColor()}
          actualColor={convertToHex(displayValue)}
          onChange={handleColorChange}
        />
        {allowAlpha && (
          <AlphaSlider
            color={getDisplayColor()}
            alpha={currentAlpha}
            onChange={handleAlphaChange}
            disabled={disabled}
            scale={scale}
          />
        )}
      </div>
    );
  }

  // Default: TextAndPicker
  return (
    <div className="flex items-center space-x-2">
      <CustomColorPicker
        scale={scale}
        disabled={disabled}
        invalid={invalid}
        displayColor={getDisplayColor()}
        actualColor={convertToHex(displayValue)}
        onChange={handleColorChange}
      />
      <div className="relative">
        <Input
          type="text"
          value={inputValue}
          onChange={handleInputChange}
          onBlur={handleInputBlur}
          onKeyDown={handleInputKeyDown}
          placeholder={
            placeholder ||
            (allowAlpha ? 'Enter color (e.g. #FF0000CC)' : 'Enter color')
          }
          disabled={disabled}
          className={cn(
            colorInputVariant({ scale }),
            ghost &&
              'border-transparent shadow-none bg-transparent dark:border-transparent dark:bg-transparent',
            invalid && inputStyles.invalidInput,
            (invalid || (nullable && value !== null && !disabled)) && 'pr-8'
          )}
        />
        {(invalid || (nullable && value !== null && !disabled)) && (
          <div
            className="absolute top-1/2 -translate-y-1/2 flex items-center gap-1 right-2"
            style={{ zIndex: 2 }}
          >
            {/* Invalid icon - rightmost */}
            {invalid && (
              <InvalidIcon message={invalid} className="pointer-events-auto" />
            )}
            {nullable && value !== null && !disabled && (
              <button
                type="button"
                tabIndex={-1}
                aria-label="Clear"
                onClick={handleClear}
                className="p-1 rounded hover:bg-accent focus:outline-none cursor-pointer"
              >
                <X className={xIconVariant({ scale })} />
              </button>
            )}
          </div>
        )}
      </div>
      {allowAlpha && (
        <AlphaSlider
          color={getDisplayColor()}
          alpha={currentAlpha}
          onChange={handleAlphaChange}
          disabled={disabled}
          scale={scale}
        />
      )}
    </div>
  );
};
