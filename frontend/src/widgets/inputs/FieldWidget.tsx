import React from 'react';
import { Sizes } from '@/types/sizes';

interface FieldWidgetProps {
  id: string;
  label: string;
  description?: string;
  required: boolean;
  children?: React.ReactNode;
  size?: Sizes;
}

export const FieldWidget: React.FC<FieldWidgetProps> = ({
  label,
  description,
  required,
  children,
  size = Sizes.Medium,
}) => {
  const labelSizeClass =
    size === Sizes.Small
      ? 'text-xs'
      : size === Sizes.Large
        ? 'text-base'
        : 'text-sm';
  const descriptionSizeClass =
    size === Sizes.Small
      ? 'text-xs'
      : size === Sizes.Large
        ? 'text-sm'
        : 'text-xs';

  const gapClass =
    size === Sizes.Small ? 'gap-2' : size === Sizes.Large ? 'gap-4' : 'gap-3';

  return (
    <div className={`flex flex-col ${gapClass} flex-1 min-w-0`}>
      {label && (
        <label
          className={`${labelSizeClass} font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70`}
        >
          {label}{' '}
          {required && <span className="font-mono text-primary">*</span>}
        </label>
      )}
      {children}
      {description && (
        <p className={`${descriptionSizeClass} text-muted-foreground`}>
          {description}
        </p>
      )}
    </div>
  );
};
