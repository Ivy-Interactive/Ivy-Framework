import React from 'react';
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from '@/components/ui/collapsible';
import { ChevronRight } from 'lucide-react';
import { useEventHandler } from '@/components/event-handler';
import Icon from '@/components/Icon';
import { cn } from '@/lib/utils';
import { TreeContext } from './TreeWidget';

interface TreeItemWidgetProps {
  id: string;
  label?: string;
  icon?: string;
  open?: boolean;
  disabled?: boolean;
  children?: React.ReactNode;
}

export const TreeItemWidget: React.FC<TreeItemWidgetProps> = ({
  id,
  label,
  icon,
  open = false,
  disabled = false,
  children,
}) => {
  const [isOpen, setIsOpen] = React.useState(open);
  const eventHandler = useEventHandler();
  const hasChildren = React.Children.count(children) > 0;
  const { showLines } = React.useContext(TreeContext);

  React.useEffect(() => {
    setIsOpen(open);
  }, [open]);

  const handleToggle = (e: React.MouseEvent) => {
    if (disabled) return;
    e.stopPropagation();
    setIsOpen(prev => !prev);
  };

  const handleClick = (e: React.MouseEvent) => {
    if (disabled) return;
    e.stopPropagation();
    eventHandler('OnClick', id, []);
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (disabled) return;
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault();
      if (hasChildren) {
        setIsOpen(prev => !prev);
      } else {
        eventHandler('OnClick', id, []);
      }
    }
    if (e.key === 'ArrowRight' && hasChildren && !isOpen) {
      e.preventDefault();
      setIsOpen(true);
    }
    if (e.key === 'ArrowLeft' && hasChildren && isOpen) {
      e.preventDefault();
      setIsOpen(false);
    }
  };

  if (hasChildren) {
    return (
      <Collapsible
        open={isOpen}
        onOpenChange={val => !disabled && setIsOpen(val)}
      >
        <div
          className={cn(
            'ivy-tree-item group flex items-center gap-1 rounded-sm py-1 px-1 text-sm cursor-pointer select-none',
            'hover:bg-accent/50 transition-colors',
            disabled && 'opacity-50 cursor-not-allowed'
          )}
          role="treeitem"
          aria-expanded={isOpen}
          aria-disabled={disabled}
          tabIndex={disabled ? -1 : 0}
          onKeyDown={handleKeyDown}
          onClick={handleClick}
        >
          <CollapsibleTrigger asChild>
            <button
              className="flex items-center justify-center h-5 w-5 shrink-0 rounded-sm hover:bg-accent transition-colors"
              onClick={handleToggle}
              tabIndex={-1}
              disabled={disabled}
            >
              <ChevronRight
                className={cn(
                  'h-3.5 w-3.5 text-muted-foreground transition-transform duration-200',
                  isOpen && 'rotate-90'
                )}
              />
            </button>
          </CollapsibleTrigger>
          {icon && icon !== 'None' && (
            <Icon
              className="h-4 w-4 shrink-0 text-muted-foreground"
              name={icon}
            />
          )}
          <span className="truncate">{label}</span>
        </div>
        <CollapsibleContent className="overflow-hidden transition-all data-[state=closed]:animate-accordion-up data-[state=open]:animate-accordion-down">
          <div
            className={cn(
              'ivy-tree-children pl-3 ml-2',
              showLines && 'border-l border-border/50'
            )}
          >
            {children}
          </div>
        </CollapsibleContent>
      </Collapsible>
    );
  }

  return (
    <div
      className={cn(
        'ivy-tree-item flex items-center gap-1 rounded-sm py-1 px-1 text-sm cursor-pointer select-none',
        'hover:bg-accent/50 transition-colors',
        disabled && 'opacity-50 cursor-not-allowed'
      )}
      role="treeitem"
      aria-disabled={disabled}
      tabIndex={disabled ? -1 : 0}
      onKeyDown={handleKeyDown}
      onClick={handleClick}
    >
      {/* Spacer matching chevron width to align leaf nodes with parent nodes */}
      <span className="h-5 w-5 shrink-0" />
      {icon && icon !== 'None' && (
        <Icon className="h-4 w-4 shrink-0 text-muted-foreground" name={icon} />
      )}
      <span className="truncate">{label}</span>
    </div>
  );
};
