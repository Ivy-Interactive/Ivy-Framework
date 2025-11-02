import { useEventHandler } from '@/components/event-handler';
import { cn } from '@/lib/utils';
import { ChevronRight } from 'lucide-react';
import { inputStyles } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from '@/components/ui/tooltip';
import { useState, useRef } from 'react';

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
  const textSpanRef = useRef<HTMLSpanElement>(null);
  const [showTooltip, setShowTooltip] = useState(false);

  const checkIsTruncated = (): boolean => {
    if (!textSpanRef.current) return false;
    return textSpanRef.current.scrollWidth > textSpanRef.current.clientWidth;
  };

  const handleSelect = () => {
    eventHandler('OnSelect', id, []);
  };

  const tooltipText = displayValue || placeholder;
  const hasContent = !!tooltipText;

  return (
    <div className="relative">
      <TooltipProvider>
        <Tooltip open={showTooltip && hasContent} onOpenChange={setShowTooltip}>
          <TooltipTrigger asChild>
            <button
              type="button"
              disabled={disabled}
              onClick={handleSelect}
              className={cn(
                'hover:bg-accent disabled:opacity-50 disabled:cursor-not-allowed flex h-9 text-left w-full items-center rounded-md border border-input bg-background text-base shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring cursor-pointer',
                invalid && inputStyles.invalidInput
              )}
              onMouseEnter={() => {
                const isTruncated = checkIsTruncated();
                setShowTooltip(isTruncated);
              }}
              onMouseLeave={() => setShowTooltip(false)}
            >
              {displayValue && (
                <span
                  ref={textSpanRef}
                  className="grow text-primary font-semibold text-body ml-3 underline truncate min-w-0 mr-2"
                >
                  {displayValue}
                </span>
              )}
              {!displayValue && (
                <span
                  ref={textSpanRef}
                  className="grow text-muted-foreground text-body ml-3 truncate min-w-0 mr-2"
                >
                  {placeholder}
                </span>
              )}
              <div className="flex items-center justify-center h-full w-9 border-l shrink-0">
                <ChevronRight className="h-4 w-4" />
              </div>
            </button>
          </TooltipTrigger>
          {tooltipText && (
            <TooltipContent className="bg-popover text-popover-foreground shadow-md max-w-sm">
              <div className="whitespace-pre-wrap wrap-break-word">
                {tooltipText}
              </div>
            </TooltipContent>
          )}
        </Tooltip>
      </TooltipProvider>
      {invalid && (
        <div className="absolute right-11 top-2.5 h-4 w-4">
          <InvalidIcon message={invalid} />
        </div>
      )}
    </div>
  );
};
