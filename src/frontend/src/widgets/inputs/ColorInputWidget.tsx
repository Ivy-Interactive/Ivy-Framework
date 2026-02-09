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
import { Slider } from '@/components/ui/slider';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from '@/components/ui/tabs';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
interface ColorInputWidgetProps {
  id: string;
  value: string | null;
  disabled?: boolean;
  invalid?: string;
  placeholder?: string;
  nullable?: boolean;
  events?: string[];
  variant?: 'Text' | 'Picker' | 'TextAndPicker' | 'Swatch' | 'ThemePicker';
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

const ThemeColorGrid: React.FC<{
  onSelect: (color: string) => void;
  selectedColor: string | null;
}> = ({ onSelect, selectedColor }) => {
  // Generate 200 colors (10 rows x 20 columns)
  const rows = 10;
  const cols = 20;

  const renderGrid = () => {
    const grid = [];
    for (let r = 0; r < rows; r++) {
      const rowColors = [];
      for (let c = 0; c < cols; c++) {
        // HSL generation
        const hue = Math.floor((c / cols) * 360);
        // Vary lightness: top (0) is light (95%), bottom (9) is dark (5%)
        const lightness = 95 - (r / (rows - 1)) * 90;
        const saturation = 85;

        // Simple HSL to Hex manually to avoid dependencies
        const h = hue;
        const s = saturation;
        const l = lightness;

        const hDecimal = h / 360;
        const sDecimal = s / 100;
        const lDecimal = l / 100;

        let rVal, gVal, bVal;

        const hue2rgb = (p: number, q: number, t: number) => {
          if (t < 0) t += 1;
          if (t > 1) t -= 1;
          if (t < 1 / 6) return p + (q - p) * 6 * t;
          if (t < 1 / 2) return q;
          if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
          return p;
        };

        const q = lDecimal < 0.5 ? lDecimal * (1 + sDecimal) : lDecimal + sDecimal - lDecimal * sDecimal;
        const p = 2 * lDecimal - q;

        rVal = hue2rgb(p, q, hDecimal + 1 / 3);
        gVal = hue2rgb(p, q, hDecimal);
        bVal = hue2rgb(p, q, hDecimal - 1 / 3);


        const toHex = (x: number) => {
          const hex = Math.round(x * 255).toString(16);
          return hex.length === 1 ? '0' + hex : hex;
        };

        const hexColor = `#${toHex(rVal)}${toHex(gVal)}${toHex(bVal)}`;
        const isSelected = selectedColor?.toLowerCase() === hexColor.toLowerCase();

        rowColors.push(
          <button
            key={`${r}-${c}`}
            type="button"
            className={cn(
              "w-5 h-5 rounded-sm hover:scale-125 transition-transform hover:z-10 hover:shadow-sm border border-black/5",
              isSelected && "ring-1 ring-offset-1 ring-black/50 z-20 scale-110"
            )}
            style={{ backgroundColor: hexColor }}
            onClick={() => onSelect(hexColor)}
            title={hexColor}
          />
        );
      }
      grid.push(<div key={r} className="flex gap-px">{rowColors}</div>);
    }
    return grid;
  };

  return (
    <div className="flex flex-col gap-px p-1 bg-background rounded-md shadow-sm">
      {renderGrid()}
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
  const [activeTab, setActiveTab] = React.useState('palette');
  const [colorFormat, setColorFormat] = React.useState<'HEX' | 'RGB' | 'HSL'>('HEX');
  const [localInputValue, setLocalInputValue] = React.useState('');

  // Helper to convert hex to other formats
  const formatColor = (hex: string, format: 'HEX' | 'RGB' | 'HSL'): string => {
    if (!hex || hex === '#000000') return format === 'HEX' ? '#000000' : format === 'RGB' ? 'rgb(0, 0, 0)' : 'hsl(0, 0%, 0%)';
    const cleanHex = hex.replace('#', '');
    const r = parseInt(cleanHex.substring(0, 2), 16);
    const g = parseInt(cleanHex.substring(2, 4), 16);
    const b = parseInt(cleanHex.substring(4, 6), 16);

    if (format === 'HEX') return hex;
    if (format === 'RGB') return `rgb(${r}, ${g}, ${b})`;
    if (format === 'HSL') {
      const rNorm = r / 255;
      const gNorm = g / 255;
      const bNorm = b / 255;
      const max = Math.max(rNorm, gNorm, bNorm);
      const min = Math.min(rNorm, gNorm, bNorm);
      let h = 0, s = 0, l = (max + min) / 2;

      if (max !== min) {
        const d = max - min;
        s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
        switch (max) {
          case rNorm: h = (gNorm - bNorm) / d + (gNorm < bNorm ? 6 : 0); break;
          case gNorm: h = (bNorm - rNorm) / d + 2; break;
          case bNorm: h = (rNorm - gNorm) / d + 4; break;
        }
        h /= 6;
      }
      return `hsl(${Math.round(h * 360)}, ${Math.round(s * 100)}%, ${Math.round(l * 100)}%)`;
    }
    return hex;
  };

  // Helper to convert hex to HSL object
  const hexToHsl = (hex: string) => {
    let cleanHex = hex.replace('#', '');
    if (cleanHex.length === 3) {
      cleanHex = cleanHex.split('').map(c => c + c).join('');
    }
    const r = parseInt(cleanHex.substring(0, 2), 16) / 255;
    const g = parseInt(cleanHex.substring(2, 4), 16) / 255;
    const b = parseInt(cleanHex.substring(4, 6), 16) / 255;

    const max = Math.max(r, g, b);
    const min = Math.min(r, g, b);
    let h = 0, s = 0, l = (max + min) / 2;

    if (max !== min) {
      const d = max - min;
      s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
      switch (max) {
        case r: h = (g - b) / d + (g < b ? 6 : 0); break;
        case g: h = (b - r) / d + 2; break;
        case b: h = (r - g) / d + 4; break;
      }
      h /= 6;
    }

    return { h: Math.round(h * 360), s: Math.round(s * 100), l: Math.round(l * 100) };
  };

  // Helper to convert HSL object to hex
  const hslToHex = (h: number, s: number, l: number) => {
    l /= 100;
    const a = s * Math.min(l, 1 - l) / 100;
    const f = (n: number) => {
      const k = (n + h / 30) % 12;
      const color = l - a * Math.max(Math.min(k - 3, 9 - k, 1), -1);
      return Math.round(255 * color).toString(16).padStart(2, '0');
    };
    return `#${f(0)}${f(8)}${f(4)}`;
  };

  const [hslValues, setHslValues] = React.useState({ h: 0, s: 0, l: 0 });

  React.useEffect(() => {
    if (activeTab === 'picker') {
      const hsl = hexToHsl(getDisplayColor());
      setHslValues(hsl);
    }
  }, [displayValue, activeTab]);

  const handleSliderChange = (type: 'h' | 's' | 'l', value: number) => {
    const newHsl = { ...hslValues, [type]: value };
    setHslValues(newHsl);
    const newHex = hslToHex(newHsl.h, newHsl.s, newHsl.l);
    eventHandler('OnChange', id, [newHex]);
  };

  const renderFooter = () => (
    <div className="flex items-center gap-2 mt-2 pt-2 border-t border-border">
      <Select
        value={colorFormat}
        onValueChange={(val: 'HEX' | 'RGB' | 'HSL') => setColorFormat(val)}
      >
        <SelectTrigger className="w-[80px] h-8 text-xs">
          <SelectValue placeholder="Format" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="HEX">HEX</SelectItem>
          <SelectItem value="RGB">RGB</SelectItem>
          <SelectItem value="HSL">HSL</SelectItem>
        </SelectContent>
      </Select>
      <Input
        value={localInputValue}
        onChange={handleLocalInputChange}
        className="h-8 text-xs font-mono"
      />
      <div
        className="w-8 h-8 rounded-md border border-input shadow-sm shrink-0"
        style={{ backgroundColor: getDisplayColor() }}
      />
    </div>
  );

  const getLuminance = (hex: string): number => {
    let cleanHex = hex.replace('#', '');
    if (cleanHex.length === 3) {
      cleanHex = cleanHex.split('').map(c => c + c).join('');
    }
    const r = parseInt(cleanHex.substring(0, 2), 16);
    const g = parseInt(cleanHex.substring(2, 4), 16);
    const b = parseInt(cleanHex.substring(4, 6), 16);
    // Standard luminance formula
    return 0.299 * r + 0.587 * g + 0.114 * b;
  };

  React.useEffect(() => {
    setLocalInputValue(formatColor(getDisplayColor(), colorFormat));
  }, [displayValue, colorFormat]);

  const handleLocalInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setLocalInputValue(e.target.value);
    // Try to parse and update parent if valid
    // For now, only simple sync if valid hex, or use existing convertToHex
    const converted = convertToHex(e.target.value);
    if (converted && converted !== '#000000' && converted !== displayValue) {
      eventHandler('OnChange', id, [converted]);
    }
  };

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
    const hslMatch = colorValue.match(/hsla?\((\d+),\s*(\d+)%?,\s*(\d+)%?(?:,\s*[\d.]+)?\)/);
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

  if (variant === 'ThemePicker') {
    return (
      <div className="flex items-center space-x-2">
        <Popover>
          <PopoverTrigger asChild>
            <button
              type="button"
              disabled={disabled}
              className={cn(
                colorInputPickerVariants({ scale }),
                'p-0 rounded-md shadow-none focus:outline-none ring-offset-1 ring-1 transition-all',
                // Dynamic double border based on luminance
                // Light background (color): Inner white, Outer black
                // Dark background (color): Inner black, Outer white
                getLuminance(getDisplayColor()) > 128
                  ? 'ring-offset-white ring-black/10'
                  : 'ring-offset-black/5 ring-white/20',
                disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer',
                invalid && inputStyles.invalidInput
              )}
              style={{ backgroundColor: getDisplayColor() }}
            >
              <span className="sr-only">Pick a color</span>
            </button>
          </PopoverTrigger>
          <PopoverContent className="w-auto p-3" align="start">
            <Tabs value={activeTab} onValueChange={setActiveTab} className="w-[460px]">
              <div className="flex items-center justify-between mb-3">
                <span className="text-sm font-medium px-1">
                  Choose a color for {placeholder || 'this item'}
                </span>
                <TabsList className="h-7">
                  <TabsTrigger value="palette" className="h-5 px-2 text-xs">Palette</TabsTrigger>
                  <TabsTrigger value="picker" className="h-5 px-2 text-xs">Picker</TabsTrigger>
                </TabsList>
              </div>

              <TabsContent value="palette" className="mt-0">
                <ThemeColorGrid
                  selectedColor={getDisplayColor()}
                  onSelect={(color) => {
                    eventHandler('OnChange', id, [color]);
                  }}
                />

                {renderFooter()}
              </TabsContent>

              <TabsContent value="picker" className="mt-0">
                <div className="h-[238px] p-2 flex flex-col justify-center gap-6">
                  <div className="space-y-2">
                    <div className="flex justify-between text-xs font-medium">
                      <span>Hue</span>
                      <span>{hslValues.h}°</span>
                    </div>
                    <div className="relative px-1">
                      <div className="absolute inset-0 h-2 rounded-full bg-gradient-to-r from-[hsl(0,100%,50%)] via-[hsl(60,100%,50%)] via-[hsl(120,100%,50%)] via-[hsl(180,100%,50%)] via-[hsl(240,100%,50%)] via-[hsl(300,100%,50%)] to-[hsl(360,100%,50%)] opacity-50 pointer-events-none" />
                      <Slider
                        value={[hslValues.h]}
                        max={360}
                        step={1}
                        onValueChange={(vals) => handleSliderChange('h', vals[0])}
                        className="[&>.bg-primary]:bg-transparent"
                      />
                    </div>
                  </div>

                  <div className="space-y-2">
                    <div className="flex justify-between text-xs font-medium">
                      <span>Saturation</span>
                      <span>{hslValues.s}%</span>
                    </div>
                    <div className="relative px-1">
                      <div
                        className="absolute inset-0 h-2 rounded-full pointer-events-none opacity-50"
                        style={{ background: `linear-gradient(to right, hsl(${hslValues.h}, 0%, ${hslValues.l}%), hsl(${hslValues.h}, 100%, ${hslValues.l}%))` }}
                      />
                      <Slider
                        value={[hslValues.s]}
                        max={100}
                        step={1}
                        onValueChange={(vals) => handleSliderChange('s', vals[0])}
                        className="[&>.bg-primary]:bg-transparent"
                      />
                    </div>
                  </div>

                  <div className="space-y-2">
                    <div className="flex justify-between text-xs font-medium">
                      <span>Lightness</span>
                      <span>{hslValues.l}%</span>
                    </div>
                    <div className="relative px-1">
                      <div
                        className="absolute inset-0 h-2 rounded-full pointer-events-none opacity-50"
                        style={{ background: `linear-gradient(to right, hsl(${hslValues.h}, ${hslValues.s}%, 0%), hsl(${hslValues.h}, ${hslValues.s}%, 50%), hsl(${hslValues.h}, ${hslValues.s}%, 100%))` }}
                      />
                      <Slider
                        value={[hslValues.l]}
                        max={100}
                        step={1}
                        onValueChange={(vals) => handleSliderChange('l', vals[0])}
                        className="[&>.bg-primary]:bg-transparent"
                      />
                    </div>
                  </div>
                </div>
                {renderFooter()}
              </TabsContent>
            </Tabs>
          </PopoverContent>
        </Popover>
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
