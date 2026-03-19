import React from 'react';
import { useEventHandler } from '@/components/event-handler';
import { cn } from '@/lib/utils';
import { ChevronRight } from 'lucide-react';
import { inputStyles } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';
import {
  Tooltip,
  TooltipProvider,
  TooltipTrigger,
  TooltipContent,
} from '@/components/ui/tooltip';
import { useRef, useEffect, useState } from 'react';
import { Densities } from '@/types/density';
import { cva } from 'class-variance-authority';

const asyncSelectContainerVariant = cva(
  'hover:bg-accent disabled:opacity-50 disabled:cursor-not-allowed flex text-left w-full items-center rounded-field border border-input bg-transparent shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring cursor-pointer relative dark:border-white/10',
  {
    variants: {
      density: {
        Small: 'h-7 px-2 py-1 pr-7',
        Medium: 'h-9 px-3 py-2 pr-9',
        Large: 'h-11 px-4 py-3 pr-11',
      },
    },
    defaultVariants: {
      density: 'Medium',
    },
  }
);

const asyncSelectTextVariant = {
  Small: 'text-xs',
  Medium: 'text-sm',
  Large: 'text-base',
};

const asyncSelectIconContainerVariant = {
  Small: 'w-7 right-0 px-2',
  Medium: 'w-8 right-0 px-2',
  Large: 'w-10 right-0 px-2',
};

const asyncSelectIconVariant = {
  Small: 'h-3 w-3',
  Medium: 'h-4 w-4',
  Large: 'h-5 w-5',
};

interface AsyncSelectInputWidgetProps {
  id: string;
  placeholder?: string;
  displayValue?: string;
  disabled?: boolean;
  loading?: boolean;
  invalid?: string;
  density?: Densities;
  ghost?: boolean;
}

export const AsyncSelectInputWidget: React.FC<AsyncSelectInputWidgetProps> = ({
  id,
  placeholder,
  displayValue,
  disabled = false,
  invalid,
  loading,
  density = Densities.Medium,
  ghost = false,
}) => {
  const eventHandler = useEventHandler();

  const handleSelect = () => {
    eventHandler('OnSelect', id, []);
  };

  // Create ref for the display value span
  const displayValueRef = useRef<HTMLSpanElement>(null);

  // Detect ellipsis on the display value span
  const [isEllipsed, setIsEllipsed] = useState(false);

  useEffect(() => {
    // Skip ellipsis check when no display value
    if (!displayValue) {
      requestAnimationFrame(() => setIsEllipsed(false));
      return;
    }

    const checkEllipsis = () => {
      if (!displayValueRef?.current) {
        return;
      }
      setIsEllipsed(
        displayValueRef.current.scrollWidth >
          displayValueRef.current.clientWidth
      );
    };

    // Check after render
    requestAnimationFrame(checkEllipsis);

    // Debounced resize handler
    let resizeTimeout: NodeJS.Timeout;
    const handleResize = () => {
      clearTimeout(resizeTimeout);
      resizeTimeout = setTimeout(checkEllipsis, 150);
    };
    window.addEventListener('resize', handleResize);

    return () => {
      clearTimeout(resizeTimeout);
      window.removeEventListener('resize', handleResize);
    };
  }, [displayValue]);

  const displayValueSpan = displayValue ? (
    <span
      ref={displayValueRef}
      className={cn(
        'grow overflow-hidden text-ellipsis whitespace-nowrap',
        asyncSelectTextVariant[density],
        !loading && 'text-primary font-semibold underline',
        loading && 'text-muted-foreground'
      )}
    >
      {displayValue}
    </span>
  ) : null;

  // Wrap display value span with tooltip if ellipsed
  const shouldShowTooltip = isEllipsed && displayValue;
  const wrappedDisplayValue = shouldShowTooltip ? (
    <TooltipProvider>
      <Tooltip delayDuration={300}>
        <TooltipTrigger asChild>{displayValueSpan}</TooltipTrigger>
        <TooltipContent className="bg-popover text-popover-foreground shadow-md max-w-sm">
          <div className="whitespace-pre-wrap wrap-break-word">
            {displayValue}
          </div>
        </TooltipContent>
      </Tooltip>
    </TooltipProvider>
  ) : (
    displayValueSpan
  );

  return (
    <div>
      <button
        type="button"
        disabled={disabled}
        onClick={handleSelect}
        className={cn(
          asyncSelectContainerVariant({ density }),
          invalid && inputStyles.invalidInput,
          ghost &&
            'border-transparent shadow-none bg-transparent dark:border-transparent dark:bg-transparent'
        )}
      >
        {wrappedDisplayValue}
        {!displayValue && (
          <span
            className={cn(
              'grow text-muted-foreground',
              asyncSelectTextVariant[density]
            )}
          >
            {placeholder}
          </span>
        )}
        {invalid && (
          <div className="flex items-center shrink-0 ml-2 mr-2">
            <InvalidIcon message={invalid} />
          </div>
        )}
        <div
          className={cn(
            'absolute top-0 bottom-0 border-l flex items-center justify-center',
            asyncSelectIconContainerVariant[density]
          )}
        >
          <ChevronRight
            className={cn(
              'opacity-50 shrink-0',
              asyncSelectIconVariant[density]
            )}
          />
        </div>
      </button>
    </div>
  );
};
