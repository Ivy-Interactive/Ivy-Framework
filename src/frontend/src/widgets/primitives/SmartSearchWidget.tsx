import { getHeight, getWidth } from '@/lib/styles';
import { cn } from '@/lib/utils';
import React from 'react';

interface SmartSearchWidgetProps {
  id: string;
  children?: React.ReactNode;
  width?: string;
  height?: string;
  'data-testid'?: string;
}

/**
 * Internal Ivy.Docs widget: AI search with inline results (no sheet).
 * Renders search bar and results below with fade-in animation.
 */
export const SmartSearchWidget: React.FC<SmartSearchWidgetProps> = ({
  id,
  children,
  width = 'Full',
  height = 'Full',
  'data-testid': dataTestId,
}) => {
  const styles = {
    ...getWidth(width),
    ...getHeight(height),
  };

  const childArray = React.Children.toArray(children);
  const searchBar = childArray[0];
  const resultsContent = childArray.length > 1 ? childArray.slice(1) : null;

  return (
    <div
      id={id}
      role="search"
      aria-label="Ivy docs smart search"
      style={styles}
      className="overflow-y-auto pt-4"
      data-testid={dataTestId}
    >
      <div className="relative mx-auto flex max-w-3xl flex-col gap-6 px-4 sm:px-6 lg:px-8">
        {/* Search bar */}
        <div className="w-full">{searchBar}</div>

        {/* Results: animate in when present (Bing-style) */}
        {resultsContent != null && resultsContent.length > 0 && (
          <div
            className={cn(
              'min-h-0 w-full',
              'animate-in fade-in-0 slide-in-from-top-2 duration-300 ease-out',
              'flex flex-col gap-4'
            )}
          >
            {resultsContent}
          </div>
        )}
      </div>
    </div>
  );
};
