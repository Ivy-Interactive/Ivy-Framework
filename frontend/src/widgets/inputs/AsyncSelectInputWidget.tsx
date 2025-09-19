import { useEventHandler } from '@/components/event-handler';
import { cn } from '@/lib/utils';
import { ChevronRight } from 'lucide-react';
import { inputStyles } from '@/lib/styles';
import { InvalidIcon } from '@/components/InvalidIcon';

interface AsyncSelectInputWidgetProps {
  id: string;
  label?: string;
  description?: string;
  placeholder?: string;
  displayValue?: string;
  disabled: boolean;
  loading: boolean;
  invalid?: string;
}

export const AsyncSelectInputWidget: React.FC<AsyncSelectInputWidgetProps> = ({
  id,
  label,
  description,
  placeholder,
  displayValue,
  disabled,
  invalid,
}) => {
  const eventHandler = useEventHandler();

  const handleSelect = () => {
    eventHandler('OnSelect', id, []);
  };

  const asyncSelectElement = (
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
        {displayValue && (
          <span className="flex-grow text-primary font-semibold text-body ml-3 underline">
            {displayValue}
          </span>
        )}
        {!displayValue && (
          <span className="flex-grow text-muted-foreground text-body ml-3">
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

  // If no label or description, return just the async select input
  if (!label && !description) {
    return asyncSelectElement;
  }

  // Otherwise, wrap with label and description structure
  return (
    <div className="flex flex-col gap-2 flex-1 min-w-0">
      {label && (
        <label className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70">
          {label}
        </label>
      )}
      {asyncSelectElement}
      {description && (
        <p className="text-sm text-muted-foreground">{description}</p>
      )}
    </div>
  );
};
