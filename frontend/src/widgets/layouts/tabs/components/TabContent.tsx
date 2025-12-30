import React from 'react';
import Icon from '@/components/Icon';
import { Badge } from '@/components/ui/badge';
import { cn } from '@/lib/utils';
import { RotateCw, X } from 'lucide-react';
import type { TabWidgetProps } from '../types';

interface TabContentRendererProps {
  tabWidget: React.ReactElement;
  activeTabId: string | null;
  showClose: boolean;
  showRefresh: boolean;
  tabOrder: string[];
  isUserInitiatedChangeRef: React.MutableRefObject<boolean>;
  safeEvent: (
    name:
      | 'OnSelect'
      | 'OnClose'
      | 'OnRefresh'
      | 'OnReorder'
      | 'OnAddButtonClick',
    args: unknown[]
  ) => void;
}

/**
 * Renders the content of a tab including icon, title, badge, and action buttons.
 * Handles close and refresh button visibility and interactions.
 */
export const TabContentRenderer: React.FC<TabContentRendererProps> = ({
  tabWidget,
  activeTabId,
  showClose,
  showRefresh,
  tabOrder,
  isUserInitiatedChangeRef,
  safeEvent,
}) => {
  if (!React.isValidElement(tabWidget)) return null;

  const { title, id: tabId, icon, badge } = tabWidget.props as TabWidgetProps;
  const isActive = activeTabId === tabId;

  return (
    <>
      {icon && (
        <Icon name={icon} className="-ms-0.5 me-1.5 opacity-60" size={16} />
      )}
      <span>{title}</span>
      {badge && (
        <Badge variant="primary" className="ml-2 w-min whitespace-nowrap">
          {badge}
        </Badge>
      )}
      <div className="ml-2 items-center flex gap-0">
        {isActive && showRefresh && (
          <a
            onClick={e => {
              e.stopPropagation();
              isUserInitiatedChangeRef.current = true;
              safeEvent('OnRefresh', [tabOrder.indexOf(tabId)]);
            }}
            className="opacity-60 p-1 rounded-full border border-transparent hover:border-border hover:bg-accent hover:opacity-100 transition-colors cursor-pointer"
          >
            <RotateCw className="w-3 h-3" />
          </a>
        )}
        {showClose && (
          <a
            onClick={e => {
              e.stopPropagation();
              isUserInitiatedChangeRef.current = true;
              safeEvent('OnClose', [tabOrder.indexOf(tabId)]);
            }}
            className={cn(
              'opacity-60 p-1 rounded-full border border-transparent hover:border-border hover:bg-accent hover:opacity-100 transition-colors cursor-pointer',
              !isActive && 'invisible group-hover:visible'
            )}
          >
            <X className="w-3 h-3" />
          </a>
        )}
      </div>
    </>
  );
};
