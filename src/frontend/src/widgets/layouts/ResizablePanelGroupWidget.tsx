import React from 'react';
import {
  ResizablePanelGroup,
  ResizablePanel,
  ResizableHandle,
} from '@/components/ui/resizable';
import { camelCase, cn } from '@/lib/utils';
import { getHeight, getWidth } from '@/lib/styles';

interface ResizablePanelWidgetProps {
  children: React.ReactNode[];
  defaultSize?: string | number;
  id?: string;
}

export const ResizablePanelWidget: React.FC<ResizablePanelWidgetProps> = ({
  children,
}) => {
  return <div className="h-full w-full p-4">{children}</div>;
};

ResizablePanelWidget.displayName = 'ResizablePanelWidget';

interface ResizablePanelGroupWidgetProps {
  id: string;
  children: React.ReactElement<ResizablePanelWidgetProps>[];
  showHandle?: boolean;
  direction?: 'Horizontal' | 'Vertical';
  width?: string;
  height?: string;
}

export const ResizablePanelGroupWidget: React.FC<
  ResizablePanelGroupWidgetProps
> = ({
  id,
  children,
  showHandle = true,
  direction = 'Horizontal',
  width = 'Full',
  height = 'Full',
}) => {
  const panelWidgets = React.Children.toArray(children).filter(child => {
    if (!React.isValidElement(child)) return false;

    // Direct component check
    if (
      typeof child.type === 'function' &&
      (child.type as { displayName?: string })?.displayName ===
        'ResizablePanelWidget'
    ) {
      return true;
    }

    // MemoizedWidget check - look at node.type prop
    const props = child.props as { node?: { type?: string } };
    if (props.node?.type === 'Ivy.ResizablePanel') {
      return true;
    }

    return false;
  });

  if (panelWidgets.length === 0)
    return <div className="remove-parent-padding"></div>;

  const style = {
    ...getWidth(width),
    ...getHeight(height),
  };

  return (
    <ResizablePanelGroup
      style={style}
      direction={camelCase(direction) as 'horizontal' | 'vertical'}
      className="remove-parent-padding"
      id={id}
    >
      {panelWidgets.map((panelWidget, index) => {
        if (React.isValidElement(panelWidget)) {
          // Check if this is a MemoizedWidget wrapping a ResizablePanel
          const memoizedProps = panelWidget.props as {
            node?: { props?: { defaultSize?: string | number } };
          };
          const directProps = panelWidget.props as ResizablePanelWidgetProps;

          // Get defaultSize from either MemoizedWidget's node.props or direct props
          const defaultSizeProp =
            memoizedProps.node?.props?.defaultSize ?? directProps.defaultSize;

          const { defaultSize, minSize, maxSize } =
            parseSizeString(defaultSizeProp);

          return (
            <React.Fragment
              key={
                (panelWidget as React.ReactElement<{ id?: string }>).props.id ||
                index
              }
            >
              {index > 0 && showHandle && (
                <ResizableHandle
                  withHandle={showHandle}
                  className={cn(
                    'border',
                    direction === 'Horizontal' ? 'border-r' : 'border-t'
                  )}
                />
              )}
              <ResizablePanel
                defaultSize={
                  defaultSize ?? Math.floor(100 / panelWidgets.length)
                }
                {...(minSize !== undefined && { minSize })}
                {...(maxSize !== undefined && { maxSize })}
                className="h-full"
              >
                {panelWidget}
              </ResizablePanel>
            </React.Fragment>
          );
        }
        return null;
      })}
    </ResizablePanelGroup>
  );
};

function parseSizeString(size?: string | number): {
  defaultSize?: number;
  minSize?: number;
  maxSize?: number;
} {
  if (typeof size === 'number') return { defaultSize: size };
  if (!size) return {};

  // Size.ToString() format: Type:Value,MinType:MinValue,MaxType:MaxValue
  // Example: Fraction:0.5,Fraction:0.2,Fraction:0.8 (Value, Min, Max)
  // Or just: Fraction:0.5

  const parts = size.split(',');
  const result: {
    defaultSize?: number;
    minSize?: number;
    maxSize?: number;
  } = {};

  // The first part is always the main size
  if (parts.length > 0) {
    result.defaultSize = parseSingleSize(parts[0]);
  }

  // Subsequent parts are Min/Max, but we don't know the order from split alone if some are missing?
  // Actually, Size.ToString() *always* appends comma then nested Size.ToString() if present.
  // But wait, Size.ToString() format is:
  // $"{size.Type.ToString()}:{Value}" followed by optional comma and Min, then Max.
  // BUT the nested Min/Max are also Size objects, so they produce "Type:Value...".
  // This simple split by comma might work IF existing Size.ToString implementation guarantees order: Value, Min, Max.
  // Let's re-verify Size.cs ToString implementation.
  // Format(this) + "," + Format(Min) + "," + Format(Max)
  // Yes, order is fixed: Value, Min, Max.
  // However, if Min is null, it skips it?
  // Use `if (Min != null) sb.Append(...)`.
  // Wait, if Min is missing but Max is present, the comma structure might be ambiguous if we just split by comma?
  // Actually, Size.ToString implementation:
  // sb.Append(Format(this));
  // sb.Append(",");
  // if (Min != null) sb.Append(Format(Min!)); // NO COMMA BEFORE?
  // No, `sb.Append(",")` is unconditional after `Format(this)`.
  // Then `if (Min != null)` appends Min.
  // Then `sb.Append(",")` unconditional.
  // Then `if (Max != null)` appends Max.
  // Then `TrimEnd(',')`.
  // So:
  // Case 1: Value only -> "Type:Val,," -> Trimmed -> "Type:Val"
  // Case 2: Value + Min -> "Type:Val,MinType:MinVal," -> Trimmed -> "Type:Val,MinType:MinVal"
  // Case 3: Value + Max -> "Type:Val,,MaxType:MaxVal" -> Trimmed -> "Type:Val,,MaxType:MaxVal"  <-- Double comma!
  // Case 4: All -> "Type:Val,MinType:MinVal,MaxType:MaxVal"

  // So we can split by comma and handle empty strings.
  // Index 0: Value
  // Index 1: Min
  // Index 2: Max

  if (parts.length > 1 && parts[1]) {
    result.minSize = parseSingleSize(parts[1]);
  }
  if (parts.length > 2 && parts[2]) {
    result.maxSize = parseSingleSize(parts[2]);
  }

  return result;
}

function parseSingleSize(sizeStr: string): number | undefined {
  if (!sizeStr) return undefined;
  const [type, val] = sizeStr.split(':');
  const value = parseFloat(val);

  if (isNaN(value)) return undefined;

  switch (type.toLowerCase()) {
    case 'fraction':
      return value * 100;
    case 'px':
      // ResizablePanel usually expects percentages (0-100) or pixels?
      // react-resizable-panels uses percentages by default for defaultSize?
      // Checked docs/usage: defaultSize is often percentage.
      // But it can support pixels if implicit?
      // Actually typical usage is percentage.
      // If Px is passed, we might be in trouble if the group is percentage based.
      // For now, let's treat Fraction (0-1) as percentage (0-100).
      // If explicit Px are needed, we might need a converter or specialized logic.
      // Given user request "Size API", Fraction is most likely intended target for panels.
      return undefined; // Px not fully supported yet for panels without container query
    default:
      return undefined;
  }
}
