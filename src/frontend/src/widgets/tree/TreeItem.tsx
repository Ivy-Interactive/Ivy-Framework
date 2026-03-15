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
import { ActionRenderer } from '../dataTables/dataTableRowAction/actionRenderer';

interface TreeItemWidgetProps {
  item: MenuItem;
  rowActions?: MenuItem[];
  hasSiblingWithChildren?: boolean;
  onItemClick: (item: MenuItem) => void;
  onRowActionClick?: (item: MenuItem, action: MenuItem) => void;
}

export const TreeItem: React.FC<TreeItemWidgetProps> = ({
  item,
  rowActions,
  hasSiblingWithChildren,
  onItemClick,
  onRowActionClick,
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
            'ivy-tree-item group flex items-center gap-1 flex-1 rounded-sm py-1 px-1 text-sm cursor-pointer select-none outline-none',
            'hover:bg-accent/50 transition-colors focus-visible:ring-1 focus-visible:ring-ring',
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
          <span className="truncate flex-1">{item.label}</span>

          {rowActions && rowActions.length > 0 && onRowActionClick && (
            <div
              className="opacity-0 group-hover:opacity-100 focus-within:opacity-100 has-[[data-state=open]]:opacity-100 flex items-center shrink-0"
              onClick={e => e.stopPropagation()}
              onKeyDown={e => e.stopPropagation()}
              onPointerDown={e => e.stopPropagation()}
              role="toolbar"
            >
              {rowActions.map(action => (
                <ActionRenderer
                  key={action.tag || action.label}
                  action={action}
                  onActionClick={clickedAction =>
                    onRowActionClick(item, clickedAction)
                  }
                />
              ))}
            </div>
          )}
        </div>
        <CollapsibleContent className="overflow-hidden transition-all data-[state=closed]:animate-accordion-up data-[state=open]:animate-accordion-down">
          <div className="ivy-tree-children pl-3 ml-2 border-l border-border/50">
            {item.children!.map(child => (
              <TreeItem
                key={child.tag || child.label}
                item={child}
                onItemClick={onItemClick}
                rowActions={rowActions}
                hasSiblingWithChildren={item.children!.some(
                  c => c.children && c.children.length > 0
                )}
                onRowActionClick={onRowActionClick}
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
        'ivy-tree-item group flex items-center flex-1 gap-1 rounded-sm py-1 px-1 text-sm cursor-pointer select-none outline-none',
        'hover:bg-accent/50 transition-colors focus-visible:ring-1 focus-visible:ring-ring',
        item.disabled && 'opacity-50 cursor-not-allowed'
      )}
      role="treeitem"
      aria-disabled={item.disabled}
      tabIndex={item.disabled ? -1 : 0}
      onKeyDown={handleKeyDown}
      onClick={handleClick}
    >
      {hasSiblingWithChildren && <span className="h-5 w-5 shrink-0" />}
      {item.icon && item.icon !== 'None' && (
        <Icon
          className="h-4 w-4 shrink-0 text-muted-foreground"
          name={item.icon}
        />
      )}
      <span className="truncate flex-1">{item.label}</span>

      {rowActions && rowActions.length > 0 && onRowActionClick && (
        <div
          className="opacity-0 group-hover:opacity-100 focus-within:opacity-100 has-[[data-state=open]]:opacity-100 flex items-center shrink-0 pr-1"
          onClick={e => e.stopPropagation()}
          onKeyDown={e => e.stopPropagation()}
          onPointerDown={e => e.stopPropagation()}
          role="toolbar"
        >
          {rowActions.map(action => (
            <ActionRenderer
              key={action.tag || action.label}
              action={action}
              onActionClick={clickedAction =>
                onRowActionClick(item, clickedAction)
              }
            />
          ))}
        </div>
      )}
    </div>
  );
};
