import React from 'react';
import { Sizes } from '@/types/sizes';
import { getWidth, getHeight } from '@/lib/styles';
import Icon from '@/components/Icon';
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from '@/components/ui/tooltip';

interface FieldWidgetProps {
  id: string;
  label: string;
  description?: string;
  required: boolean;
  help?: string;
  children?: React.ReactNode;
  size?: Sizes;
  width?: string;
  height?: string;
}

export const FieldWidget: React.FC<FieldWidgetProps> = ({
  label,
  description,
  required,
  help,
  children,
  size = Sizes.Medium,
  width,
  height,
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

  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  const flexClass = width || height ? '' : 'flex-1';

  return (
    <div
      className={`flex flex-col ${gapClass} ${flexClass} min-w-0`}
      style={styles}
    >
      {label && (
        <div className="flex items-center gap-1.5">
          <label
            className={`${labelSizeClass} font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70`}
          >
            {label}{' '}
            {required && <span className="font-mono text-primary">*</span>}
          </label>
          {help && (
            <TooltipProvider>
              <Tooltip>
                <TooltipTrigger asChild>
                  <button
                    type="button"
                    aria-label="Help"
                    className="inline-flex items-center justify-center focus:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 rounded-sm"
                  >
                    <Icon
                      name="Info"
                      size="14"
                      className="text-muted-foreground hover:text-foreground transition-colors"
                    />
                  </button>
                </TooltipTrigger>
                <TooltipContent className="bg-popover text-popover-foreground shadow-md">
                  {help}
                </TooltipContent>
              </Tooltip>
            </TooltipProvider>
          )}
        </div>
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
