import * as React from 'react';
import * as SliderPrimitive from '@radix-ui/react-slider';
import { cn } from '@/lib/utils';

interface SliderWithCurrencyProps
  extends React.ComponentPropsWithoutRef<typeof SliderPrimitive.Root> {
  currency?: string;
}
const Slider = React.forwardRef<
  React.ElementRef<typeof SliderPrimitive.Root>,
  SliderWithCurrencyProps
>(({ className, currency, ...props }, ref) => {
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

  return (
    <SliderPrimitive.Root
      ref={ref}
      className={cn(
        'relative flex w-full touch-none select-none items-center',
        className
      )}
      {...props}
    >
      <SliderPrimitive.Track className="relative h-1.5 w-full grow overflow-hidden rounded-full bg-muted">
        <SliderPrimitive.Range className="absolute h-full bg-primary" />
      </SliderPrimitive.Track>
      <SliderPrimitive.Thumb className="relative block h-4 w-4 rounded-full border bg-background shadow transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-50 cursor-pointer">
        <div className="absolute -top-6 left-1/2 transform -translate-x-1/2 text-xs bg-popover text-popover-foreground p-1 rounded shadow">
          {formattedValue}
        </div>
      </SliderPrimitive.Thumb>
    </SliderPrimitive.Root>
  );
});

Slider.displayName = SliderPrimitive.Root.displayName;

export { Slider };
