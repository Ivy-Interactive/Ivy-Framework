import React, { useState } from 'react';

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
import { MenuItem } from '@/types/widgets';
import { ScrollArea } from '@/components/ui/scroll-area';

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
}

export const SidebarMenuWidget: React.FC<SidebarMenuWidgetProps> = ({
  id,
  items  
}) => {
  const eventHandler = useEventHandler();
  const [openItems, setOpenItems] = useState<Record<string, boolean>>({});

  const onItemClick = (item: MenuItem) => {
    if(!item.tag) return;
    eventHandler("OnSelect", id, [item.tag]);
  }

  const onCtrlRightMouseClick = (e: React.MouseEvent, item: MenuItem) => {
    if(e.ctrlKey && e.button === 2 && !!item.tag) {
      e.preventDefault();
      eventHandler("OnCtrlRightClickSelect", id, [item.tag]);
    }
  }

  const toggleItem = (itemKey: string) => {
    setOpenItems(prev => ({
      ...prev,
      [itemKey]: !prev[itemKey]
    }));
  }

  const renderMenuItems = (items: MenuItem[], level: number): React.ReactNode[] => {
    return items.map((item) => {
      const itemKey = item.tag || item.label;
      
      if ('children' in item) {
        if(level === 0) {
          return ( 
            <SidebarGroup key={item.label}>
              <SidebarGroupLabel>
                {item.label}
              </SidebarGroupLabel>
                <SidebarGroupContent>
                  <SidebarMenu>
                    {item.children && renderMenuItems(item.children!, 1)}
                  </SidebarMenu>
                </SidebarGroupContent>  
            </SidebarGroup>
          )
        }
        else
        {
          if(!!item.children && item.children!.length > 0)
          {
            const isOpen = openItems[itemKey] ?? item.expanded ?? false;
            return (
              <Collapsible className="group/collapsible" key={item.label} open={isOpen} onOpenChange={() => toggleItem(itemKey)}>
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
                      {item.children && renderMenuItems(item.children!, 1)}
                    </SidebarMenuSub>
                  </CollapsibleContent>
                </SidebarMenuItem>
              </Collapsible>
            );
          }
          else
          {
            return <SidebarMenuItem key={item.label}>
              <SidebarMenuButton onClick={() => onItemClick(item)} onMouseDown={(e) => onCtrlRightMouseClick(e, item)}>
                <Icon name={item.icon} size={20} />
                <span>{item.label}</span>
              </SidebarMenuButton>
            </SidebarMenuItem>
          }
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

  return <>{renderMenuItems(items, 0)}</>;
}