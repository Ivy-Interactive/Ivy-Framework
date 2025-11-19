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

interface AsyncSelectInputWidgetProps {
  id: string;
  placeholder?: string;
  displayValue?: string;
  disabled: boolean;
  loading: boolean;
  invalid?: string;
}

export const AsyncSelectInputWidget: React.FC<AsyncSelectInputWidgetProps> = ({
  id,
  placeholder,
  displayValue,
  disabled,
  invalid,
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
      className="grow text-primary font-semibold text-body ml-3 underline overflow-hidden text-ellipsis whitespace-nowrap"
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
          'hover:bg-accent disabled:opacity-50 disabled:cursor-not-allowed flex h-9 text-left w-full items-center rounded-md border border-input bg-background text-base shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring cursor-pointer',
          invalid && inputStyles.invalidInput
        )}
      >
        {wrappedDisplayValue}
        {!displayValue && (
          <span className="grow text-muted-foreground text-body ml-3">
            {placeholder}
          </span>
        )}
        <div className="flex items-center justify-center h-full w-9 border-l">
          <ChevronRight className="h-4 w-4" />
        </div>
      </button>
      {invalid && (
        <div className="absolute right-11 top-2.5 h-4 w-4">
          <InvalidIcon message={invalid} />
        </div>
      )}
    </div>
  );
};
