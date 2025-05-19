import React, { CSSProperties } from 'react';
import { ScrollArea, ScrollBar } from "@/components/ui/scroll-area";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import Icon from '@/components/Icon';
import { RotateCw, X, ChevronDown } from 'lucide-react';
import { useEventHandler } from '@/components/EventHandlerContext';
import { Badge } from '@/components/ui/badge';
import { cn } from '@/lib/utils';
import { getHeight, getPadding, getWidth } from '@/lib/styles';
import {
  DropdownMenu,
  DropdownMenuTrigger,
  DropdownMenuContent,
  DropdownMenuItem
} from '@/components/ui/dropdown-menu';

interface TabWidgetProps {
  children: React.ReactNode[];
  title: string;
  id: string;
  badge?:string;
  icon?:string;
}

export const TabWidget: React.FC<TabWidgetProps> = ({
  children
}) => {
  return (
    <div className='h-full'> 
      {children} 
    </div>
  );
};

TabWidget.displayName = 'TabWidget';

interface TabsLayoutWidgetProps {
  id: string;
  variant?: "Tabs" | "Content";
  removeParentPadding?: boolean;
  selectedIndex?: number;
  children: React.ReactElement<TabWidgetProps>[];
  events: string[];
  width?: string;
  height?: string;
  padding?: string;
}

export const TabsLayoutWidget: React.FC<TabsLayoutWidgetProps> = ({
  id,
  children,
  events,
  selectedIndex,
  removeParentPadding,
  width,
  height,
  padding,
  variant
}) => {
  const eventHandler = useEventHandler();

  const tabsLayoutId = id;

  const tabWidgets = React.Children.toArray(children).filter((child) => 
    React.isValidElement(child) && 
    (child.type as any)?.displayName === 'TabWidget'
  );

  if(tabWidgets.length === 0) return <div className='remove-parent-padding'></div>;

  const activeTab = selectedIndex != null && React.isValidElement(tabWidgets[selectedIndex]) ? tabWidgets[selectedIndex].props.id : null;

  const showClose = events.includes("OnClose");
  const showRefresh = events.includes("OnRefresh");

  const handleMouseDown = (e: React.MouseEvent, index: number) => {
    if (e.button === 1) {
      e.preventDefault(); 
      eventHandler("OnClose", tabsLayoutId, [index])
    }
  };

  const styles:CSSProperties = { 
    ...getWidth(width),
    ...getHeight(height),
  };

  return (
    <Tabs value={activeTab} style={styles} className={
      cn(
        removeParentPadding && 'remove-parent-padding',
        'flex flex-col h-full'
      )}>
      <div className="flex-shrink-0">
        <div className="relative pl-12">
          <ScrollArea className="w-full pr-12">
            <TabsList className={cn(
              "relative h-auto w-max min-w-full gap-0.5 mt-3 bg-transparent p-0 before:absolute before:inset-x-0 before:bottom-0 before:h-px flex justify-start",
              variant === "Tabs" && "before:bg-border",
              variant === "Content" && ""
              )}>
              {tabWidgets.map((tabWidget, index) => {
                if (React.isValidElement(tabWidget)) {
                  const { title, id, icon, badge  } = tabWidget.props as TabWidgetProps;
                    return (
                      <TabsTrigger
                        key={id}
                        value={id}
                        onClick={() => eventHandler("OnSelect", tabsLayoutId, [index])}
                        onMouseDown={(e) => handleMouseDown(e, index)}
                        className={cn(
                          "group overflow-hidden rounded-b-none py-2 data-[state=active]:z-10 data-[state=active]:shadow-none",
                          (variant === "Tabs" && index === 0) && "ml-12",
                          variant === "Tabs" && "border-x border-t border-border bg-muted",
                          variant === "Content" && "border-b-2 data-[state=active]:border-b-primary"
                        )}
                        >

                        {icon && <Icon
                          name={icon}
                          className="-ms-0.5 me-1.5 opacity-60"
                          size={16}
                          aria-hidden="true"
                          />
                        }
                        
                        <span>{title}</span>

                        {badge && <Badge
                        variant="default"
                        className={cn(
                          "ml-2",
                          "w-min",
                          "whitespace-nowrap",
                          "visible",
                          (showClose || showRefresh) && "group-hover:hidden"
                        )}
                          >{badge}</Badge>}

                        {(showClose || showRefresh) && <div className="absolute ml-2 items-center flex gap-0 invisible group-hover:visible group-hover:relative">
                        {showRefresh && <a
                          onClick={(e) => {
                            e.stopPropagation();
                            eventHandler("OnRefresh", tabsLayoutId, [index])
                          }}
                          className="opacity-60 p-1 rounded-full hover:bg-gray-200 hover:opacity-100 transition-colors"
                        >
                          <RotateCw className="w-3 h-3" />
                        </a>}
                        {showClose && <a
                          onClick={(e) => {
                            e.stopPropagation();
                            eventHandler("OnClose", tabsLayoutId, [index])
                          }}
                          className="opacity-60 p-1 rounded-full  hover:bg-gray-200 hover:opacity-100 transition-colors"
                        >
                          <X className="w-3 h-3" />
                        </a>}
                        </div>}

                      </TabsTrigger>
                    );
                  }
                  return null;
              })}
            </TabsList>
            <ScrollBar orientation="horizontal" />
          </ScrollArea>
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <button
                className="absolute right-0 top-1/2 -translate-y-1/2 rounded p-2 hover:bg-muted transition"
                aria-label="Show more tabs"
                type="button"
              >
                <ChevronDown className="w-5 h-5" />
              </button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              {tabWidgets.map((tabWidget, index) => {
                if (React.isValidElement(tabWidget)) {
                  const { title } = tabWidget.props as TabWidgetProps;
                  return (
                    <DropdownMenuItem
                      key={index}
                      onClick={() => eventHandler("OnSelect", tabsLayoutId, [index])}
                    >
                      {title}
                    </DropdownMenuItem>
                  );
                }
                return null;
              })}
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      </div>

      <div className="flex-1 overflow-hidden">
        {tabWidgets.map((tabWidget, _) => {
          if (React.isValidElement(tabWidget)) {
            const { id } = tabWidget.props as TabWidgetProps;
            return (
              <div 
                key={id} 
                className={cn(
                  'h-full overflow-auto',
                  activeTab === id ? 'block' : 'hidden'
                )}
                style={{ 
                  ...getPadding(padding)
                }}
              >
                {tabWidget}
              </div>
            );
          }
          return null;
        })}
      </div>
    </Tabs>
  );
};
