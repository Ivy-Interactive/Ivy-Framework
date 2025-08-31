import { Button } from '@/components/ui/button';
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from '@/components/ui/collapsible';
import { ChevronDown, ChevronRight } from 'lucide-react';
import React from 'react';

interface ExpandableWidgetProps {
  id: string;
  disabled?: boolean;
  slots?: {
    Header: React.ReactNode;
    Content: React.ReactNode;
  };
}

export const ExpandableWidget: React.FC<ExpandableWidgetProps> = ({
  id,
  disabled,
  slots,
}) => {
  const [isOpen, setIsOpen] = React.useState(false);

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
    >
      <div className="flex justify-between items-center space-x-4">
        <div className="flex-1 ml-2 min-w-0">{slots?.Header}</div>
        <CollapsibleTrigger asChild disabled={disabled}>
          <Button
            variant="ghost"
            className="p-0 h-9 w-9 shrink-0 hover:bg-accent"
          >
            {!isOpen && <ChevronRight className="h-4 w-4" />}
            {isOpen && <ChevronDown className="h-4 w-4" />}
          </Button>
        </CollapsibleTrigger>
      </div>
      <CollapsibleContent className="space-y-4 p-2">
        {slots?.Content}
      </CollapsibleContent>
    </Collapsible>
  );
};
