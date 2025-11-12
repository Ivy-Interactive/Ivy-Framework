import React from 'react';
import { Tabs, TabsList, TabsContent } from '@/components/ui/tabs';
import { cn } from '@/lib/utils';
import { getPadding, getWidth } from '@/lib/styles';
import { DndContext, closestCenter } from '@dnd-kit/core';
import { SortableContext } from '@dnd-kit/sortable';
import { SortableTabTrigger } from './Sortable';
import type { TabWidgetProps } from '../types';

interface ContentVariantProps {
  removeParentPadding?: boolean;
  width?: string;
  padding?: string;
  containerRef: React.MutableRefObject<HTMLDivElement | null>;
  tabsListRef: React.MutableRefObject<HTMLDivElement | null>;
  tabRefs: React.MutableRefObject<(HTMLButtonElement | null)[]>;
  activeIndex: number;
  isInitialRender: boolean;
  activeStyle: React.CSSProperties;
  activeTabId: string | null;
  visibleTabs: string[];
  orderedTabWidgets: React.ReactElement[];
  tabOrder: string[];
  loadedTabs: Set<string>;
  addButtonText?: string;
  isUserInitiatedChangeRef: React.MutableRefObject<boolean>;
  addToLoadedTabs: (tabId: string) => void;
  setActiveIndex: React.Dispatch<React.SetStateAction<number>>;
  setActiveTabId: React.Dispatch<React.SetStateAction<string | null>>;
  safeEvent: (
    name:
      | 'OnSelect'
      | 'OnClose'
      | 'OnRefresh'
      | 'OnReorder'
      | 'OnAddButtonClick',
    args: unknown[]
  ) => void;
  dropdownMenu: React.ReactNode;
}

interface TabsVariantProps {
  removeParentPadding?: boolean;
  width?: string;
  padding?: string;
  variant: 'Tabs' | 'Content';
  activeTabId: string | null;
  tabOrder: string[];
  events: string[];
  addButtonText?: string;
  containerRef: React.MutableRefObject<HTMLDivElement | null>;
  tabsListRef: React.MutableRefObject<HTMLDivElement | null>;
  visibleTabs: string[];
  orderedTabWidgets: React.ReactElement[];
  tabWidgets: React.ReactElement[];
  loadedTabs: Set<string>;
  sensors: ReturnType<typeof import('@dnd-kit/core').useSensors>;
  handleDragStart: () => void;
  handleDragEnd: (event: {
    active: { id: string | number };
    over: { id: string | number } | null;
  }) => void;
  handleTabSelect: (tabId: string) => void;
  handleMouseDown: (e: React.MouseEvent, index: number) => void;
  isUserInitiatedChangeRef: React.MutableRefObject<boolean>;
  setActiveIndex: React.Dispatch<React.SetStateAction<number>>;
  safeEvent: (
    name:
      | 'OnSelect'
      | 'OnClose'
      | 'OnRefresh'
      | 'OnReorder'
      | 'OnAddButtonClick',
    args: unknown[]
  ) => void;
  renderTabContent: (tabWidget: React.ReactElement) => React.ReactNode;
  dropdownMenu: React.ReactNode;
}

/**
 * Content variant component for TabsLayoutWidget.
 * Renders a modern, animated, borderless tab bar with smooth transitions.
 */
export const ContentVariant: React.FC<ContentVariantProps> = ({
  removeParentPadding,
  width,
  padding,
  containerRef,
  tabsListRef,
  tabRefs,
  activeIndex,
  isInitialRender,
  activeStyle,
  activeTabId,
  visibleTabs,
  orderedTabWidgets,
  tabOrder,
  loadedTabs,
  addButtonText,
  isUserInitiatedChangeRef,
  addToLoadedTabs,
  setActiveIndex,
  setActiveTabId,
  safeEvent,
  dropdownMenu,
}) => {
  return (
    <div
      className={cn(
        'flex flex-col h-full',
        removeParentPadding && 'remove-parent-padding'
      )}
    >
      <div ref={containerRef} className="relative" style={getWidth(width)}>
        {/* Hover Highlight */}
        <div
          className="absolute h-[26px] transition-all duration-300 ease-out bg-accent/20 rounded-[6px] flex items-center"
          style={{
            opacity: activeIndex !== null ? 1 : 0,
            pointerEvents: 'none',
          }}
        />
        {/* Active Indicator */}
        <div
          className={cn(
            'absolute bottom-[-6px] h-[2px] bg-foreground',
            !isInitialRender && 'transition-all duration-300 ease-out',
            activeTabId && !visibleTabs.includes(activeTabId) && 'opacity-0'
          )}
          style={activeStyle}
        />
        {/* Tabs */}
        <div
          ref={tabsListRef}
          className={'relative flex space-x-[6px] gap-y-[20px] items-center'}
          role="tablist"
        >
          {orderedTabWidgets.map((tabWidget, index) => {
            if (!React.isValidElement(tabWidget)) return null;
            const props = tabWidget.props as Partial<TabWidgetProps>;
            if (!props.id) return null;
            const { title, id } = props;

            // Only render tabs that are visible
            if (!visibleTabs.includes(id)) return null;

            return (
              <button
                key={id}
                ref={el => {
                  tabRefs.current[index] = el;
                }}
                role="tab"
                value={id}
                aria-selected={index === activeIndex}
                tabIndex={0}
                className={cn(
                  'px-3 py-1.5 cursor-pointer transition-colors duration-300 h-[26px]',
                  index === activeIndex
                    ? 'text-foreground'
                    : 'text-muted-foreground'
                )}
                onClick={() => {
                  // Mark as user-initiated for Content variant
                  isUserInitiatedChangeRef.current = true;
                  const tabId = tabOrder[index];
                  addToLoadedTabs(tabId);
                  setActiveIndex(index);
                  setActiveTabId(tabId);
                  safeEvent('OnSelect', [index]);
                }}
              >
                <div className="text-sm font-medium leading-4 whitespace-nowrap flex items-center justify-center h-full">
                  {title}
                </div>
              </button>
            );
          })}
          {addButtonText && (
            <div className="flex items-center ml-2">
              <button
                onClick={() => safeEvent('OnAddButtonClick', [0])}
                className="px-3 py-1.5 cursor-pointer transition-colors duration-300 text-muted-foreground hover:text-foreground hover:muted-foreground rounded-[6px] flex items-center justify-center aspect-square border-none"
              >
                <div className="text-sm font-medium leading-4 whitespace-nowrap flex items-center justify-center">
                  {addButtonText}
                </div>
              </button>
            </div>
          )}
        </div>

        {dropdownMenu}
      </div>
      <div className="flex-1 overflow-hidden">
        {orderedTabWidgets.map(tabWidget => {
          if (!React.isValidElement(tabWidget)) return null;
          const props = tabWidget.props as Partial<TabWidgetProps>;
          if (!props.id) return null;
          const { id } = props;
          if (!loadedTabs.has(id)) return null;
          const paddingStyle = getPadding(padding);
          return (
            <div
              key={id}
              role="tabpanel"
              aria-hidden={activeTabId !== id}
              className={cn(
                'h-full overflow-auto',
                activeTabId === id ? 'block' : 'hidden',
                'border-none'
              )}
              style={paddingStyle}
            >
              {tabWidget}
            </div>
          );
        })}
      </div>
    </div>
  );
};

/**
 * Tabs variant component for TabsLayoutWidget.
 * Renders traditional tab bar with drag-and-drop support and Radix UI integration.
 */
export const TabsVariant: React.FC<TabsVariantProps> = ({
  removeParentPadding,
  width,
  padding,
  variant,
  activeTabId,
  tabOrder,
  events,
  addButtonText,
  containerRef,
  tabsListRef,
  visibleTabs,
  orderedTabWidgets,
  tabWidgets,
  loadedTabs,
  sensors,
  handleDragStart,
  handleDragEnd,
  handleTabSelect,
  handleMouseDown,
  isUserInitiatedChangeRef,
  setActiveIndex,
  safeEvent,
  renderTabContent,
  dropdownMenu,
}) => {
  const useRadix = (variant as string) === 'Content';

  return (
    <Tabs
      value={activeTabId ?? undefined}
      onValueChange={value => {
        if (!useRadix) {
          handleTabSelect(value);
        } else {
          // For Radix tabs, also mark as user-initiated to prevent flicker
          isUserInitiatedChangeRef.current = true;
          const newIndex = tabOrder.indexOf(value);
          setActiveIndex(newIndex);
          if (events?.includes('OnSelect')) safeEvent('OnSelect', [newIndex]);
        }
      }}
      useRadix={useRadix}
      className={cn(
        removeParentPadding && 'remove-parent-padding',
        'flex flex-col h-full'
      )}
    >
      <div className="flex-shrink-0" style={getWidth(width)}>
        <div
          className="relative pl-10 pr-6 before:absolute before:inset-x-0 before:bottom-0 before:h-px before:bg-border before:z-0 overflow-hidden"
          ref={containerRef}
        >
          <DndContext
            collisionDetection={closestCenter}
            onDragStart={handleDragStart}
            onDragEnd={handleDragEnd}
            sensors={sensors}
          >
            <SortableContext items={tabOrder}>
              <TabsList
                ref={tabsListRef}
                useRadix={useRadix}
                className="relative h-auto w-full bg-transparent p-0 flex justify-start flex-nowrap"
              >
                {orderedTabWidgets.map(tabWidget => {
                  if (!React.isValidElement(tabWidget)) return null;
                  const props = tabWidget.props as Partial<TabWidgetProps>;
                  if (!props.id) return null;
                  const { id } = props;

                  // Only render tabs that are visible
                  if (!visibleTabs.includes(id)) return null;

                  return (
                    <SortableTabTrigger
                      key={id}
                      id={id}
                      value={id}
                      useRadix={useRadix}
                      onClick={() => {
                        // Mark as user-initiated to prevent flicker
                        isUserInitiatedChangeRef.current = true;
                        handleTabSelect(id);
                      }}
                      onMouseDown={(e: React.MouseEvent) =>
                        handleMouseDown(e, tabOrder.indexOf(id))
                      }
                      className={cn(
                        'group overflow-hidden h-full data-[state=active]:z-10 data-[state=active]:shadow-none border-x border-t border-b border-border flex-shrink-0',
                        // Inactive tab styling - pure black in dark mode
                        'bg-muted dark:bg-background hover:bg-muted-foreground/20 dark:hover:bg-muted-foreground/20',
                        // Active tab styling (overrides inactive)
                        'data-[state=active]:bg-card data-[state=active]:hover:bg-card',
                        (variant as string) === 'Content' &&
                          'border-b-2 border-b-transparent data-[state=active]:border-b-primary'
                      )}
                    >
                      {renderTabContent(tabWidget)}
                    </SortableTabTrigger>
                  );
                })}
                {addButtonText && (
                  <div className="flex items-center ml-2">
                    <button
                      onClick={() => safeEvent('OnAddButtonClick', [0])}
                      className="py-2 cursor-pointer transition-colors duration-300 text-muted-foreground hover:text-foreground hover:bg-gray-200/20 rounded-[6px] flex-shrink-0 flex items-center justify-center aspect-square px-3 border-none"
                    >
                      <div className="text-sm font-medium leading-4 whitespace-nowrap flex items-center justify-center">
                        {addButtonText}
                      </div>
                    </button>
                  </div>
                )}
              </TabsList>
            </SortableContext>
          </DndContext>

          {dropdownMenu}
        </div>
      </div>

      <div className="flex-1 overflow-hidden">
        {React.useMemo(() => {
          return tabWidgets.map(tabWidget => {
            if (!React.isValidElement(tabWidget)) return null;
            const props = tabWidget.props as Partial<TabWidgetProps>;
            if (!props.id) return null;
            const { id } = props;

            if (!loadedTabs.has(id)) return null;

            const paddingStyle = getPadding(padding);

            if (useRadix) {
              // Use TabsContent for Radix version (Content variant)
              return (
                <TabsContent
                  key={id}
                  value={id}
                  useRadix={useRadix}
                  className={cn('h-full overflow-auto border-none mt-0')}
                  style={paddingStyle}
                >
                  {tabWidget}
                </TabsContent>
              );
            }

            // Use custom div-based content for non-Radix version (Tabs variant)
            return (
              <div
                key={id}
                className={cn(
                  'h-full overflow-auto',
                  activeTabId === id ? 'block' : 'hidden'
                )}
                style={paddingStyle}
              >
                {tabWidget}
              </div>
            );
          });
        }, [tabWidgets, loadedTabs, activeTabId, padding, useRadix])}
      </div>
    </Tabs>
  );
};
