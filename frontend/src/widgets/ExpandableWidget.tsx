import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from '@/components/ui/collapsible';
import { ChevronRight } from 'lucide-react';
import React from 'react';

interface ExpandableWidgetProps {
  id: string;
  disabled?: boolean;
  open?: boolean;
  slots?: {
    Header: React.ReactNode;
    Content: React.ReactNode;
  };
}

export const ExpandableWidget: React.FC<ExpandableWidgetProps> = ({
  id,
  disabled,
  open = false,
  slots,
}) => {
  const [isOpen, setIsOpen] = React.useState(open);

  React.useEffect(() => {
    setIsOpen(open);
  }, [open]);

  React.useEffect(() => {
    if (disabled && isOpen) {
      setIsOpen(false);
    }
  }, [disabled, isOpen]);

  return (
    <Collapsible
      key={id}
      open={isOpen}
      onOpenChange={setIsOpen}
      className="w-full rounded-md border border-border p-2 shadow-sm data-[disabled=true]:cursor-not-allowed data-[disabled=true]:opacity-50"
      data-disabled={disabled}
      role="details"
    >
      <CollapsibleTrigger
        disabled={disabled}
        className="w-full flex justify-between items-center space-x-4 cursor-pointer hover:bg-accent/50 rounded-sm transition-colors data-[disabled=true]:cursor-not-allowed data-[disabled=true]:hover:bg-transparent"
      >
        <div className="flex-1 ml-2 min-w-0" role="summary">
          {slots?.Header}
        </div>
        <span
          className="p-0 h-9 w-9 shrink-0 pointer-events-none flex items-center justify-center"
          aria-hidden="true"
        >
          <ChevronRight
            className={`h-4 w-4 transition-transform duration-200 ease-in-out ${
              isOpen ? 'rotate-90' : 'rotate-0'
            }`}
          />
        </span>
      </CollapsibleTrigger>
      <CollapsibleContent className="overflow-hidden transition-all data-[state=closed]:animate-accordion-up data-[state=open]:animate-accordion-down">
        <div className="space-y-4 p-2">{slots?.Content}</div>
      </CollapsibleContent>
    </Collapsible>
  );
};
