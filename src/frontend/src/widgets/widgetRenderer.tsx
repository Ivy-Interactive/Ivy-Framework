/* oxlint-disable react-refresh/only-export-components */
import React, { Suspense } from "react";
import { WidgetNode, CallSite } from "@/types/widgets";
import { widgetMap } from "@/widgets/widgetMap";
import { Densities } from "@/types/density";
import { isExternalWidget, createLazyExternalWidget } from "@/widgets/externalWidgetLoader";
import { ExternalWidgetWrapper } from "@/widgets/ExternalWidgetWrapper";

const processWidgetProps = (
  node: WidgetNode,
  inheritedScale?: Densities,
): Record<string, unknown> => {
  const props: Record<string, unknown> = {
    ...node.props,
    id: node.id,
    events: node.events || [],
  };
  if (inheritedScale) props.density = inheritedScale;
  if ("testId" in props && props.testId) {
    props["data-testid"] = props.testId;
    delete props.testId;
  }
  return props;
};

const processSlots = (
  children: WidgetNode[],
  scaleForChildren: Densities | undefined,
): Record<string, React.ReactNode[]> => {
  return children.reduce<Record<string, React.ReactNode[]>>((acc, child: WidgetNode) => {
    if (child.type === "Ivy.Slot") {
      const slotName = child.props.name as string;
      acc[slotName] = (child.children || []).map((slotChild: WidgetNode) =>
        renderWidgetTree(slotChild, scaleForChildren),
      );
    } else {
      acc.default = acc.default || [];
      acc.default.push(renderWidgetTree(child, scaleForChildren));
    }
    return acc;
  }, {});
};

const registerCallSite = (node: WidgetNode) => {
  if (node.callSite) {
    widgetCallSiteRegistry.set(node.id, node.callSite);
  }
};

export interface MemoizedWidgetProps {
  node: WidgetNode;
  inheritedScale?: Densities;
}

export const MemoizedWidget = React.memo(
  function MemoizedWidget({ node, inheritedScale }: MemoizedWidgetProps) {
    const Component = widgetMap[node.type as keyof typeof widgetMap] as React.ComponentType<
      Record<string, unknown>
    >;

    if (!Component) {
      return <div>{`Unknown component type: ${node.type}`}</div>;
    }

    const props = processWidgetProps(node, inheritedScale);
    const children = flattenChildren(node.children || []);
    const scaleForChildren = (props.density as Densities) || inheritedScale;
    const slots = processSlots(children, scaleForChildren);

    props.widgetNodeChildren = children;

    registerCallSite(node);

    // Store raw content for text-editable widgets
    const isTextEditable = TEXT_EDITABLE_TYPES.includes(node.type);
    const rawContent = isTextEditable
      ? (node.props.content as string) || (node.props.text as string) || ""
      : undefined;

    const content = (
      <ivy-widget id={node.id} type={node.type} data-content={rawContent}>
        <Component {...props} slots={slots}>
          {slots.default}
        </Component>
      </ivy-widget>
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

    return isLazyComponent(Component) ? <Suspense>{content}</Suspense> : content;
  },
  (prevProps, nextProps) => {
    return (
      prevProps.node === nextProps.node && prevProps.inheritedScale === nextProps.inheritedScale
    );
  },
);

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
    listeners.forEach((listener) => listener());
  }
};

export const clearWidgetContentOverride = (widgetId: string) => {
  widgetContentOverrides.delete(widgetId);
  const listeners = contentListeners.get(widgetId);
  if (listeners) {
    listeners.forEach((listener) => listener());
  }
};

export const subscribeToContentOverride = (widgetId: string, listener: () => void) => {
  if (!contentListeners.has(widgetId)) {
    contentListeners.set(widgetId, new Set());
  }
  contentListeners.get(widgetId)!.add(listener);
  return () => {
    contentListeners.get(widgetId)?.delete(listener);
  };
};

// Types that support text editing
export const TEXT_EDITABLE_TYPES = ["Ivy.TextBlock", "Ivy.Markdown"];

export const isLazyComponent = (
  component:
    | React.ComponentType<Record<string, unknown>>
    | React.LazyExoticComponent<React.ComponentType<Record<string, unknown>>>,
): boolean => {
  return component && (component as { $$typeof?: symbol }).$$typeof === Symbol.for("react.lazy");
};

export const isChartComponent = (nodeType: string): boolean => {
  return nodeType.startsWith("Ivy.") && nodeType.includes("Chart");
};

export const flattenChildren = (children: WidgetNode[]): WidgetNode[] => {
  return children.flatMap((child) => {
    if (child.type === "Ivy.Fragment") {
      return flattenChildren(child.children || []);
    }
    return [child];
  });
};

/**
 * Renders an external widget node directly without memoization.
 */
const renderExternalWidget = (node: WidgetNode, inheritedScale?: Densities): React.ReactNode => {
  const Component = createLazyExternalWidget(node.type);

  const props = processWidgetProps(node, inheritedScale);

  const children = flattenChildren(node.children || []);
  const scaleForChildren = (props.density as Densities) || inheritedScale;
  const slots = processSlots(children, scaleForChildren);

  props.widgetNodeChildren = children;

  registerCallSite(node);

  const content = (
    <ivy-widget key={node.id} id={node.id} type={node.type}>
      <ExternalWidgetWrapper Component={Component} props={props}>
        {slots.default}
      </ExternalWidgetWrapper>
    </ivy-widget>
  );

  return isLazyComponent(Component) ? <Suspense key={node.id}>{content}</Suspense> : content;
};

/**
 * Entry point for rendering the widget tree.
 * Uses MemoizedWidget for built-in widgets and direct rendering for external widgets.
 */
export const renderWidgetTree = (node: WidgetNode, inheritedScale?: Densities): React.ReactNode => {
  // Check if it's a built-in widget first
  const isBuiltIn = node.type in widgetMap;

  if (isBuiltIn) {
    // Use memoized rendering for built-in widgets
    return <MemoizedWidget key={node.id} node={node} inheritedScale={inheritedScale} />;
  }

  // Check if it's an external widget
  if (isExternalWidget(node.type)) {
    return renderExternalWidget(node, inheritedScale);
  }

  // Unknown widget type
  return <div key={node.id}>{`Unknown component type: ${node.type}`}</div>;
};

export const loadingState = (): WidgetNode => ({
  type: "$loading",
  id: "loading",
  props: {},
  events: [],
});
