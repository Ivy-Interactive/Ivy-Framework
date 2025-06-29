import React, { useState, useEffect, useCallback, useMemo } from 'react';

import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarHeader,
  SidebarTrigger,
  SidebarProvider,
  SidebarGroup,
  SidebarGroupLabel,
  SidebarGroupContent,
  SidebarMenu,
  SidebarMenuItem,
  SidebarMenuButton,
  SidebarMenuSub,
  SidebarMenuSubItem,
  SidebarMenuSubButton
} from "@/components/ui/sidebar"
import Icon from '@/components/Icon';
import { useEventHandler } from '@/components/EventHandlerContext';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '@/components/ui/collapsible';
import { ChevronRight } from 'lucide-react';
import { MenuItem, WidgetEventHandlerType } from '@/types/widgets';
import { ScrollArea } from '@/components/ui/scroll-area';
import { useFocusable } from '@/hooks/use-focus-management';

interface SidebarLayoutWidgetProps {
  slots?: {
    SidebarHeader?: React.ReactNode[];
    SidebarContent?: React.ReactNode[];
    SidebarFooter?: React.ReactNode[];
    MainContent: React.ReactNode[];
  };
}

export const SidebarLayoutWidget: React.FC<SidebarLayoutWidgetProps> = ({
  slots
}) => {
  return (
    <div className="flex flex-row h-screen w-screen remove-parent-padding"> 
      <SidebarProvider>
        <Sidebar>
          {slots?.SidebarHeader && <SidebarHeader>
            {slots?.SidebarHeader}
          </SidebarHeader>}
          {slots?.SidebarContent && <SidebarContent>
            <ScrollArea className="h-full">
              {slots?.SidebarContent}
            </ScrollArea>
          </SidebarContent>}
          {slots?.SidebarFooter && <SidebarFooter>
            {slots?.SidebarFooter}
          </SidebarFooter>}
        </Sidebar>
        <main className='flex-1 min-w-0 flex flex-col'>
          <SidebarTrigger className='absolute z-50 ml-2 mt-3'/>
          {slots?.MainContent}
        </main>
      </SidebarProvider>
    </div>
  );
};

interface SidebarMenuWidgetProps {
  id: string;
  items: MenuItem[];
  searchActive?: boolean;
}

type FlatMenuItem = MenuItem & { isGroup?: boolean };
const flattenMenuItems = (items: MenuItem[], parentExpanded = true): FlatMenuItem[] => {
  let flat: FlatMenuItem[] = [];
  for (const item of items) {
    if (item.children && item.children.length > 0 && (parentExpanded || item.expanded)) {
      flat.push({ ...(item as MenuItem), isGroup: true });
      flat = flat.concat(flattenMenuItems(item.children, true));
    } else {
      flat.push(item);
    }
  }
  return flat;
};

// Separate component for collapsible menu items to properly manage state
const CollapsibleMenuItem: React.FC<{
  item: MenuItem;
  eventHandler: WidgetEventHandlerType;
  widgetId: string;
  level: number;
}> = ({ item, eventHandler, widgetId, level }) => {
  const [isOpen, setIsOpen] = useState(item.expanded);

  const onItemClick = (item: MenuItem) => {
    if(!item.tag) return;
    eventHandler("OnSelect", widgetId, [item.tag]);
  }

  const onCtrlRightMouseClick = (e: React.MouseEvent, item: MenuItem) => {
    if(e.ctrlKey && e.button === 2 && !!item.tag) {
      e.preventDefault();
      eventHandler("OnCtrlRightClickSelect", widgetId, [item.tag]);
    }
  }

  if(!!item.children && item.children!.length > 0) {
    return (
      <Collapsible className="group/collapsible" key={item.label} open={isOpen} onOpenChange={setIsOpen}>
        <SidebarMenuItem>
          <CollapsibleTrigger asChild>
            <SidebarMenuButton onClick={() => onItemClick(item)} onMouseDown={(e) => onCtrlRightMouseClick(e, item)}>
              <Icon name={item.icon} size={20} />
              <span>{item.label}</span>
              <ChevronRight className="ml-auto transition-transform group-data-[state=open]/collapsible:rotate-90" />
            </SidebarMenuButton>
          </CollapsibleTrigger>
          <CollapsibleContent>
            <SidebarMenuSub>
              {item.children && renderMenuItems(item.children!, eventHandler, widgetId, level + 1)}
            </SidebarMenuSub>
          </CollapsibleContent>
        </SidebarMenuItem>
      </Collapsible>
    );
  } else {
    return (
      <SidebarMenuItem key={item.label}>
        <SidebarMenuButton onClick={() => onItemClick(item)} onMouseDown={(e) => onCtrlRightMouseClick(e, item)}>
          <Icon name={item.icon} size={20} />
          <span>{item.label}</span>
        </SidebarMenuButton>
      </SidebarMenuItem>
    );
  }
};

const renderMenuItems = (items: (MenuItem)[], eventHandler:WidgetEventHandlerType, widgetId:string,  level:number) => {

  const onItemClick = (item:MenuItem) => {
    if(!item.tag) return;
    eventHandler("OnSelect", widgetId, [item.tag]);
  }

  const onCtrlRightMouseClick = (e: React.MouseEvent, item:MenuItem) => {
    if(e.ctrlKey && e.button === 2 && !!item.tag) {
      e.preventDefault();
      eventHandler("OnCtrlRightClickSelect", widgetId, [item.tag]);
    }
  }

  return items.map((item) => {
    if ('children' in item) {
      if(level === 0) {
        return ( 
          <SidebarGroup key={item.label}>
            <SidebarGroupLabel>
              {item.label}
            </SidebarGroupLabel>
              <SidebarGroupContent>
                <SidebarMenu>
                  {item.children && renderMenuItems(item.children!, eventHandler, widgetId, 1)}
                </SidebarMenu>
              </SidebarGroupContent>  
          </SidebarGroup>
        )
      }
      else
      {
        return <CollapsibleMenuItem 
          key={item.label}
          item={item}
          eventHandler={eventHandler}
          widgetId={widgetId}
          level={level}
        />;
      }

    } else {
      if(level === 0) {
        return <></>      
      } 
      if(level === 1) {
        return <SidebarMenuItem key={item.tag}>
          <SidebarMenuButton onClick={() => onItemClick(item)} onMouseDown={(e) => onCtrlRightMouseClick(e, item)}>
            <Icon name={item.icon} size={20} />
            <span>{item.label}</span>
          </SidebarMenuButton>
        </SidebarMenuItem>
      }
      else 
      {
        return <SidebarMenuSubItem key={item.tag}>
        <SidebarMenuSubButton onClick={() => onItemClick(item)} onMouseDown={(e) => onCtrlRightMouseClick(e, item)}>
          <Icon name={item.icon} size={20} />
          <span>{item.label}</span>
        </SidebarMenuSubButton>
      </SidebarMenuSubItem>
      }
    }
  });
};

export const sidebarMenuRef = React.createRef<HTMLDivElement>();

export const SidebarMenuWidget: React.FC<SidebarMenuWidgetProps> = ({
  id,
  items,
  searchActive = false
}) => {
  const eventHandler = useEventHandler();
  const [selectedIndex, setSelectedIndex] = useState(0);
  // Register only the sidebar menu container with useFocusable
  const { ref: focusRef } = useFocusable('sidebar-navigation', 1);

  const flatItems: FlatMenuItem[] = useMemo(() => {
    return searchActive ? flattenMenuItems(items).filter(i => !i.isGroup) : [];
  }, [searchActive, items]);

  useEffect(() => {
    setSelectedIndex(0);
  }, [searchActive]);

  const handleMenuKeyDown = useCallback((e: React.KeyboardEvent) => {
    if (!searchActive || flatItems.length === 0) return;
    if (e.key === 'ArrowDown') {
      setSelectedIndex(idx => Math.min(idx + 1, flatItems.length - 1));
      e.preventDefault();
    } else if (e.key === 'ArrowUp') {
      setSelectedIndex(idx => Math.max(idx - 1, 0));
      e.preventDefault();
    } else if (e.key === 'Enter') {
      const item = flatItems[selectedIndex];
      if (item && item.tag) {
        eventHandler('OnSelect', id, [item.tag]);
      }
      e.preventDefault();
    }
  }, [searchActive, flatItems, selectedIndex, eventHandler, id]);

  const renderMenuItemsWithHighlight = (items: MenuItem[], level: number, flatIdxRef: { current: number }) => {
    return items.map((item) => {
      if (item.children && item.children.length > 0) {
        return (
          <SidebarGroup key={item.label}>
            <SidebarGroupLabel>{item.label}</SidebarGroupLabel>
            <SidebarGroupContent>
              <SidebarMenu>
                {renderMenuItemsWithHighlight(item.children, level + 1, flatIdxRef)}
              </SidebarMenu>
            </SidebarGroupContent>
          </SidebarGroup>
        );
      } else {
        const flatIdx = flatIdxRef.current;
        flatIdxRef.current++;
        const isActive = searchActive && flatIdx === selectedIndex;
        return (
          <SidebarMenuItem key={item.tag}>
            <SidebarMenuButton
              isActive={isActive}
              tabIndex={-1} // Not focusable
              onClick={() => item.tag && eventHandler('OnSelect', id, [item.tag])}
              onMouseEnter={() => {
                if (searchActive) {
                  setSelectedIndex(flatIdx);
                }
              }}
              style={isActive ? { background: 'var(--sidebar-accent)', color: 'var(--sidebar-accent-foreground)' } : {}}
            >
              <Icon name={item.icon} size={20} />
              <span>{item.label}</span>
            </SidebarMenuButton>
          </SidebarMenuItem>
        );
      }
    });
  };

  const flatIdxRef = { current: 0 };
  return (
    <div
      ref={el => {
        focusRef(el);
        (sidebarMenuRef as React.MutableRefObject<HTMLDivElement | null>).current = el;
      }}
      tabIndex={0}
      onFocus={() => {
        if (searchActive && flatItems.length > 0) setSelectedIndex(0);
      }}
      onKeyDown={handleMenuKeyDown}
      style={{ outline: 'none' }}
      data-sidebar-menu-widget
    >
      {searchActive
        ? renderMenuItemsWithHighlight(items, 0, flatIdxRef)
        : renderMenuItems(items, eventHandler, id, 0)}
    </div>
  );
}