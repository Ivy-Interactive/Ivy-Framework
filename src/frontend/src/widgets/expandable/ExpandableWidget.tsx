import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from '@/components/ui/collapsible';
import {
  expandableTriggerVariant,
  expandableHeaderVariant,
  expandableChevronContainerVariant,
  expandableChevronVariant,
  expandableContentVariant,
} from '@/components/ui/expandable/expandable-variant';
import { ChevronRight } from 'lucide-react';
import React from 'react';
import Icon from '@/components/Icon';
import { Scales } from '@/types/scale';
import { cn } from '@/lib/utils';

interface ExpandableWidgetProps {
  id: string;
  disabled?: boolean;
  open?: boolean;
  scale?: Scales;
  icon?: string;
  slots?: {
    Header: React.ReactNode;
    Content: React.ReactNode;
  };
}

export const ExpandableWidget: React.FC<ExpandableWidgetProps> = ({
  id,
  disabled = false,
  open = false,
  scale = Scales.Medium,
  icon = undefined,
  slots,
}) => {
  let iconSize: number = 4;

  switch (scale) {
    case Scales.Small:
      iconSize = 3;
      break;
    case Scales.Large:
      iconSize = 5;
      break;
    default:
      break;
  }

  const iconStyles: React.CSSProperties = {
    width: `${iconSize * 0.25}rem`,
    height: `${iconSize * 0.25}rem`,
    flexShrink: 0,
  };

  const [isOpen, setIsOpen] = React.useState(open);

  React.useEffect(() => {
    setIsOpen(open);
  }, [open]);

  React.useEffect(() => {
    if (disabled && isOpen) {
      setIsOpen(false);
    }
  }, [disabled, isOpen]);

  const handleOpenChange = (newOpen: boolean) => {
    // Prevent toggle if disabled
    if (disabled) {
      return;
    }
    setIsOpen(newOpen);
  };

  const handleTriggerClick = (e: React.MouseEvent) => {
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
        'w-full rounded-box border border-border shadow-sm data-[disabled=true]:cursor-not-allowed',
        'p-0'
      )}
      data-disabled={disabled}
      role="details"
    >
      <CollapsibleTrigger asChild>
        <div
          className={cn(
            expandableTriggerVariant({ scale }),
            'relative cursor-pointer data-[disabled=true]:cursor-not-allowed'
          )}
          onClick={handleTriggerClick}
          data-collapsible-trigger
          data-disabled={disabled}
          role="button"
          tabIndex={disabled ? -1 : 0}
          onKeyDown={e => {
            if (e.key === 'Enter' || e.key === ' ') {
              e.preventDefault();
              if (!disabled) setIsOpen(prev => !prev);
            }
          }}
        >
          <div
            className={cn(
              expandableHeaderVariant({ scale }),
              disabled && 'text-muted-foreground',
              'flex items-center gap-2'
            )}
            role="summary"
          >
            {icon && icon !== 'None' && <Icon style={iconStyles} name={icon} />}
            {slots?.Header}
          </div>
          <span
            className={cn(
              expandableChevronContainerVariant({ scale }),
              disabled && 'opacity-50'
            )}
            aria-hidden="true"
          >
            <ChevronRight
              className={cn(
                expandableChevronVariant({ scale }),
                isOpen ? 'rotate-90' : 'rotate-0'
              )}
            />
          </span>
        </div>
      </CollapsibleTrigger>
      <CollapsibleContent className="overflow-hidden transition-all data-[state=closed]:animate-accordion-up data-[state=open]:animate-accordion-down">
        <div className={expandableContentVariant({ scale })}>
          {slots?.Content}
        </div>
      </CollapsibleContent>
    </Collapsible>
  );
};
