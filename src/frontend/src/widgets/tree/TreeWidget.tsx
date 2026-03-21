import React from 'react';
import { cn } from '@/lib/utils';
import { MenuItem } from '@/types/widgets';
import { TreeItem } from './TreeItem';
import { useEventHandler } from '@/components/event-handler';

const EMPTY_ARRAY: never[] = [];

interface TreeWidgetProps {
  id: string;
  items?: MenuItem[];
  rowActions?: MenuItem[];
}

export const TreeWidget: React.FC<TreeWidgetProps> = ({
  id,
  items = EMPTY_ARRAY,
  rowActions,
}) => {
  const eventHandler = useEventHandler();

  const onItemClick = React.useCallback(
    (item: MenuItem) => {
      if (!item.tag) return;
      eventHandler('OnSelect', id, [item.tag]);
    },
    [eventHandler, id]
  );

  const onRowActionClick = React.useCallback(
    (item: MenuItem, action: MenuItem) => {
      // Both the item and the action might have tags (often undefined if just labels)
      // Send them via the payload of the TreeRowActionClickEventArgs
      eventHandler('OnRowAction', id, [
        {
          itemValue: item.tag ?? item.label,
          actionTag: action.tag ?? action.label,
        },
      ]);
    },
    [eventHandler, id]
  );

  return (
    <div className={cn('ivy-tree w-full')} role="tree">
      {items.map(item => (
        <TreeItem
          key={item.tag || item.label}
          item={item}
          onItemClick={onItemClick}
          rowActions={rowActions}
          hasSiblingWithChildren={items.some(
            i => i.children && i.children.length > 0
          )}
          onRowActionClick={onRowActionClick}
        />
      ))}
    </div>
  );
};
