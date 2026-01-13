import React, { Suspense, memo } from 'react';
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
 * Note: This function caches components, so subsequent calls return the same component instance.
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

interface MemoizedWidgetProps {
  node: WidgetNode;
  inheritedScale?: Scales;
}

/**
 * Memoized widget component that only re-renders when the node reference changes.
 * Works with structural sharing in use-backend.tsx - unchanged subtrees keep
 * their reference identity, allowing React to skip re-rendering them.
 * Note: This component only handles built-in widgets. External widgets are
 * rendered directly by renderWidgetTree to avoid component creation during render.
 */
const MemoizedWidget = memo(
  function MemoizedWidget({ node, inheritedScale }: MemoizedWidgetProps) {
    // Only handle built-in widgets - external widgets are handled by renderWidgetTree
    const Component = widgetMap[
      node.type as keyof typeof widgetMap
    ] as React.ComponentType<Record<string, unknown>>;

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

    // Process children, grouping by Slot widgets
    // Use renderWidgetTree for children to properly handle external widgets
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
      <Component {...props} slots={slots}>
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
        >
          {content}
        </Suspense>
      );
    }

    // For other lazy components, use original behavior
    return isLazyComponent(Component) ? (
      <Suspense>{content}</Suspense>
    ) : (
      content
    );
  },
  // Custom comparison: only re-render if the node reference changed
  // Structural sharing ensures unchanged nodes keep their reference
  (prevProps, nextProps) => {
    return (
      prevProps.node === nextProps.node &&
      prevProps.inheritedScale === nextProps.inheritedScale
    );
  }
);

/**
 * Renders an external widget node directly without memoization.
 * This avoids the react-hooks/static-components rule violation that occurs
 * when getExternalWidgetComponent is called inside a memoized component.
 */
const renderExternalWidget = (
  node: WidgetNode,
  inheritedScale?: Scales
): React.ReactNode => {
  const Component = getExternalWidgetComponent(node.type);

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

  // For lazy components (external widgets are typically lazy), wrap in Suspense
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
