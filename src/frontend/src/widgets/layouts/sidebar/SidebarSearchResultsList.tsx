import React from 'react';
import Icon from '@/components/Icon';
import { Separator } from '@/components/ui/separator';
import { cn } from '@/lib/utils';
import type { MenuItem } from '@/types/widgets';

export interface SidebarSearchResultsListProps {
  items: MenuItem[];
  flatItems: MenuItem[];
  selectedIndex: number;
  setSelectedIndex: (index: number) => void;
  onSelect: (tag: string) => void;
  onCtrlRightClick?: (e: React.MouseEvent, item: MenuItem) => void;
  activeTag?: string | null;
  /** Optional data attribute for scroll-into-view in sidebar; omit in overlay */
  dataSidebarResultIndexAttribute?: string;
}

export const SidebarSearchResultsList: React.FC<
  SidebarSearchResultsListProps
> = ({
  items,
  flatItems,
  selectedIndex,
  setSelectedIndex,
  onSelect,
  onCtrlRightClick,
  activeTag,
  dataSidebarResultIndexAttribute = 'data-sidebar-result-index',
}) => {
  const renderResultItem = (item: MenuItem, showPath: boolean) => {
    const flatIdx = flatItems.findIndex(flatItem => flatItem.tag === item.tag);
    const isHovered = flatIdx === selectedIndex;
    const isActivePage = item.tag === activeTag;
    return (
      <li key={item.tag}>
        <button
          {...(flatIdx >= 0 && {
            [dataSidebarResultIndexAttribute]: flatIdx,
          })}
          type="button"
          className={cn(
            'flex w-full rounded-selector p-2 text-sm hover:bg-accent/50 cursor-pointer min-h-8 text-left',
            showPath && item.path
              ? 'flex-col items-start gap-1'
              : 'items-center gap-2',
            isHovered && !isActivePage && 'bg-accent/30',
            isActivePage && 'bg-accent text-accent-foreground hover:bg-accent'
          )}
          tabIndex={-1}
          onClick={() => {
            if (item.tag) {
              if (flatIdx !== -1) setSelectedIndex(flatIdx);
              onSelect(item.tag);
            }
          }}
          onMouseDown={e => onCtrlRightClick?.(e, item)}
          onMouseEnter={() => {
            if (flatIdx >= 0) setSelectedIndex(flatIdx);
          }}
        >
          {showPath && item.path && (
            <span className="text-xs text-muted-foreground truncate w-full">
              {item.path}
            </span>
          )}
          <div className="flex w-full items-center gap-2 min-w-0">
            <Icon name={item.icon} size={16} className="shrink-0" />
            <span className="text-sm truncate font-medium">{item.label}</span>
          </div>
        </button>
      </li>
    );
  };

  return (
    <>
      {items.map(item => {
        if (item.children && item.children.length > 0) {
          const children = item.children;
          const groupsMap = children.reduce<Record<string, MenuItem[]>>(
            (acc, child) => {
              const path = child.path ?? '';
              (acc[path] ??= []).push(child);
              return acc;
            },
            {}
          );
          const groupsOrdered = Object.entries(groupsMap).sort(
            ([pathA], [pathB]) => {
              if (!pathA) return 1;
              if (!pathB) return -1;
              return 0;
            }
          );

          return (
            <div key={item.label} className="space-y-1 mt-6 first:mt-0">
              <h4 className="sticky top-0 z-10 bg-background px-2 py-2 text-small-label text-muted-foreground mb-0">
                {item.label}
              </h4>
              <ul className="space-y-1">
                {groupsOrdered.map(([path, pathItems], index) => (
                  <React.Fragment key={path || '__none__'}>
                    {index > 0 && (
                      <li className="list-none py-2" aria-hidden>
                        <Separator orientation="horizontal" />
                      </li>
                    )}
                    <li className="list-none">
                      {path && (
                        <div className="px-2 pt-2 pb-1 text-xs text-muted-foreground truncate">
                          {path}
                        </div>
                      )}
                      <ul className="space-y-1">
                        {pathItems.map(child => renderResultItem(child, false))}
                      </ul>
                    </li>
                  </React.Fragment>
                ))}
              </ul>
            </div>
          );
        }
        return renderResultItem(item, true);
      })}
    </>
  );
};
