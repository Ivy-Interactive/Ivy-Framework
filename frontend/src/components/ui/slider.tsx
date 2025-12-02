import * as React from 'react';
import * as SliderPrimitive from '@radix-ui/react-slider';
import { cn } from '@/lib/utils';
import { Scales } from '@/types/scale';

interface SliderWithCurrencyProps
  extends React.ComponentPropsWithoutRef<typeof SliderPrimitive.Root> {
  currency?: string;
  scale?: Scales;
}
const Slider = React.forwardRef<
  React.ElementRef<typeof SliderPrimitive.Root>,
  SliderWithCurrencyProps
>(({ className, currency, scale = Scales.Medium, ...props }, ref) => {
  const currentValue = props.value?.[0] ?? props.defaultValue?.[0] ?? 0;

  const formattedValue = React.useMemo(() => {
    if (!currency) return currentValue;
    try {
      return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency,
        minimumFractionDigits: 0,
        maximumFractionDigits: 2,
      }).format(currentValue);
    } catch {
      return currentValue;
    }
  }, [currentValue, currency]);

  // Size variants for track and thumb
  const sizeVariants: Record<
    string,
    { track: string; thumb: string; tooltip: string }
  > = {
    Small: {
      track: 'h-1',
      thumb: 'h-3 w-3',
      tooltip: 'text-xs -top-6',
    },
    Medium: {
      track: 'h-1.5',
      thumb: 'h-4 w-4',
      tooltip: 'text-sm -top-7',
    },
    Large: {
      track: 'h-2',
      thumb: 'h-5 w-5',
      tooltip: 'text-ml -top-8',
    },
  };

  const variant = sizeVariants[String(scale)];

  return (
    <SliderPrimitive.Root
      ref={ref}
      className={cn(
        'relative flex w-full touch-none select-none items-center',
        className
      )}
      {...props}
    >
      <SliderPrimitive.Track
        className={cn(
          'relative w-full grow overflow-hidden rounded-full bg-muted',
          variant.track
        )}
      >
        <SliderPrimitive.Range className="absolute h-full bg-primary" />
      </SliderPrimitive.Track>
      <SliderPrimitive.Thumb
        className={cn(
          'relative block rounded-full border bg-background shadow transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-50 cursor-pointer',
          variant.thumb
        )}
      >
        <div
          className={cn(
            'absolute left-1/2 transform -translate-x-1/2 bg-popover text-foreground p-1 rounded shadow',
            variant.tooltip
          )}
        >
          {formattedValue}
        </div>
      </SliderPrimitive.Thumb>
    </SliderPrimitive.Root>
  );
});

Slider.displayName = SliderPrimitive.Root.displayName;

export { Slider };
