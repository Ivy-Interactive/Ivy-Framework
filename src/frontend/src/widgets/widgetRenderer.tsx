import React, { Suspense } from 'react';
import { WidgetNode, CallSite } from '@/types/widgets';
import { widgetMap } from '@/widgets/widgetMap';
import { Scales } from '@/types/scale';
import {
  isExternalWidget,
  createLazyExternalWidget,
  getCachedExternalWidget,
} from '@/widgets/externalWidgetLoader';
import { ExternalWidgetWrapper } from '@/widgets/ExternalWidgetWrapper';
import { MemoizedWidget } from '@/widgets/MemoizedWidget';

// Registry for widget callsite information, keyed by widget id
export const widgetCallSiteRegistry = new Map<string, CallSite>();

// Registry for widget content overrides (used by DevTools text editing)
export const widgetContentOverrides = new Map<string, string>();
const contentListeners = new Map<string, Set<() => void>>();

export const setWidgetContentOverride = (widgetId: string, content: string) => {
  widgetContentOverrides.set(widgetId, content);
  // Notify listeners
  const listeners = contentListeners.get(widgetId);
  if (listeners) {
    listeners.forEach(listener => listener());
  }
};

export const clearWidgetContentOverride = (widgetId: string) => {
  widgetContentOverrides.delete(widgetId);
  const listeners = contentListeners.get(widgetId);
  if (listeners) {
    listeners.forEach(listener => listener());
  }
};

export const subscribeToContentOverride = (
  widgetId: string,
  listener: () => void
) => {
  if (!contentListeners.has(widgetId)) {
    contentListeners.set(widgetId, new Set());
  }
  contentListeners.get(widgetId)!.add(listener);
  return () => {
    contentListeners.get(widgetId)?.delete(listener);
  };
};

// Types that support text editing
export const TEXT_EDITABLE_TYPES = ['Ivy.TextBlock', 'Ivy.Markdown'];

export const isLazyComponent = (
  component:
    | React.ComponentType<Record<string, unknown>>
    | React.LazyExoticComponent<React.ComponentType<Record<string, unknown>>>
): boolean => {
  return (
    component &&
    (component as { $$typeof?: symbol }).$$typeof === Symbol.for('react.lazy')
  );
};

export const isChartComponent = (nodeType: string): boolean => {
  return nodeType.startsWith('Ivy.') && nodeType.includes('Chart');
};

export const flattenChildren = (children: WidgetNode[]): WidgetNode[] => {
  return children.flatMap(child => {
    if (child.type === 'Ivy.Fragment') {
      return flattenChildren(child.children || []);
    }
    return [child];
  });
};

/**
 * Renders an external widget node directly without memoization.
 */
const renderExternalWidget = (
  node: WidgetNode,
  inheritedScale?: Scales
): React.ReactNode => {
  let Component = getCachedExternalWidget(node.type);
  if (!Component) {
    Component = createLazyExternalWidget(node.type);
  }

  const props: Record<string, unknown> = {
    ...node.props,
    id: node.id,
    events: node.events || [],
  };

  if (inheritedScale) {
    props.scale = inheritedScale;
  }

  if ('testId' in props && props.testId) {
    props['data-testid'] = props.testId;
    delete props.testId;
  }

  const children = flattenChildren(node.children || []);
  const scaleForChildren = (props.scale as Scales) || inheritedScale;

  // Process children, grouping by Slot widgets
  const slots = children.reduce<Record<string, React.ReactNode[]>>(
    (acc, child: WidgetNode) => {
      if (child.type === 'Ivy.Slot') {
        const slotName = child.props.name as string;
        acc[slotName] = (child.children || []).map((slotChild: WidgetNode) =>
          renderWidgetTree(slotChild, scaleForChildren)
        );
      } else {
        acc.default = acc.default || [];
        acc.default.push(renderWidgetTree(child, scaleForChildren));
      }
      return acc;
    },
    {}
  );

  // For Kanban widget, pass widget node children for structured data extraction
  if (node.type === 'Ivy.Kanban') {
    props.widgetNodeChildren = children.filter(
      (child: WidgetNode) => child.type === 'Ivy.KanbanCard'
    );
  }

  if (node.callSite) {
    widgetCallSiteRegistry.set(node.id, node.callSite);
  }

  const content = (
    <ivy-widget key={node.id} id={node.id} type={node.type}>
      <ExternalWidgetWrapper Component={Component} props={props}>
        {slots.default}
      </ExternalWidgetWrapper>
    </ivy-widget>
  );

  return isLazyComponent(Component) ? (
    <Suspense key={node.id}>{content}</Suspense>
  ) : (
    content
  );
};

/**
 * Entry point for rendering the widget tree.
 * Uses MemoizedWidget for built-in widgets and direct rendering for external widgets.
 */
export const renderWidgetTree = (
  node: WidgetNode,
  inheritedScale?: Scales
): React.ReactNode => {
  // Check if it's a built-in widget first
  const isBuiltIn = node.type in widgetMap;

  if (isBuiltIn) {
    // Use memoized rendering for built-in widgets
    return (
      <MemoizedWidget
        key={node.id}
        node={node}
        inheritedScale={inheritedScale}
      />
    );
  }

  // Check if it's an external widget
  if (isExternalWidget(node.type)) {
    return renderExternalWidget(node, inheritedScale);
  }

  // Unknown widget type
  return <div key={node.id}>{`Unknown component type: ${node.type}`}</div>;
};

export const loadingState = (): WidgetNode => ({
  type: '$loading',
  id: 'loading',
  props: {},
  events: [],
});
