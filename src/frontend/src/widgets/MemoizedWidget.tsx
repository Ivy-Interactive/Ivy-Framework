import React, { Suspense, memo } from 'react';
import { WidgetNode } from '@/types/widgets';
import { widgetMap } from '@/widgets/widgetMap';
import { Scales } from '@/types/scale';
import {
  isLazyComponent,
  isChartComponent,
  flattenChildren,
  renderWidgetTree,
} from '@/widgets/widgetRenderer';

export interface MemoizedWidgetProps {
  node: WidgetNode;
  inheritedScale?: Scales;
}

export const MemoizedWidget = memo(
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

    if (node.type === 'Ivy.Kanban') {
      props.widgetNodeChildren = children.filter(
        (child: WidgetNode) => child.type === 'Ivy.KanbanCard'
      );
    }

    const content = (
      <Component {...props} slots={slots}>
        {slots.default}
      </Component>
    );

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

    return isLazyComponent(Component) ? (
      <Suspense>{content}</Suspense>
    ) : (
      content
    );
  },
  (prevProps, nextProps) => {
    return (
      prevProps.node === nextProps.node &&
      prevProps.inheritedScale === nextProps.inheritedScale
    );
  }
);
