import React from 'react';
import type { VariantProps } from 'class-variance-authority';

import { cn } from '@/lib/utils';
import {
  detailLabelSizeVariants,
  detailValueSizeVariants,
  detailValueMultiLinePaddingVariants,
  detailsSizeVariants,
} from './detail/detail-variants';
import { DetailProvider } from './detail/DetailContext';
import { useDetailScale } from './detail/useDetailScale';
import { Scales } from '@/types/scale';

export interface DetailsProps
  extends Omit<React.HTMLAttributes<HTMLDivElement>, 'size'>,
    VariantProps<typeof detailsSizeVariants> {}

const Details = React.forwardRef<HTMLDivElement, DetailsProps>(
  ({ className, scale: propScale, children, ...props }, ref) => {
    const contextScale = useDetailScale();
    const scale = propScale ?? contextScale ?? Scales.Medium;

    return (
      <DetailProvider scale={scale as Scales}>
        <div
          ref={ref}
          className={cn('w-full [&>:last-child]:border-0', className)}
          {...props}
        >
          {children}
        </div>
      </DetailProvider>
    );
  }
);
Details.displayName = 'Details';

export interface DetailItemProps
  extends Omit<React.HTMLAttributes<HTMLDivElement>, 'size'> {
  label: string;
  multiLine?: boolean;
  scale?: Scales;
}

const DetailItem = React.forwardRef<HTMLDivElement, DetailItemProps>(
  (
    { className, label, multiLine, scale: propScale, children, ...props },
    ref
  ) => {
    const contextScale = useDetailScale();
    const scale = propScale ?? contextScale;

    return (
      <div
        ref={ref}
        className={cn(
          'border-b flex',
          multiLine && 'flex-col',
          !multiLine && 'items-center',
          className
        )}
        {...props}
      >
        <div className={cn(detailLabelSizeVariants({ scale }))}>{label}</div>
        <div
          className={cn(
            detailValueSizeVariants({ scale }),
            multiLine
              ? cn(
                  'whitespace-normal break-words text-left',
                  detailValueMultiLinePaddingVariants({ scale })
                )
              : 'truncate text-right ml-auto'
          )}
        >
          {children}
        </div>
      </div>
    );
  }
);
DetailItem.displayName = 'DetailItem';

export { Details, DetailItem };
