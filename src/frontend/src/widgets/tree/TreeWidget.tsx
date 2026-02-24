import React from 'react';
import { cn } from '@/lib/utils';
import { MenuItem } from '@/types/widgets';
import { TreeItem } from './TreeItem';
import { useEventHandler } from '@/components/event-handler';

interface TreeWidgetProps {
  id: string;
  items?: MenuItem[];
}

export const TreeWidget: React.FC<TreeWidgetProps> = ({ id, items = [] }) => {
  const eventHandler = useEventHandler();

  const onItemClick = React.useCallback(
    (item: MenuItem) => {
      if (!item.tag) return;
      eventHandler('OnSelect', id, [item.tag]);
    },
    [eventHandler, id]
  );

  return (
    <div className={cn('ivy-tree w-full')} role="tree">
      {items.map((item, index) => (
        <TreeItem
          key={`${item.label}-${index}`}
          item={item}
          onItemClick={onItemClick}
        />
      ))}
    </div>
  );
};
