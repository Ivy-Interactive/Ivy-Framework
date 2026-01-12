import React, { useCallback } from 'react';

/**
 * Event handler type provided by Ivy to external widgets.
 * Call this function to trigger events back to the C# backend.
 */
type IvyEventHandler = (
  eventName: string,
  widgetId: string,
  args: unknown[]
) => void;

/**
 * Props interface matching the C# SuperChart widget properties.
 * Props come from the [Prop] attributes on the C# class.
 */
interface SuperChartProps {
  /** Widget ID (automatically provided by Ivy) */
  id: string;
  /** The chart title */
  title?: string;
  /** Data points to display */
  data?: number[];
  /** Chart color (CSS color value) */
  color?: string;
  /** Whether to show data labels */
  showLabels?: boolean;
  /** Event names that have handlers (automatically provided by Ivy) */
  events?: string[];
  /**
   * Event handler provided by Ivy for triggering backend events.
   * Call this with (eventName, widgetId, args) to trigger an event.
   */
  onIvyEvent?: IvyEventHandler;
}

/**
 * SuperChart - An example external widget component.
 *
 * This component demonstrates how to create an external widget that:
 * 1. Receives props from the C# widget definition
 * 2. Uses Tailwind CSS classes from the host app
 * 3. Triggers events back to the C# backend via the onIvyEvent prop
 */
export const SuperChart: React.FC<SuperChartProps> = ({
  id,
  title,
  data = [],
  color = '#3b82f6',
  showLabels = true,
  events = [],
  onIvyEvent,
}) => {
  // Check if the OnPointClick event has a handler
  const hasClickHandler = events.includes('OnPointClick');

  const handleBarClick = useCallback(
    (index: number) => {
      if (hasClickHandler && onIvyEvent) {
        // Trigger the backend event with the clicked index
        onIvyEvent('OnPointClick', id, [index]);
      }
    },
    [hasClickHandler, onIvyEvent, id]
  );

  // Calculate the max value for scaling
  const maxValue = Math.max(...data, 1);

  return (
    <div className="rounded-lg border bg-card p-4 shadow-sm">
      {title && (
        <h3 className="mb-4 text-lg font-semibold text-foreground">{title}</h3>
      )}

      <div className="flex items-end gap-2" style={{ height: '12rem' }}>
        {data.map((value, index) => {
          const heightPercent = (value / maxValue) * 100;
          return (
            <div
              key={index}
              className="flex flex-col items-center justify-end flex-1 min-w-0 h-full"
            >
              <div
                className={`w-full rounded-t transition-all duration-200 ${
                  hasClickHandler
                    ? 'cursor-pointer hover:opacity-80'
                    : ''
                }`}
                style={{
                  height: `${heightPercent}%`,
                  backgroundColor: color,
                  minHeight: '4px',
                }}
                onClick={() => handleBarClick(index)}
                title={`Value: ${value}`}
              />
              {showLabels && (
                <span className="mt-1 text-xs text-muted-foreground truncate w-full text-center" style={{ flexShrink: 0 }}>
                  {value.toFixed(1)}
                </span>
              )}
            </div>
          );
        })}
      </div>

      {data.length === 0 && (
        <div className="flex items-center justify-center h-48 text-muted-foreground">
          No data available
        </div>
      )}
    </div>
  );
};

export default SuperChart;
