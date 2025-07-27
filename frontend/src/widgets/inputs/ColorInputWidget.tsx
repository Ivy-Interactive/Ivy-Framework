import { useEventHandler } from '@/components/EventHandlerContext';
import { InvalidIcon } from '@/components/InvalidIcon';
import { inputStyles } from '@/lib/styles';
import { Input } from '@/components/ui/input';
import { X } from 'lucide-react';
import React, { useState, useEffect } from 'react';

interface ColorInputWidgetProps {
  id: string;
  value: string | null;
  disabled?: boolean;
  invalid?: string;
  placeholder?: string;
  nullable?: boolean;
  events?: string[];
  variant?: 'Text' | 'Picker' | 'TextAndPicker';
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
}) => {
  const eventHandler = useEventHandler();
  const [displayValue, setDisplayValue] = useState(value ?? '');
  const [inputValue, setInputValue] = useState(value ?? '');

  useEffect(() => {
    setDisplayValue(value ?? '');
    setInputValue(value ?? '');
  }, [value]);

  const handleColorChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.value;
    setDisplayValue(newValue);
    setInputValue(newValue);
    eventHandler('OnChange', id, [newValue]);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.value;
    setInputValue(newValue);
  };

  const handleInputBlur = () => {
    const convertedValue = convertToHex(inputValue);
    setDisplayValue(convertedValue);
    setInputValue(convertedValue);
    eventHandler('OnChange', id, [convertedValue]);
    if (events.includes('OnBlur')) eventHandler('OnBlur', id, [convertedValue]);
  };

  const handleInputKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleInputBlur();
    }
  };

  const handleClear = () => {
    setDisplayValue('');
    setInputValue('');
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
    const oklchMatch = colorValue.match(
      /oklch\(([^,]+),\s*([^,]+),\s*([^)]+)\)/
    );
    if (oklchMatch) {
      // TODO: Replace this placeholder with a real OKLCH → HEX conversion.
      const hash = Math.abs(
        colorValue.split('').reduce((a, b) => a + b.charCodeAt(0), 0)
      );
      return `#${(hash % 0xffffff).toString(16).padStart(6, '0')}`;
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
            className={`${invalid ? inputStyles.invalidInput + ' pr-8' : ''}`}
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

  if (variant === 'Picker') {
    return (
      <div className="flex items-center space-x-2">
        <div className="relative">
          <input
            type="color"
            value={getDisplayColor()}
            onChange={handleColorChange}
            disabled={disabled}
            className={`w-10 h-10 p-1 rounded-lg border ${
              disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'
            } ${invalid ? inputStyles.invalidInput : 'border-gray-300'}`}
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
          className={`w-10 h-10 p-1 rounded-lg border ${
            disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'
          } ${invalid ? inputStyles.invalidInput : 'border-gray-300'}`}
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
          className={`${invalid ? inputStyles.invalidInput + ' pr-8' : ''}`}
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
};
