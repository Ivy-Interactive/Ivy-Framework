import { logger } from '@/lib/logger';
import { cn } from '@/lib/utils';
import * as CheckboxPrimitive from '@radix-ui/react-checkbox';
import { Check, Minus } from 'lucide-react';
import * as React from 'react';
import { Scales } from '@/types/scale';
export type NullableBoolean = boolean | null | undefined;

const getSizeClasses = (scale?: Scales): string => {
  switch (scale) {
    case Scales.Small:
      return 'h-3 w-3';
    case Scales.Large:
      return 'h-5 w-5';
    default:
      return 'h-4 w-4';
  }
};

type AppCheckboxProps = {
  id: string;
  checked: NullableBoolean;
  onCheckedChange: (checked: boolean | null) => void;
  disabled?: boolean;
  nullable?: boolean;
  className?: string;
  scale?: Scales;
};

const Checkbox = React.forwardRef<
  React.ElementRef<typeof CheckboxPrimitive.Root>,
  AppCheckboxProps
>(
  (
    {
      id,
      checked,
      onCheckedChange,
      disabled,
      nullable = false,
      className = '',
      scale = Scales.Medium,
      ...props
    },
    ref
  ) => {
    // Map undefined to null when nullable, then null to 'indeterminate' for Radix
    const normalizedChecked =
      nullable && checked === undefined ? null : checked;
    const uiChecked =
      nullable && normalizedChecked === null
        ? 'indeterminate'
        : !!normalizedChecked;

    // Cycle: null -> true -> false -> null (if nullable)
    const handleCheckedChange = (next: boolean) => {
      logger.debug(
        'Checkbox clicked, next:',
        next,
        'current checked:',
        normalizedChecked
      );
      if (nullable) {
        if (normalizedChecked === null) onCheckedChange(true);
        else if (normalizedChecked === true) onCheckedChange(false);
        else onCheckedChange(null);
      } else {
        onCheckedChange(next);
      }
    };

    const baseClass = `peer ${getSizeClasses(scale)} shrink-0 rounded-sm border border-border shadow focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring cursor-pointer disabled:cursor-not-allowed disabled:opacity-50 data-[state=checked]:bg-primary data-[state=checked]:text-primary-foreground data-[state=checked]:border-border`;
    const finalClass = className?.includes('bg-red-50')
      ? baseClass.replace('data-[state=checked]:bg-primary', '')
      : baseClass;

    return (
      <CheckboxPrimitive.Root
        ref={ref}
        id={id}
        checked={uiChecked}
        onCheckedChange={handleCheckedChange}
        disabled={disabled}
        className={cn(finalClass, className)}
        {...props}
      >
        <CheckboxPrimitive.Indicator
          className={cn('flex items-center justify-center text-current')}
        >
          {uiChecked === 'indeterminate' ? (
            <Minus className={getSizeClasses(scale)} />
          ) : (
            <Check className={getSizeClasses(scale)} />
          )}
        </CheckboxPrimitive.Indicator>
      </CheckboxPrimitive.Root>
    );
  }
);
Checkbox.displayName = 'AppCheckbox';

export { Checkbox };
