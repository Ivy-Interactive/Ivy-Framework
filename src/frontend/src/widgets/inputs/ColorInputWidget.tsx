import { useEventHandler } from '@/components/event-handler';
import { InvalidIcon } from '@/components/InvalidIcon';
import { inputStyles } from '@/lib/styles';
import { Input } from '@/components/ui/input';
import { X, Check } from 'lucide-react';
import React from 'react';
import { logger } from '@/lib/logger';
import { cn } from '@/lib/utils';
import {
  colorInputVariants,
  colorInputPickerVariants,
} from '@/components/ui/input/color-input-variants';
import { Scales } from '@/types/scale';
import { xIconVariants } from '@/components/ui/input/text-input-variants';
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

export const ColorInputWidget: React.FC<ColorInputWidgetProps> = ({
  id,
  value,
  disabled = false,
  invalid,
  placeholder,
  nullable = false,
  events = [],
  variant = 'TextAndPicker',
  scale = Scales.Medium,
}) => {
  const eventHandler = useEventHandler();
  // Use derived state for display and input values
  const displayValue = value ?? '';
  const inputValue = value ?? '';

  const handleColorChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.value;
    eventHandler('OnChange', id, [newValue]);
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
   * Supported formats: hex (#rrggbb), rgb(), named colors
   * Unsupported formats: oklch() - returns fallback color (#000000)
   */
  const convertToHex = (colorValue: string): string => {
    if (!colorValue) return '';
    if (colorValue.startsWith('#')) {
      return colorValue;
    }
    const rgbMatch = colorValue.match(/rgb\((\d+),\s*(\d+),\s*(\d+)\)/);
    if (rgbMatch) {
      const r = parseInt(rgbMatch[1]);
      const g = parseInt(rgbMatch[2]);
      const b = parseInt(rgbMatch[3]);
      return `#${r.toString(16).padStart(2, '0')}${g.toString(16).padStart(2, '0')}${b.toString(16).padStart(2, '0')}`;
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
    return hexValue.startsWith('#') ? hexValue : '#000000';
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
            placeholder={placeholder || 'Enter color'}
            disabled={disabled}
            className={cn(
              colorInputVariants({ scale }),
              'border-none shadow-none focus-visible:ring-0',
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
        <div className="relative">
          <input
            type="color"
            value={getDisplayColor()}
            onChange={handleColorChange}
            disabled={disabled}
            className={cn(
              colorInputPickerVariants({ scale }),
              'p-0 rounded-md bg-transparent border-none shadow-none focus:outline-none',
              disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer',
              invalid && inputStyles.invalidInput
            )}
          />
        </div>
      </div>
    );
  }

  // Default: TextAndPicker
  return (
    <div className="flex items-center space-x-2">
      <div className="relative">
        <input
          type="color"
          value={getDisplayColor()}
          onChange={handleColorChange}
          disabled={disabled}
          className={cn(
            colorInputPickerVariants({ scale }),
            'p-0 rounded-md bg-transparent border-none shadow-none focus:outline-none',
            disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer',
            invalid && inputStyles.invalidInput
          )}
        />
      </div>
      <div className="relative">
        <Input
          type="text"
          value={inputValue}
          onChange={handleInputChange}
          onBlur={handleInputBlur}
          onKeyDown={handleInputKeyDown}
          placeholder={placeholder || 'Enter color'}
          disabled={disabled}
          className={cn(
            colorInputVariants({ scale }),
            'border-none shadow-none focus-visible:ring-0',
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
                <X className={xIconVariants({ scale })} />
              </button>
            )}
          </div>
        )}
      </div>
    </div>
  );
};
