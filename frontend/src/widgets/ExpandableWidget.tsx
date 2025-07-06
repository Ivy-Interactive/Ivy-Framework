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
    Header: React.ReactNode[];
    Content: React.ReactNode[];
  };
}

export const ExpandableWidget: React.FC<ExpandableWidgetProps> = ({
  id,
  disabled,
  slots,
}) => {
  const [isOpen, setIsOpen] = React.useState(false);

  if (disabled && isOpen) {
    setIsOpen(false);
  }

  return (
    <Collapsible
      key={id}
      open={isOpen}
      onOpenChange={setIsOpen}
      className="w-full space-y-2 rounded-md border border-gray-200 p-2 shadow-sm"
    >
      <div className="flex justify-between space-x-4">
        <CollapsibleTrigger asChild>
          <Button variant="ghost" className="w-full p-0">
            <div className="ml-2">{slots?.Header}</div>
            {!isOpen && <ChevronRight className="h-4 w-4 ml-auto mr-2" />}
            {isOpen && <ChevronDown className="h-4 w-4 ml-auto mr-2" />}
          </Button>
        </CollapsibleTrigger>
      </div>
      <CollapsibleContent className="space-y-2 p-2">
        {slots?.Content}
      </CollapsibleContent>
    </Collapsible>
  );
};
