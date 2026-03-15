import React from 'react';
import { Densities } from '@/types/density';
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
  density?: Densities;
  width?: string;
  height?: string;
  labelPosition?: 'Top' | 'Left' | 0 | 1;
}

export const FieldWidget: React.FC<FieldWidgetProps> = ({
  label,
  description,
  required,
  help,
  children,
  density = Densities.Medium,
  width,
  height,
  labelPosition,
}) => {
  const labelSizeClass =
    density === Densities.Small
      ? 'text-xs'
      : density === Densities.Large
        ? 'text-base'
        : 'text-sm';
  const descriptionSizeClass =
    density === Densities.Small
      ? 'text-xs'
      : density === Densities.Large
        ? 'text-sm'
        : 'text-xs';

  const gapClass =
    density === Densities.Small
      ? 'gap-1'
      : density === Densities.Large
        ? 'gap-3'
        : 'gap-2';

  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  const flexClass = width || height ? '' : 'flex-1';

  const isLeft = labelPosition === 'Left' || labelPosition === 1;

  if (isLeft) {
    return (
      <div
        className={`flex flex-col sm:flex-row ${gapClass} ${flexClass} min-w-0`}
        style={styles}
      >
        {label && (
          <div className="flex items-center gap-1.5 min-w-[120px] w-1/4 sm:w-1/3 pt-2 sm:pt-0 sm:mt-2.5 self-start">
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
        <div className="flex-1 flex flex-col gap-2 min-w-0">
          {children}
          {description && (
            <p className={`${descriptionSizeClass} text-muted-foreground`}>
              {description}
            </p>
          )}
        </div>
      </div>
    );
  }

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
