import React, { Suspense } from 'react';
import { WidgetNode } from '@/types/widgets';
import { widgetMap } from '@/widgets/widgetMap';
import { Scales } from '@/types/scale';
import {
  isExternalWidget,
  createLazyExternalWidget,
  getCachedExternalWidget,
} from '@/widgets/externalWidgetLoader';
import {
  wrapExternalWidget,
  ExternalWidgetWrapper,
} from '@/widgets/ExternalWidgetWrapper';

// Cache for wrapped external widget components
const wrappedExternalWidgetCache = new Map<
  string,
  React.ComponentType<Record<string, unknown>>
>();

/**
 * Gets or creates a wrapped component for an external widget.
 * The wrapper provides the event handler as a prop.
 */
const getExternalWidgetComponent = (
  typeName: string
): React.ComponentType<Record<string, unknown>> => {
  // Check if we have a wrapped component cached
  const cached = wrappedExternalWidgetCache.get(typeName);
  if (cached) {
    return cached;
  }

  // Check if the component is already loaded (not lazy)
  const loadedComponent = getCachedExternalWidget(typeName);
  if (loadedComponent) {
    // Already loaded, wrap it with ExternalWidgetWrapper
    const Wrapped: React.FC<Record<string, unknown>> = props => (
      <ExternalWidgetWrapper Component={loadedComponent} props={props}>
        {props.children as React.ReactNode}
      </ExternalWidgetWrapper>
    );
    wrappedExternalWidgetCache.set(typeName, Wrapped);
    return Wrapped;
  }

  // Create lazy component and wrap it
  const lazyComponent = createLazyExternalWidget(typeName);
  const wrapped = wrapExternalWidget(lazyComponent);
  wrappedExternalWidgetCache.set(typeName, wrapped);

  return wrapped;
};

const isLazyComponent = (
  component:
    | React.ComponentType<Record<string, unknown>>
    | React.LazyExoticComponent<React.ComponentType<Record<string, unknown>>>
): boolean => {
  return (
    component &&
    (component as { $$typeof?: symbol }).$$typeof === Symbol.for('react.lazy')
  );
};

const isChartComponent = (nodeType: string): boolean => {
  return nodeType.startsWith('Ivy.') && nodeType.includes('Chart');
};

const flattenChildren = (children: WidgetNode[]): WidgetNode[] => {
  return children.flatMap(child => {
    if (child.type === 'Ivy.Fragment') {
      return flattenChildren(child.children || []);
    }
    return [child];
  });
};

export const renderWidgetTree = (
  node: WidgetNode,
  inheritedScale?: Scales
): React.ReactNode => {
  // First check built-in widgets
  let Component = widgetMap[
    node.type as keyof typeof widgetMap
  ] as React.ComponentType<Record<string, unknown>>;

  // If not found, check if it's an external widget
  if (!Component && isExternalWidget(node.type)) {
    Component = getExternalWidgetComponent(node.type);
  }

  if (!Component) {
    return <div>{`Unknown component type: ${node.type}`}</div>;
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

  // Process children, grouping by Slot widgets (original behavior)
  const slots = children.reduce(
    (acc, child) => {
      if (child.type === 'Ivy.Slot') {
        const slotName = child.props.name as string;
        acc[slotName] = (child.children || []).map(slotChild =>
          renderWidgetTree(slotChild, scaleForChildren)
        );
      } else {
        acc.default = acc.default || [];
        acc.default.push(renderWidgetTree(child, scaleForChildren));
      }
      return acc;
    },
    {} as Record<string, React.ReactNode[]>
  );

  // For Kanban widget, pass widget node children for structured data extraction
  if (node.type === 'Ivy.Kanban') {
    props.widgetNodeChildren = children.filter(
      child => child.type === 'Ivy.KanbanCard'
    );
  }

  const content = (
    <Component {...props} slots={slots} key={node.id}>
      {slots.default}
    </Component>
  );

  // For chart components, provide a specific fallback
  if (isLazyComponent(Component) && isChartComponent(node.type)) {
    return (
      <Suspense
        fallback={
          <div className="flex items-center justify-center p-8 text-muted-foreground">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
            <span className="ml-2">Loading chart...</span>
          </div>
        }
        key={node.id}
      >
        {content}
      </Suspense>
    );
  }

  // For other lazy components, use original behavior
  return isLazyComponent(Component) ? (
    <Suspense key={node.id}>{content}</Suspense>
  ) : (
    content
  );
};

export const loadingState = (): WidgetNode => ({
  type: '$loading',
  id: 'loading',
  props: {},
  events: [],
});
