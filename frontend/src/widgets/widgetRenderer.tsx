import React, { Suspense, memo } from 'react';
import { WidgetNode } from '@/types/widgets';
import { widgetMap } from '@/widgets/widgetMap';
import { Scales } from '@/types/scale';

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
 */
const MemoizedWidget = memo(
  function MemoizedWidget({ node, inheritedScale }: MemoizedWidgetProps) {
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
    const slots = children.reduce(
      (acc, child) => {
        if (child.type === 'Ivy.Slot') {
          const slotName = child.props.name as string;
          acc[slotName] = (child.children || []).map(slotChild => (
            <MemoizedWidget
              key={slotChild.id}
              node={slotChild}
              inheritedScale={scaleForChildren}
            />
          ));
        } else {
          acc.default = acc.default || [];
          acc.default.push(
            <MemoizedWidget
              key={child.id}
              node={child}
              inheritedScale={scaleForChildren}
            />
          );
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
 * Entry point for rendering the widget tree.
 * Uses MemoizedWidget internally for optimal re-rendering.
 */
export const renderWidgetTree = (
  node: WidgetNode,
  inheritedScale?: Scales
): React.ReactNode => {
  return <MemoizedWidget node={node} inheritedScale={inheritedScale} />;
};

export const loadingState = (): WidgetNode => ({
  type: '$loading',
  id: 'loading',
  props: {},
  events: [],
});
