import { useEventHandler } from '@/components/event-handler';
import { inputStyles } from '@/lib/styles';
import { Input } from '@/components/ui/input';
import React from 'react';
import { cn } from '@/lib/utils';
import { colorInputPickerVariant } from '@/components/ui/input/color-input-variant';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import * as SliderPrimitive from '@radix-ui/react-slider';
import { Scales } from '@/types/scale';
import CopyToClipboardButton from '@/components/CopyToClipboardButton';

interface ThemeColorPickerWidgetProps {
  id: string;
  value: string | null;
  disabled?: boolean;
  invalid?: string;
  placeholder?: string;
  nullable?: boolean;
  events?: string[];
  scale?: Scales;
  foreground?: boolean;
  allowAlpha?: boolean;
}

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

const ThemeColorGrid: React.FC<{
  onSelect: (color: string) => void;
  selectedColor: string | null;
}> = ({ onSelect, selectedColor }) => {
  // Generate 160 colors (8 rows x 20 columns)
  const rows = 8;
  const cols = 20;

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

export const ThemeColorPickerWidget: React.FC<ThemeColorPickerWidgetProps> = ({
  id,
  value,
  disabled = false,
  invalid,
  placeholder,
  scale = Scales.Medium,
  foreground = false,
  allowAlpha = false,
}) => {
  const eventHandler = useEventHandler();
  const displayValue = value ?? '';
  const [activeTab, setActiveTab] = React.useState('palette');
  const [localInputValue, setLocalInputValue] = React.useState('');
  const [colorFormat] = React.useState<'HEX'>('HEX');

  const parseAlpha = (hex: string): number => {
    if (!hex || !hex.startsWith('#')) return 255;
    const clean = hex.slice(1);
    if (clean.length === 8) return parseInt(clean.slice(6, 8), 16);
    return 255;
  };

  const [alphaValue, setAlphaValue] = React.useState(() =>
    parseAlpha(value ?? '')
  );

  React.useEffect(() => {
    setAlphaValue(parseAlpha(value ?? ''));
  }, [value]);

  const combineWithAlpha = (hex6: string, alpha: number): string => {
    const base = hex6.startsWith('#') ? hex6 : '#' + hex6;
    const clean =
      base.length === 7
        ? base
        : base.length === 9
          ? base.slice(0, 7)
          : '#000000';
    if (alpha >= 255) return clean;
    const aa = Math.max(0, Math.min(255, alpha)).toString(16).padStart(2, '0');
    return clean + aa;
  };

  const getDisplayColor = React.useCallback((): string => {
    if (!displayValue) return '#000000';
    if (!displayValue.startsWith('#')) return '#000000';
    if (displayValue.length === 9) return displayValue.slice(0, 7);
    return displayValue;
  }, [displayValue]);

  // Helper to determine contrast color for the "A"
  const getContrastColor = (hex: string): string => {
    if (!hex || !hex.startsWith('#')) return '#000000';
    const r = parseInt(hex.substring(1, 3), 16);
    const g = parseInt(hex.substring(3, 5), 16);
    const b = parseInt(hex.substring(5, 7), 16);
    const yiq = (r * 299 + g * 587 + b * 114) / 1000;
    return yiq >= 128 ? '#000000' : '#FFFFFF';
  };

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
      const rgb = hexToRgb(getDisplayColor());
      setRgbValues(rgb);
    }
  }, [displayValue, activeTab, getDisplayColor]);

  const handleRgbSliderChange = (type: 'r' | 'g' | 'b', value: number) => {
    const newRgb = { ...rgbValues, [type]: value };
    setRgbValues(newRgb);
    const newHex = rgbToHex(newRgb.r, newRgb.g, newRgb.b);
    if (allowAlpha) {
      eventHandler('OnChange', id, [combineWithAlpha(newHex, alphaValue)]);
    } else {
      eventHandler('OnChange', id, [newHex]);
    }
  };

  const handleAlphaSliderChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setLocalDragAlpha(Number(e.target.value));
  };

  const handleAlphaCommit = () => {
    if (localDragAlpha !== null) {
      const baseHex = getDisplayColor();
      eventHandler('OnChange', id, [combineWithAlpha(baseHex, localDragAlpha)]);
    }
  };

  const [localDragAlpha, setLocalDragAlpha] = React.useState<number | null>(
    null
  );
  if (localDragAlpha !== null && alphaValue === localDragAlpha) {
    setLocalDragAlpha(null);
  }
  const displayAlpha = localDragAlpha ?? alphaValue;

  const renderFooter = () => (
    <div className="flex items-center gap-2 mt-2 pt-2 border-t border-border">
      <div className="w-[50px] h-8 flex items-center justify-center text-xs font-medium text-muted-foreground border rounded bg-muted/50 shrink-0">
        HEX
      </div>
      <Input
        value={localInputValue}
        onChange={handleLocalInputChange}
        className="h-8 text-xs font-mono min-w-0"
      />
      {allowAlpha && (
        <div
          className="relative rounded-md overflow-hidden border border-input shrink-0"
          style={{ width: 100, height: 24 }}
        >
          <div
            className="absolute inset-0"
            style={{
              backgroundImage:
                'repeating-conic-gradient(hsl(var(--muted)) 0% 25%, transparent 0% 50%)',
              backgroundSize: '12px 12px',
            }}
          />
          <div
            className="absolute inset-0"
            style={{
              background: `linear-gradient(to right, transparent, ${getDisplayColor()})`,
            }}
          />
          <input
            type="range"
            min={0}
            max={255}
            value={displayAlpha}
            onChange={handleAlphaSliderChange}
            onPointerUp={handleAlphaCommit}
            onKeyUp={handleAlphaCommit}
            className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
            aria-label={`Opacity: ${Math.round((displayAlpha / 255) * 100)}%`}
          />
          <div
            className="absolute top-0 bottom-0 w-1 bg-white border border-foreground/40 rounded-sm pointer-events-none"
            style={{ left: `calc(${(displayAlpha / 255) * 100}% - 2px)` }}
          />
        </div>
      )}
      {allowAlpha && (
        <span className="text-xs text-muted-foreground w-8 text-right tabular-nums shrink-0">
          {Math.round((displayAlpha / 255) * 100)}%
        </span>
      )}
      <CopyToClipboardButton
        textToCopy={localInputValue}
        className="h-8 w-8 px-0"
      />
      <div
        className="w-8 h-8 rounded-md border border-input shadow-sm shrink-0"
        style={{
          backgroundColor: getDisplayColor(),
          opacity: displayAlpha / 255,
        }}
      />
    </div>
  );

  // Helper to convert hex to other formats
  const formatColor = React.useCallback((hex: string): string => {
    // simplified for brevity as we forced HEX
    return hex;
  }, []);

  React.useEffect(() => {
    if (allowAlpha && alphaValue < 255) {
      setLocalInputValue(combineWithAlpha(getDisplayColor(), alphaValue));
    } else {
      setLocalInputValue(formatColor(getDisplayColor()));
    }
  }, [
    displayValue,
    colorFormat,
    getDisplayColor,
    formatColor,
    allowAlpha,
    alphaValue,
  ]);

  const handleLocalInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setLocalInputValue(e.target.value);
    const val = e.target.value;
    // Support both 6-digit and 8-digit hex
    if (/^#[0-9A-Fa-f]{6}$/.test(val)) {
      if (allowAlpha) {
        eventHandler('OnChange', id, [combineWithAlpha(val, alphaValue)]);
      } else {
        eventHandler('OnChange', id, [val]);
      }
    } else if (/^#[0-9A-Fa-f]{8}$/.test(val)) {
      setAlphaValue(parseInt(val.slice(7, 9), 16));
      eventHandler('OnChange', id, [val]);
    }
  };

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
              colorInputPickerVariant({ scale }),
              'p-0 rounded-md shadow-none focus:outline-none ring-offset-1 ring-1 transition-all relative',
              'ring-offset-white ring-black',
              disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer',
              invalid && inputStyles.invalidInput
            )}
            style={{
              backgroundColor:
                isForeground &&
                placeholder?.toLowerCase().endsWith('foreground') &&
                placeholder.toLowerCase() !== 'foreground'
                  ? `var(--${placeholder.toLowerCase().replace(' foreground', '')})`
                  : placeholder?.toLowerCase() === 'foreground'
                    ? 'var(--background)'
                    : getDisplayColor(),
              ...(allowAlpha && !isForeground
                ? { opacity: alphaValue / 255 }
                : {}),
            }}
          >
            <span className="sr-only">Pick a color</span>
            {isForeground && (
              <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
                <span
                  style={{
                    color:
                      (placeholder?.toLowerCase().endsWith('foreground') &&
                        placeholder.toLowerCase() !== 'foreground') ||
                      placeholder?.toLowerCase() === 'foreground'
                        ? getDisplayColor()
                        : contrastColor,
                    fontSize: '20px',
                    lineHeight: '1',
                  }}
                  className="font-extrabold"
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
                {/* RGB Sliders */}
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
};
