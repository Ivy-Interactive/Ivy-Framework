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
import { Scales } from '@/types/scale';
import { cva } from 'class-variance-authority';

const asyncSelectContainerVariants = cva(
  'hover:bg-accent disabled:opacity-50 disabled:cursor-not-allowed flex text-left w-full items-center rounded-md border border-input bg-transparent shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring cursor-pointer',
  {
    variants: {
      scale: {
        Small: 'h-7 px-2 py-1',
        Medium: 'h-9 px-3 py-2',
        Large: 'h-11 px-4 py-3',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);

const asyncSelectTextVariants = {
  Small: 'text-xs',
  Medium: 'text-sm',
  Large: 'text-base',
};

interface AsyncSelectInputWidgetProps {
  id: string;
  placeholder?: string;
  displayValue?: string;
  disabled: boolean;
  loading: boolean;
  invalid?: string;
  scale?: Scales;
}

export const AsyncSelectInputWidget: React.FC<AsyncSelectInputWidgetProps> = ({
  id,
  placeholder,
  displayValue,
  disabled,
  invalid,
  scale = Scales.Medium,
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
        'grow text-primary font-semibold ml-3 underline overflow-hidden text-ellipsis whitespace-nowrap',
        asyncSelectTextVariants[scale]
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
    <div className="relative">
      <button
        type="button"
        disabled={disabled}
        onClick={handleSelect}
        className={cn(
          asyncSelectContainerVariants({ scale }),
          invalid && inputStyles.invalidInput
        )}
      >
        {wrappedDisplayValue}
        {!displayValue && (
          <span
            className={cn(
              'grow text-muted-foreground ml-3',
              asyncSelectTextVariants[scale]
            )}
          >
            {placeholder}
          </span>
        )}
        <div
          className={cn(
            'flex items-center justify-center h-full border-l',
            scale === Scales.Small
              ? 'w-7'
              : scale === Scales.Large
                ? 'w-11'
                : 'w-9'
          )}
        >
          <ChevronRight
            className={cn(
              scale === Scales.Small
                ? 'h-3 w-3'
                : scale === Scales.Large
                  ? 'h-5 w-5'
                  : 'h-4 w-4'
            )}
          />
        </div>
      </button>
      {invalid && (
        <div
          className={cn(
            'absolute h-4 w-4',
            scale === Scales.Small
              ? 'right-7 top-1.5'
              : scale === Scales.Large
                ? 'right-11 top-3.5'
                : 'right-11 top-2.5'
          )}
        >
          <InvalidIcon message={invalid} />
        </div>
      )}
    </div>
  );
};
