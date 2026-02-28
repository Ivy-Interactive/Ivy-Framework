import React from 'react';
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from '@/components/ui/collapsible';
import { ChevronRight } from 'lucide-react';
import Icon from '@/components/Icon';
import { cn } from '@/lib/utils';
import { MenuItem } from '@/types/widgets';

interface TreeItemWidgetProps {
  item: MenuItem;
  onItemClick: (item: MenuItem) => void;
}

export const TreeItem: React.FC<TreeItemWidgetProps> = ({
  item,
  onItemClick,
}) => {
  const [isOpen, setIsOpen] = React.useState(item.expanded ?? false);
  const hasChildren = item.children && item.children.length > 0;

  React.useEffect(() => {
    setIsOpen(item.expanded ?? false);
  }, [item.expanded]);

  const handleToggle = (e: React.MouseEvent) => {
    if (item.disabled) return;
    e.stopPropagation();
    setIsOpen(prev => !prev);
  };

  const handleClick = (e: React.MouseEvent) => {
    if (item.disabled) return;
    e.stopPropagation();
    if (hasChildren) {
      setIsOpen(prev => !prev);
    }
    onItemClick(item);
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (item.disabled) return;
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault();
      if (hasChildren) {
        setIsOpen(prev => !prev);
      } else {
        onItemClick(item);
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
        onOpenChange={val => !item.disabled && setIsOpen(val)}
      >
        <div
          className={cn(
            'ivy-tree-item group flex items-center gap-1 rounded-sm py-1 px-1 text-sm cursor-pointer select-none',
            'hover:bg-accent/50 transition-colors',
            item.disabled && 'opacity-50 cursor-not-allowed'
          )}
          role="treeitem"
          aria-expanded={isOpen}
          aria-disabled={item.disabled}
          tabIndex={item.disabled ? -1 : 0}
          onKeyDown={handleKeyDown}
          onClick={handleClick}
        >
          <CollapsibleTrigger asChild>
            <button
              className="flex items-center justify-center h-5 w-5 shrink-0 rounded-sm hover:bg-accent transition-colors"
              onClick={handleToggle}
              tabIndex={-1}
              disabled={item.disabled}
            >
              <ChevronRight
                className={cn(
                  'h-3.5 w-3.5 text-muted-foreground transition-transform duration-200',
                  isOpen && 'rotate-90'
                )}
              />
            </button>
          </CollapsibleTrigger>
          {item.icon && item.icon !== 'None' && (
            <Icon
              className="h-4 w-4 shrink-0 text-muted-foreground"
              name={item.icon}
            />
          )}
          <span className="truncate">{item.label}</span>
        </div>
        <CollapsibleContent className="overflow-hidden transition-all data-[state=closed]:animate-accordion-up data-[state=open]:animate-accordion-down">
          <div className="ivy-tree-children pl-3 ml-2 border-l border-border/50">
            {item.children!.map((child, index) => (
              <TreeItem
                key={`${child.label}-${index}`}
                item={child}
                onItemClick={onItemClick}
              />
            ))}
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
        item.disabled && 'opacity-50 cursor-not-allowed'
      )}
      role="treeitem"
      aria-disabled={item.disabled}
      tabIndex={item.disabled ? -1 : 0}
      onKeyDown={handleKeyDown}
      onClick={handleClick}
    >
      {/* Spacer matching chevron width to align leaf nodes with parent nodes */}
      <span className="h-5 w-5 shrink-0" />
      {item.icon && item.icon !== 'None' && (
        <Icon
          className="h-4 w-4 shrink-0 text-muted-foreground"
          name={item.icon}
        />
      )}
      <span className="truncate">{item.label}</span>
    </div>
  );
};
