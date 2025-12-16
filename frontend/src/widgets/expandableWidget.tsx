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
import { Switch } from '@/components/ui/switch';
import { useEventHandler } from '@/components/event-handler';

interface ExpandableWidgetProps {
  id: string;
  disabled?: boolean;
  open?: boolean;
  scale?: Scales;
  /** When true, shows a switch in the header that controls whether the expandable is enabled. */
  enableToggle?: boolean;
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
  enableToggle,
  slots,
}) => {
  const [isOpen, setIsOpen] = React.useState(open);
  const hasEnableToggle = enableToggle === true;
  const eventHandler = useEventHandler();

  React.useEffect(() => {
    setIsOpen(open);
  }, [open]);

  React.useEffect(() => {
    if (disabled && isOpen) {
      setIsOpen(false);
    }
  }, [disabled, isOpen]);

  const handleOpenChange = (newOpen: boolean) => {
    if (disabled) {
      return;
    }
    setIsOpen(newOpen);
  };

  const handleEnableToggleChange = (checked: boolean | null) => {
    eventHandler('OnEnableToggleChange', id, [!!checked]);
  };

  const renderEnableToggle = () => {
    if (!hasEnableToggle) return null;

    const toggleId = `${id}-enable-toggle`;

    return (
      <div onClick={e => e.stopPropagation()} className="mr-2">
        <Switch
          id={toggleId}
          checked={!disabled}
          onCheckedChange={handleEnableToggleChange}
          scale={scale}
        />
      </div>
    );
  };

  const handleTriggerClick = (e: React.MouseEvent) => {
    // If clicking on an interactive element, stop propagation so it doesn't toggle
    const target = e.target as HTMLElement;
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

    if (disabled) {
      e.preventDefault();
      e.stopPropagation();
    }
  };

  return (
    <Collapsible
      key={id}
      open={isOpen}
      onOpenChange={handleOpenChange}
      className={cn(
        'w-full rounded-md border border-border shadow-sm data-[disabled=true]:cursor-not-allowed',
        'p-0'
      )}
      data-disabled={disabled}
      role="details"
    >
      <CollapsibleTrigger
        aria-disabled={disabled}
        className={cn(
          expandableTriggerVariants({ scale }),
          'relative',
          disabled && 'cursor-not-allowed'
        )}
        onClick={handleTriggerClick}
        data-collapsible-trigger
      >
        <div
          className={cn(
            expandableHeaderVariants({ scale }),
            disabled && 'expandable-header-disabled',
            'flex items-center'
          )}
          role="summary"
        >
          {renderEnableToggle()}
          {slots?.Header}
        </div>
        <span
          className={cn(
            expandableChevronContainerVariants({ scale }),
            disabled && 'opacity-50'
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
