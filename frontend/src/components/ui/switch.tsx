import * as React from 'react';
import * as SwitchPrimitives from '@radix-ui/react-switch';
import type { VariantProps } from 'class-variance-authority';

import { cn } from '@/lib/utils';
import { switchVariants, switchThumbVariants } from './input/switch-variants';

const Switch = React.forwardRef<
  React.ElementRef<typeof SwitchPrimitives.Root>,
  React.ComponentPropsWithoutRef<typeof SwitchPrimitives.Root> &
    VariantProps<typeof switchVariants>
>(({ className, scale, ...props }, ref) => {
  const baseClass = switchVariants({ scale });
  const finalClass = className?.includes('bg-red-50')
    ? baseClass.replace('data-[state=checked]:bg-primary', '')
    : baseClass;
  return (
    <SwitchPrimitives.Root
      className={cn(finalClass, className)}
      {...props}
      ref={ref}
    >
      <SwitchPrimitives.Thumb className={cn(switchThumbVariants({ scale }))} />
    </SwitchPrimitives.Root>
  );
});
Switch.displayName = SwitchPrimitives.Root.displayName;

export { Switch };
