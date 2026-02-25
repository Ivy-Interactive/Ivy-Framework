import { getHeight, getWidth } from '@/lib/styles';
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
      className="overflow-y-auto"
      data-testid={dataTestId}
    >
      {/* Search bar pinned at top – fixed; half-transparent background only under the bar (not full width) */}
      <div className="fixed top-0 z-50 w-full max-w-3xl pt-4">
        <div className="rounded-lg border border-border/40 bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/80">
          <div className="w-full">{searchBar}</div>
        </div>
      </div>
      {/* Spacer so content starts below the pinned bar */}
      <div className="h-8 shrink-0" aria-hidden />
      {/* Results in FloatingPanel when present */}
      {resultsContent != null && resultsContent.length > 0 && resultsContent}
    </div>
  );
};
