import { useEventHandler } from '@/components/EventHandlerContext';
import { cn } from '@/lib/utils';
import { ChevronRight } from 'lucide-react';
import { inputStyles } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';

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

  return (
    <div className="relative">
      <button
        type="button"
        disabled={disabled}
        onClick={handleSelect}
        className={cn(
          'hover:bg-accent disabled:opacity-50 disabled:cursor-not-allowed flex h-9 text-left w-full items-center rounded-md border border-input bg-transparent text-base shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring md:text-sm cursor-pointer',
          invalid && inputStyles.invalid
        )}
      >
        {displayValue && (
          <span className="flex-grow text-primary font-semibold text-sm ml-3 underline">
            {displayValue}
          </span>
        )}
        {!displayValue && (
          <span className="flex-grow text-muted-foreground text-sm ml-3">
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
