import * as React from 'react';
import type { VariantProps } from 'class-variance-authority';

import { cn } from '@/lib/utils';
import { badgeVariants } from './variants';

export interface BadgeProps
  extends React.HTMLAttributes<HTMLDivElement>,
    VariantProps<typeof badgeVariants> {}

function Badge({ className, variant, scale, ...props }: BadgeProps) {
  return (
    <div
      className={cn(badgeVariants({ variant, scale }), className)}
      {...props}
    />
  );
}

export { Badge };
