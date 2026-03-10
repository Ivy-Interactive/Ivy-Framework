import * as React from 'react';
import type { VariantProps } from 'class-variance-authority';

import { cn } from '@/lib/utils';
import { inputVariant } from './input/variant';

export interface InputProps
  extends Omit<React.ComponentProps<'input'>, 'size'>,
    VariantProps<typeof inputVariant> {}

const Input = React.forwardRef<HTMLInputElement, InputProps>(
  ({ className, type, scale, ...props }, ref) => {
    return (
      <input
        type={type}
        data-1p-ignore
        autoComplete="off"
        className={cn(inputVariant({ scale, className }))}
        ref={ref}
        {...props}
      />
    );
  }
);
Input.displayName = 'Input';

export { Input };
