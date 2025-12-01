import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from '@/components/ui/collapsible';
import {
  expandableTriggerVariants,
  expandableHeaderVariants,
  expandableChevronContainerVariants,
  expandableChevronVariants,
  expandableContentVariants,
} from '@/components/ui/expandable/expandable-variants';
import { ChevronRight } from 'lucide-react';
import React from 'react';
import { Scales } from '@/types/scale';
import { cn } from '@/lib/utils';

interface ExpandableWidgetProps {
  id: string;
  disabled?: boolean;
  open?: boolean;
  scale?: Scales;
  slots?: {
    Header: React.ReactNode;
    Content: React.ReactNode;
  };
}

export const ExpandableWidget: React.FC<ExpandableWidgetProps> = ({
  id,
  disabled,
  open = false,
  scale = Scales.Medium,
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
      className={cn(
        'w-full rounded-md border border-border shadow-sm data-[disabled=true]:cursor-not-allowed data-[disabled=true]:opacity-50',
        'p-0'
      )}
      data-disabled={disabled}
      role="details"
    >
      <CollapsibleTrigger
        disabled={disabled}
        className={cn(expandableTriggerVariants({ scale }), 'relative')}
      >
        <div className={expandableHeaderVariants({ scale })} role="summary">
          {slots?.Header}
        </div>
        <span
          className={expandableChevronContainerVariants({ scale })}
          aria-hidden="true"
        >
          <ChevronRight
            className={cn(
              expandableChevronVariants({ scale }),
              isOpen ? 'rotate-90' : 'rotate-0'
            )}
          />
        </span>
      </CollapsibleTrigger>
      <CollapsibleContent className="overflow-hidden transition-all data-[state=closed]:animate-accordion-up data-[state=open]:animate-accordion-down">
        <div className={expandableContentVariants({ scale })}>
          {slots?.Content}
        </div>
      </CollapsibleContent>
    </Collapsible>
  );
};
