import * as React from 'react';
import * as TogglePrimitive from '@radix-ui/react-toggle';
import { type VariantProps } from 'class-variance-authority';

import { cn } from '@/lib/utils';
import { toggleVariants } from './toggle-variants';

const Toggle = React.forwardRef<
  React.ElementRef<typeof TogglePrimitive.Root>,
  React.ComponentPropsWithoutRef<typeof TogglePrimitive.Root> &
    VariantProps<typeof toggleVariants> & {
      dataTestId?: string;
    }
>(({ className, variant, size, dataTestId, ...props }, ref) => {
  let toggleClass = toggleVariants({ variant, size, className });
  if (className?.includes('bg-red-50')) {
    toggleClass = toggleClass.replace('data-[state=on]:bg-accent', '');
  }
  return (
    <TogglePrimitive.Root
      ref={ref}
      className={cn(toggleClass, className)}
      data-testid={dataTestId}
      {...props}
    />
  );
});

Toggle.displayName = TogglePrimitive.Root.displayName;

export { Toggle };
