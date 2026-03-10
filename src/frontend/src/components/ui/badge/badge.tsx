import * as React from 'react';
import type { VariantProps } from 'class-variance-authority';

import { cn } from '@/lib/utils';
import { badgeVariant } from './variant';

export interface BadgeProps
  extends React.HTMLAttributes<HTMLDivElement>,
    VariantProps<typeof badgeVariant> {}

function Badge({ className, variant, scale, ...props }: BadgeProps) {
  return (
    <div
      className={cn(badgeVariant({ variant, scale }), className)}
      {...props}
    />
  );
}

export { Badge };
