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
import { Switch } from '@/components/ui/switch';
import { ChevronRight } from 'lucide-react';
import React from 'react';
import { Scales } from '@/types/scale';
import { cn } from '@/lib/utils';

interface ExpandableWidgetProps {
  disabled?: boolean;
  toggleable?: boolean;
  open?: boolean;
  scale?: Scales;
  slots?: {
    Header: React.ReactNode;
    Content: React.ReactNode;
  };
}

export const ExpandableWidget: React.FC<ExpandableWidgetProps> = ({
  disabled = false,
  toggleable = false,
  open = false,
  scale = Scales.Medium,
  slots,
}) => {
  const [isOpen, setIsOpen] = React.useState(open);
  const [toggledOn, setToggledOn] = React.useState(false);

  const isDisabled = toggleable ? !toggledOn : disabled;

  React.useEffect(() => {
    setIsOpen(open);
  }, [open]);

  React.useEffect(() => {
    if (isDisabled && isOpen) {
      setIsOpen(false);
    }
  }, [isDisabled, isOpen]);

  const handleOpenChange = (newOpen: boolean) => {
    if (isDisabled) {
      return;
    }
    setIsOpen(newOpen);
  };

  const handleToggleSwitch = (checked: boolean) => {
    setToggledOn(checked);
  };

  const handleTriggerClick = (e: React.MouseEvent) => {
    const target = e.target as HTMLElement;

    // Check for interactive elements in the header (like in main)
    const isInteractiveElement =
      target.closest('button:not([data-collapsible-trigger])') ||
      target.closest('input') ||
      target.closest('select') ||
      target.closest('[role="button"]:not([data-collapsible-trigger])') ||
      target.closest('[role="switch"]') ||
      target.closest('[role="checkbox"]') ||
      target.closest('a[href]');

    if (isInteractiveElement) {
      e.stopPropagation();
      return;
    }

    if (isDisabled) {
      e.preventDefault();
      e.stopPropagation();
    }
  };

  return (
    <Collapsible
      open={isOpen}
      onOpenChange={handleOpenChange}
      className={cn(
        'w-full rounded-md border border-border shadow-sm p-0',
        isDisabled && 'cursor-not-allowed'
      )}
      data-disabled={isDisabled}
      role="details"
    >
      <CollapsibleTrigger
        className={cn(
          expandableTriggerVariants({ scale }),
          'relative',
          isDisabled && 'cursor-not-allowed'
        )}
        onClick={handleTriggerClick}
        data-collapsible-trigger
      >
        {toggleable && (
          <div
            className="flex items-center mr-2"
            onClick={e => e.stopPropagation()}
          >
            <Switch
              checked={!isDisabled}
              onCheckedChange={handleToggleSwitch}
              scale={scale}
            />
          </div>
        )}
        <div
          className={cn(
            expandableHeaderVariants({ scale }),
            isDisabled &&
              '[&>*:not(:has([role=switch],[role=checkbox],input))]:opacity-50',
            isDisabled &&
              '[&_[role=switch]]:pointer-events-none [&_[role=switch]]:opacity-50'
          )}
          role="summary"
        >
          {slots?.Header}
        </div>
        <span
          className={cn(
            expandableChevronContainerVariants({ scale }),
            isDisabled && 'opacity-50'
          )}
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
