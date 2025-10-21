import React from 'react';
import { useFormSize } from '@/widgets/forms/FormWidget';
import { Sizes } from '@/types/sizes';
import { cn } from '@/lib/utils';

interface FieldWidgetProps {
  id: string;
  label: string;
  description?: string;
  required: boolean;
  children?: React.ReactNode;
}

export const FieldWidget: React.FC<FieldWidgetProps> = ({
  label,
  description,
  required,
  children,
}) => {
  const formSize = useFormSize();

  // Determine gap spacing based on form size
  const getGapClass = (size: Sizes) => {
    switch (size) {
      case Sizes.Small:
        return 'gap-1'; // 4px - very tight spacing
      case Sizes.Large:
        return 'gap-6'; // 24px - very generous spacing
      default:
        return 'gap-3'; // 12px - moderate spacing
    }
  };

  return (
    <div className={cn('flex flex-col flex-1 min-w-0', getGapClass(formSize))}>
      {label && (
        <label className="font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70">
          {label}{' '}
          {required && <span className="font-mono text-primary">*</span>}
        </label>
      )}
      {children}
      {description && <p className="text-muted-foreground">{description}</p>}
    </div>
  );
};
