import Icon from '@/components/Icon';
import { useEventHandler } from '@/components/event-handler';
import { getWidth } from '@/lib/styles';
import { cn } from '@/lib/utils';
import React from 'react';

export interface StepperItem {
  symbol: string;
  icon?: string;
  label?: string;
  description?: string;
  loading?: boolean;
}

interface StepperWidgetProps {
  id: string;
  selectedIndex?: number;
  items: StepperItem[];
  width?: string;
  allowSelectForward?: boolean;
  events?: string[];
}

export const StepperWidget: React.FC<StepperWidgetProps> = ({
  id,
  selectedIndex = 0,
  items,
  width,
  allowSelectForward = false,
  events = [],
}) => {
  const eventHandler = useEventHandler();
  const hasSelectHandler = events.includes('OnSelect');

  const handleSelect = (index: number) => {
    if (!hasSelectHandler) return;
    if (index === selectedIndex) return; // Current step not clickable
    if (index > selectedIndex && !allowSelectForward) return; // Upcoming only if allowed
    eventHandler('OnSelect', id, [index]);
  };

  const styles: React.CSSProperties = {
    ...getWidth(width),
  };

  const getStepState = (index: number) => {
    if (selectedIndex === null || selectedIndex === undefined) {
      return 'upcoming';
    }
    if (index < selectedIndex) return 'completed';
    if (index === selectedIndex) return 'current';
    return 'upcoming';
  };

  return (
    <div key={id} style={styles} className="flex flex-col w-full">
      {/* Row 1: Circles and lines */}
      <div className="flex items-center w-full">
        {items.map((item, index) => {
          const state = getStepState(index);
          const isLast = index === items.length - 1;
          const isLineCompleted = index < selectedIndex;
          const isClickable =
            hasSelectHandler &&
            (state === 'completed' ||
              (state === 'upcoming' && allowSelectForward));

          return (
            <React.Fragment key={index}>
              <button
                type="button"
                onClick={() => handleSelect(index)}
                disabled={!isClickable}
                className={cn(
                  'relative z-10 flex-shrink-0 flex items-center justify-center w-8 h-8 rounded-full border-2 text-sm font-medium transition-all bg-background',
                  state === 'completed' &&
                    isClickable &&
                    'border-primary bg-primary text-primary-foreground cursor-pointer hover:scale-110 hover:shadow-md',
                  state === 'completed' &&
                    !isClickable &&
                    'border-primary bg-primary text-primary-foreground',
                  state === 'current' &&
                    'border-primary bg-primary text-primary-foreground',
                  state === 'upcoming' &&
                    isClickable &&
                    'border-muted-foreground/30 text-muted-foreground/50 cursor-pointer hover:border-primary/50 hover:text-muted-foreground hover:scale-105',
                  state === 'upcoming' &&
                    !isClickable &&
                    'border-muted-foreground/30 text-muted-foreground/50'
                )}
              >
                {item.icon ? (
                  <Icon name={item.icon} size={16} />
                ) : (
                  item.symbol || index + 1
                )}
              </button>

              {/* Connector line between steps */}
              {!isLast && (
                <div
                  className={cn(
                    'flex-1 h-0.5 mx-2',
                    isLineCompleted ? 'bg-primary' : 'bg-muted-foreground/30'
                  )}
                />
              )}
            </React.Fragment>
          );
        })}
      </div>

      {/* Row 2: Labels and descriptions - mirrors the structure of row 1 */}
      <div className="flex items-start w-full mt-2">
        {items.map((item, index) => {
          const state = getStepState(index);
          const isFirst = index === 0;
          const isLast = index === items.length - 1;

          return (
            <React.Fragment key={index}>
              {/* Label container - same width as circle (w-8) */}
              <div
                className={cn(
                  'flex flex-col flex-shrink-0 w-8',
                  isFirst && 'items-start',
                  isLast && 'items-end',
                  !isFirst && !isLast && 'items-center'
                )}
              >
                <div
                  className={cn(
                    'flex flex-col whitespace-nowrap',
                    isFirst && 'items-start text-left',
                    isLast && 'items-end text-right',
                    !isFirst && !isLast && 'items-center text-center'
                  )}
                >
                  {item.label && (
                    <span
                      className={cn(
                        'text-sm font-medium',
                        state === 'upcoming' && 'text-muted-foreground/50'
                      )}
                    >
                      {item.label}
                    </span>
                  )}
                  {item.description && (
                    <span
                      className={cn(
                        'text-sm',
                        state === 'upcoming'
                          ? 'text-muted-foreground/40'
                          : 'text-muted-foreground'
                      )}
                    >
                      {item.description}
                    </span>
                  )}
                </div>
              </div>

              {/* Spacer to match the line width */}
              {!isLast && <div className="flex-1 mx-2" />}
            </React.Fragment>
          );
        })}
      </div>
    </div>
  );
};

export default StepperWidget;
