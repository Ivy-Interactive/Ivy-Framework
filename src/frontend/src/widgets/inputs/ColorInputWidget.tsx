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
// unused Slider import removed
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import {} from // Select imports removed as they are no longer used
'@/components/ui/select';
import * as SliderPrimitive from '@radix-ui/react-slider';

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
  foreground?: boolean;
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
  // Generate 160 colors (8 rows x 20 columns)
  const rows = 8;
  const cols = 20;

  // Theme color mappings
  const THEME_COLOR_MAPPINGS = [
    { label: 'P', var: '--primary' },
    { label: 'PF', var: '--primary-foreground' },
    { label: 'S', var: '--secondary' },
    { label: 'SF', var: '--secondary-foreground' },
    { label: 'Su', var: '--success' },
    { label: 'SuF', var: '--success-foreground' },
    { label: 'D', var: '--destructive' },
    { label: 'DF', var: '--destructive-foreground' },
    { label: 'W', var: '--warning' },
    { label: 'WF', var: '--warning-foreground' },
    { label: 'I', var: '--info' },
    { label: 'IF', var: '--info-foreground' },
    { label: 'M', var: '--muted' },
    { label: 'MF', var: '--muted-foreground' },
    { label: 'A', var: '--accent' },
    { label: 'AF', var: '--accent-foreground' },
    { label: 'Po', var: '--popover' },
    { label: 'PoF', var: '--popover-foreground' },
    { label: 'Ca', var: '--card' },
    { label: 'CaF', var: '--card-foreground' },
    { label: 'Bg', var: '--background' },
    { label: 'Fg', var: '--foreground' },
    { label: 'In', var: '--input' },
    { label: 'Bo', var: '--border' },
    { label: 'Ri', var: '--ring' },
  ];

  const [resolvedThemeColors, setResolvedThemeColors] = React.useState<
    Record<string, string[]>
  >({});

  // Helper to convert any CSS color string to Hex
  const colorToHex = (color: string): string | null => {
    if (!color) return null;
    const ctx = document.createElement('canvas').getContext('2d');
    if (!ctx) return null;
    ctx.fillStyle = color;
    return ctx.fillStyle;
  };

  const updateThemeColors = React.useCallback(() => {
    if (typeof window === 'undefined') return;

    const computedStyle = getComputedStyle(document.documentElement);
    const newMappings: Record<string, string[]> = {};

    THEME_COLOR_MAPPINGS.forEach(mapping => {
      // Get the value of the CSS variable
      let colorValue = computedStyle.getPropertyValue(mapping.var).trim();

      // Handle Tailwind's space-separated HSL channels (e.g., "222.2 47.4% 11.2%")
      // Check if it looks like numbers/percentages separated by spaces
      if (/^[\d.]+\s+[\d.]+%?\s+[\d.]+%?/.test(colorValue)) {
        colorValue = `hsl(${colorValue})`;
      }

      const hex = colorToHex(colorValue);

      if (hex) {
        const normalizedHex = hex.toLowerCase();
        if (!newMappings[normalizedHex]) {
          newMappings[normalizedHex] = [];
        }
        newMappings[normalizedHex].push(mapping.label);
      }
    });
    setResolvedThemeColors(newMappings);
  }, []);

  React.useEffect(() => {
    // Initial load with retries to handle async style injection
    updateThemeColors();
    const retryTimers = [100, 300, 500, 1000].map(delay =>
      setTimeout(updateThemeColors, delay)
    );

    if (typeof window === 'undefined') {
      retryTimers.forEach(clearTimeout);
      return;
    }

    // Observe changes to the html element (for class/style) and head (for style tag injection)
    const observer = new MutationObserver(mutations => {
      let shouldUpdate = false;
      for (const mutation of mutations) {
        if (
          mutation.type === 'attributes' &&
          (mutation.attributeName === 'style' ||
            mutation.attributeName === 'class')
        ) {
          shouldUpdate = true;
          break;
        }
        if (mutation.type === 'childList') {
          // Check if a style tag was added/removed to head
          for (const node of mutation.addedNodes) {
            if (node.nodeName === 'STYLE') {
              shouldUpdate = true;
              break;
            }
          }
          if (!shouldUpdate) {
            for (const node of mutation.removedNodes) {
              if (node.nodeName === 'STYLE') {
                shouldUpdate = true;
                break;
              }
            }
          }
        }
        if (shouldUpdate) break;
      }

      if (shouldUpdate) {
        // Small delay to ensure styles are computed by browser
        setTimeout(updateThemeColors, 10);
      }
    });

    observer.observe(document.documentElement, {
      attributes: true,
      attributeFilter: ['style', 'class'],
    });

    observer.observe(document.head, {
      childList: true,
    });

    return () => {
      observer.disconnect();
      retryTimers.forEach(clearTimeout);
    };
  }, [updateThemeColors]);

  // Helper to determine contrast color for the labels
  const getContrastColor = (hex: string): string => {
    if (!hex || !hex.startsWith('#')) return '#000000';
    const r = parseInt(hex.substring(1, 3), 16);
    const g = parseInt(hex.substring(3, 5), 16);
    const b = parseInt(hex.substring(5, 7), 16);
    const yiq = (r * 299 + g * 587 + b * 114) / 1000;
    return yiq >= 128 ? '#000000' : '#FFFFFF';
  };

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

        // Simple HSL to Hex
        const h = hue;
        const s = saturation;
        const l = lightness;

        const hNorm = h / 360;
        const sNorm = s / 100;
        const lNorm = l / 100;
        let rVal, gVal, bVal;

        if (sNorm === 0) {
          rVal = gVal = bVal = lNorm;
        } else {
          const hue2rgb = (p: number, q: number, t: number) => {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1 / 6) return p + (q - p) * 6 * t;
            if (t < 1 / 2) return q;
            if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
            return p;
          };
          const q =
            lNorm < 0.5 ? lNorm * (1 + sNorm) : lNorm + sNorm - lNorm * sNorm;
          const p = 2 * lNorm - q;
          rVal = hue2rgb(p, q, hNorm + 1 / 3);
          gVal = hue2rgb(p, q, hNorm);
          bVal = hue2rgb(p, q, hNorm - 1 / 3);
        }

        const toHex = (x: number) => {
          const hex = Math.round(x * 255).toString(16);
          return hex.length === 1 ? '0' + hex : hex;
        };

        const hexColor = `#${toHex(rVal)}${toHex(gVal)}${toHex(bVal)}`;
        const normalizedHex = hexColor.toLowerCase();
        const isSelected = selectedColor?.toLowerCase() === normalizedHex;

        // Check for theme color match
        const themeLabels = resolvedThemeColors[normalizedHex];
        let label = null;
        if (themeLabels && themeLabels.length > 0) {
          label = themeLabels[0];
          if (themeLabels.length > 1) {
            label += '+';
          }
        }

        rowColors.push(
          <button
            key={`${r}-${c}`}
            type="button"
            className={cn(
              'w-7 h-7 shrink-0 rounded-full hover:scale-125 transition-transform hover:z-10 hover:shadow-sm border border-black/5 relative flex items-center justify-center',
              isSelected && 'ring-1 ring-offset-1 ring-black/50 z-20 scale-110'
            )}
            style={{ backgroundColor: hexColor }}
            onClick={() => onSelect(hexColor)}
            title={hexColor}
          >
            {label && (
              <span
                style={{
                  color: getContrastColor(hexColor),
                  // Add text shadow for better visibility on mid-tone colors
                  textShadow:
                    getContrastColor(hexColor) === '#FFFFFF'
                      ? '0 1px 2px rgba(0,0,0,0.5)'
                      : '0 1px 1px rgba(255,255,255,0.5)',
                }}
                className="text-[10px] font-black leading-none pointer-events-none select-none z-10"
              >
                {label}
              </span>
            )}
          </button>
        );
      }
      grid.push(
        <div key={r} className="flex gap-1 justify-center">
          {rowColors}
        </div>
      );
    }

    return grid;
  };

  return (
    <div className="flex flex-col gap-1 p-6 h-[300px] w-full items-center justify-center bg-background rounded-md shadow-sm">
      {renderGrid()}
    </div>
  );
};

const ColorSlider = React.forwardRef<
  React.ElementRef<typeof SliderPrimitive.Root>,
  React.ComponentPropsWithoutRef<typeof SliderPrimitive.Root>
>(({ className, ...props }, ref) => (
  <SliderPrimitive.Root
    ref={ref}
    className={cn(
      'relative flex w-full touch-none select-none items-center',
      className
    )}
    {...props}
  >
    <SliderPrimitive.Track className="relative h-6 w-full grow overflow-hidden rounded-full cursor-pointer">
      <SliderPrimitive.Range className="absolute h-full bg-transparent" />
    </SliderPrimitive.Track>
    <SliderPrimitive.Thumb className="block h-6 w-6 rounded-full border-2 border-white bg-transparent shadow transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-50 cursor-grab active:cursor-grabbing hover:bg-white/10" />
  </SliderPrimitive.Root>
));
ColorSlider.displayName = SliderPrimitive.Root.displayName;

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
  foreground = false,
}) => {
  const eventHandler = useEventHandler();
  // Use derived state for display and input values
  const displayValue = value ?? '';
  const inputValue = value ?? '';
  const [activeTab, setActiveTab] = React.useState('palette');
  // Enforce HEX mode only as per requirement
  const [colorFormat] = React.useState<'HEX'>('HEX');
  const [localInputValue, setLocalInputValue] = React.useState('');

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
    return hexValue.startsWith('#') ? hexValue : '#000000';
  };

  // Helper to convert hex to other formats
  const formatColor = (hex: string, format: 'HEX' | 'RGB' | 'HSL'): string => {
    if (!hex || hex === '#000000') return '#000000'; // Default to Hex format only

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
      let h = 0,
        s = 0;
      const l = (max + min) / 2;

      if (max !== min) {
        const d = max - min;
        s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
        switch (max) {
          case rNorm:
            h = (gNorm - bNorm) / d + (gNorm < bNorm ? 6 : 0);
            break;
          case gNorm:
            h = (bNorm - rNorm) / d + 2;
            break;
          case bNorm:
            h = (rNorm - gNorm) / d + 4;
            break;
        }
        h /= 6;
      }
      return `hsl(${Math.round(h * 360)}, ${Math.round(s * 100)}%, ${Math.round(l * 100)}%)`;
    }
    return hex;
  };

  // hexToHsl removed as unused

  // hslToHex removed as unused

  // Helper to convert hex to RGB object
  const hexToRgb = (hex: string) => {
    let cleanHex = hex.replace('#', '');
    if (cleanHex.length === 3) {
      cleanHex = cleanHex
        .split('')
        .map(c => c + c)
        .join('');
    }
    const r = parseInt(cleanHex.substring(0, 2), 16);
    const g = parseInt(cleanHex.substring(2, 4), 16);
    const b = parseInt(cleanHex.substring(4, 6), 16);
    return { r, g, b };
  };

  // Helper to convert RGB object to hex
  const rgbToHex = (r: number, g: number, b: number) => {
    const toHex = (n: number) => Math.round(n).toString(16).padStart(2, '0');
    return `#${toHex(r)}${toHex(g)}${toHex(b)}`;
  };

  const [rgbValues, setRgbValues] = React.useState({ r: 0, g: 0, b: 0 });

  React.useEffect(() => {
    if (activeTab === 'picker') {
      // HSL update removed
      const rgb = hexToRgb(getDisplayColor());
      setRgbValues(rgb);
    }
  }, [displayValue, activeTab]);

  // handleSliderChange removed as HSL sliders are no longer used

  const handleRgbSliderChange = (type: 'r' | 'g' | 'b', value: number) => {
    const newRgb = { ...rgbValues, [type]: value };
    setRgbValues(newRgb);
    const newHex = rgbToHex(newRgb.r, newRgb.g, newRgb.b);
    eventHandler('OnChange', id, [newHex]);
  };

  const renderFooter = () => (
    <div className="flex items-center gap-2 mt-2 pt-2 border-t border-border">
      {/* Format selection removed - HEX only forced */}
      <div className="w-[80px] h-8 flex items-center justify-center text-xs font-medium text-muted-foreground border rounded bg-muted/50">
        HEX
      </div>
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

  // Helper to determine contrast color for the "A"
  const getContrastColor = (hex: string): string => {
    if (!hex || !hex.startsWith('#')) return '#000000';
    const r = parseInt(hex.substring(1, 3), 16);
    const g = parseInt(hex.substring(3, 5), 16);
    const b = parseInt(hex.substring(5, 7), 16);
    const yiq = (r * 299 + g * 587 + b * 114) / 1000;
    return yiq >= 128 ? '#000000' : '#FFFFFF';
  };

  if (variant === 'ThemePicker') {
    const isForeground =
      foreground ||
      (placeholder && placeholder.toLowerCase().includes('foreground'));
    const contrastColor = getContrastColor(getDisplayColor());

    return (
      <div className="flex items-center space-x-2">
        <Popover>
          <PopoverTrigger asChild>
            <button
              type="button"
              disabled={disabled}
              className={cn(
                colorInputPickerVariants({ scale }),
                'p-0 rounded-md shadow-none focus:outline-none ring-offset-1 ring-1 transition-all relative',
                'ring-offset-white ring-black',
                disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer',
                invalid && inputStyles.invalidInput
              )}
              style={{ backgroundColor: getDisplayColor() }}
            >
              <span className="sr-only">Pick a color</span>
              {isForeground && (
                <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
                  <span
                    style={{ color: contrastColor }}
                    className="font-extrabold text-lg"
                  >
                    A
                  </span>
                </div>
              )}
            </button>
          </PopoverTrigger>
          <PopoverContent className="w-auto p-3" align="start">
            <Tabs
              value={activeTab}
              onValueChange={setActiveTab}
              className="w-[740px]"
            >
              <div className="flex items-center justify-between mb-3">
                <span className="text-sm font-medium px-1">
                  Choose a color for {placeholder || 'this item'}
                </span>
                <TabsList className="h-7">
                  <TabsTrigger value="palette" className="h-5 px-2 text-xs">
                    Palette
                  </TabsTrigger>
                  <TabsTrigger value="picker" className="h-5 px-2 text-xs">
                    Picker
                  </TabsTrigger>
                </TabsList>
              </div>

              <TabsContent value="palette" className="mt-0">
                <ThemeColorGrid
                  selectedColor={getDisplayColor()}
                  onSelect={color => {
                    eventHandler('OnChange', id, [color]);
                  }}
                />

                {renderFooter()}
              </TabsContent>

              <TabsContent value="picker" className="mt-0">
                <div className="h-[300px] p-6 flex flex-col justify-center gap-6">
                  {/* HSL logic removed, rendering only RGB sliders with HEX values */}
                  <>
                    <div className="space-y-2">
                      <div className="flex justify-between text-xs font-medium">
                        <span>Red</span>
                        <span>
                          {rgbValues.r
                            .toString(16)
                            .toUpperCase()
                            .padStart(2, '0')}
                        </span>
                      </div>
                      <div className="relative px-1">
                        <div
                          className="absolute inset-0 h-6 rounded-full pointer-events-none opacity-50"
                          style={{
                            background: `linear-gradient(to right, rgb(0, ${rgbValues.g}, ${rgbValues.b}), rgb(255, ${rgbValues.g}, ${rgbValues.b}))`,
                          }}
                        />
                        <ColorSlider
                          value={[rgbValues.r]}
                          max={255}
                          step={1}
                          onValueChange={vals =>
                            handleRgbSliderChange('r', vals[0])
                          }
                          className=""
                        />
                      </div>
                    </div>

                    <div className="space-y-2">
                      <div className="flex justify-between text-xs font-medium">
                        <span>Green</span>
                        <span>
                          {rgbValues.g
                            .toString(16)
                            .toUpperCase()
                            .padStart(2, '0')}
                        </span>
                      </div>
                      <div className="relative px-1">
                        <div
                          className="absolute inset-0 h-6 rounded-full pointer-events-none opacity-50"
                          style={{
                            background: `linear-gradient(to right, rgb(${rgbValues.r}, 0, ${rgbValues.b}), rgb(${rgbValues.r}, 255, ${rgbValues.b}))`,
                          }}
                        />
                        <ColorSlider
                          value={[rgbValues.g]}
                          max={255}
                          step={1}
                          onValueChange={vals =>
                            handleRgbSliderChange('g', vals[0])
                          }
                          className=""
                        />
                      </div>
                    </div>

                    <div className="space-y-2">
                      <div className="flex justify-between text-xs font-medium">
                        <span>Blue</span>
                        <span>
                          {rgbValues.b
                            .toString(16)
                            .toUpperCase()
                            .padStart(2, '0')}
                        </span>
                      </div>
                      <div className="relative px-1">
                        <div
                          className="absolute inset-0 h-6 rounded-full pointer-events-none opacity-50"
                          style={{
                            background: `linear-gradient(to right, rgb(${rgbValues.r}, ${rgbValues.g}, 0), rgb(${rgbValues.r}, ${rgbValues.g}, 255))`,
                          }}
                        />
                        <ColorSlider
                          value={[rgbValues.b]}
                          max={255}
                          step={1}
                          onValueChange={vals =>
                            handleRgbSliderChange('b', vals[0])
                          }
                          className=""
                        />
                      </div>
                    </div>
                  </>
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
