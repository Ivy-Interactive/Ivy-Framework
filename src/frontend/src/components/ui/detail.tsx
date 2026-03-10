import React from 'react';
import type { VariantProps } from 'class-variance-authority';

import { cn } from '@/lib/utils';
import {
  detailLabelSizeVariant,
  detailValueSizeVariant,
  detailValueMultiLinePaddingVariant,
  detailsSizeVariant,
} from './detail/detail-variant';
import { DetailProvider } from './detail/DetailContext';
import { useDetailScale } from './detail/useDetailScale';
import { Scales } from '@/types/scale';

export interface DetailsProps
  extends Omit<React.HTMLAttributes<HTMLDivElement>, 'size'>,
    VariantProps<typeof detailsSizeVariant> {}

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
  multiline?: boolean;
  scale?: Scales;
}

const DetailItem = React.forwardRef<HTMLDivElement, DetailItemProps>(
  (
    { className, label, multiline, scale: propScale, children, ...props },
    ref
  ) => {
    const contextScale = useDetailScale();
    const scale = propScale ?? contextScale;

    return (
      <div
        ref={ref}
        className={cn(
          'border-b flex',
          multiline && 'flex-col',
          !multiline && 'items-center',
          className
        )}
        {...props}
      >
        <div className={cn(detailLabelSizeVariant({ scale }))}>{label}</div>
        <div
          className={cn(
            detailValueSizeVariant({ scale }),
            multiline
              ? cn(
                  'whitespace-normal break-words text-left',
                  detailValueMultiLinePaddingVariant({ scale })
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
