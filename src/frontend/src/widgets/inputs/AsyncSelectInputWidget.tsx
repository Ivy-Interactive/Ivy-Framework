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
  'hover:bg-accent disabled:opacity-50 disabled:cursor-not-allowed flex text-left w-full items-center rounded-md border border-input bg-transparent shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring cursor-pointer relative dark:border-white/10',
  {
    variants: {
      scale: {
        Small: 'h-7 px-2 py-1 pr-7',
        Medium: 'h-9 px-3 py-2 pr-9',
        Large: 'h-11 px-4 py-3 pr-11',
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

const asyncSelectIconContainerVariants = {
  Small: 'w-7 right-0 px-2',
  Medium: 'w-8 right-0 px-2',
  Large: 'w-10 right-0 px-2',
};

const asyncSelectIconVariants = {
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
  scale?: Scales;
}

export const AsyncSelectInputWidget: React.FC<AsyncSelectInputWidgetProps> = ({
  id,
  placeholder,
  displayValue,
  disabled = false,
  invalid,
  loading,
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
        'grow overflow-hidden text-ellipsis whitespace-nowrap',
        asyncSelectTextVariants[scale],
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
          asyncSelectContainerVariants({ scale }),
          invalid && inputStyles.invalidInput
        )}
      >
        {wrappedDisplayValue}
        {!displayValue && (
          <span
            className={cn(
              'grow text-muted-foreground',
              asyncSelectTextVariants[scale]
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
            asyncSelectIconContainerVariants[scale]
          )}
        >
          <ChevronRight
            className={cn(
              'opacity-50 shrink-0',
              asyncSelectIconVariants[scale]
            )}
          />
        </div>
      </button>
    </div>
  );
};
