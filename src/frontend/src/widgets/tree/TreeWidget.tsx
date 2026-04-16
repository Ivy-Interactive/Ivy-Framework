import React from "react";
import { cn } from "@/lib/utils";
import { MenuItem } from "@/types/widgets";
import { TreeItem } from "./TreeItem";
import { useEventHandler } from "@/components/event-handler";
import { Densities } from "@/types/density";
import { densityTreeGap } from "@/components/ui/density-scale";

const EMPTY_ARRAY: never[] = [];

interface TreeWidgetProps {
  id: string;
  items?: MenuItem[];
  rowActions?: MenuItem[];
  density?: Densities;
  events?: string[];
}

export const TreeWidget: React.FC<TreeWidgetProps> = ({
  id,
  items = EMPTY_ARRAY,
  rowActions,
  density = Densities.Medium,
  events = EMPTY_ARRAY,
}) => {
  const eventHandler = useEventHandler();

  const onItemClick = React.useCallback(
    (item: MenuItem) => {
      if (!item.tag) return;
      if (events.includes("OnSelect")) eventHandler("OnSelect", id, [item.tag]);
    },
    [eventHandler, id, events],
  );

  const onRowActionClick = React.useCallback(
    (item: MenuItem, action: MenuItem) => {
      if (!events.includes("OnRowAction")) return;
      // Both the item and the action might have tags (often undefined if just labels)
      // Send them via the payload of the TreeRowActionClickEventArgs
      eventHandler("OnRowAction", id, [
        {
          itemValue: item.tag ?? item.label,
          actionTag: action.tag ?? action.label,
        },
      ]);
    },
    [eventHandler, id, events],
  );

  const gapClass = densityTreeGap[density];

  return (
    <div className={cn("ivy-tree flex flex-col w-full", gapClass)} role="tree">
      {items.map((item) => (
        <TreeItem
          key={item.tag || item.label}
          item={item}
          density={density}
          onItemClick={onItemClick}
          rowActions={rowActions}
          hasSiblingWithChildren={items.some((i) => i.children && i.children.length > 0)}
          onRowActionClick={onRowActionClick}
        />
      ))}
    </div>
  );
};
