import { getHeight, getWidth } from '@/lib/styles';
import React, { useRef } from 'react';
import { X } from 'lucide-react';

interface SmartSearchSlots {
  SearchBar?: React.ReactNode[];
  ResultsHeader?: React.ReactNode[];
  ResultsContent?: React.ReactNode[];
  ClearButton?: React.ReactNode[];
}

interface SmartSearchWidgetProps {
  id: string;
  slots?: SmartSearchSlots;
  width?: string;
  height?: string;
  'data-testid'?: string;
}

export const SmartSearchWidget: React.FC<SmartSearchWidgetProps> = ({
  id,
  slots: slotsProp,
  width = 'Full',
  height = 'Full',
  'data-testid': dataTestId,
}) => {
  const styles = {
    ...getWidth(width),
    ...getHeight(height),
  };

  const clearButtonRef = useRef<HTMLDivElement>(null);
  const slots = slotsProp ?? {};
  const searchBar = slots.SearchBar;
  const resultsHeader = slots.ResultsHeader;
  const resultsContent = slots.ResultsContent;
  const clearButton = slots.ClearButton;
  const hasResults = resultsContent != null && clearButton != null;

  return (
    <div
      id={id}
      role="search"
      aria-label="Ivy docs smart search"
      style={styles}
      className="overflow-y-auto pt-4"
      data-testid={dataTestId}
    >
      <div className="fixed top-0 z-50 w-full max-w-3xl pt-4 pb-4">
        <div className="rounded-lg border border-border/40 bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/80">
          <div className="w-full">{searchBar}</div>
        </div>
      </div>
      {hasResults && (
        <div className="fixed inset-0 z-40 flex items-center justify-center bg-black/20 backdrop-blur-sm">
          <div
            className="relative flex max-h-[85vh] w-[90vw] max-w-4xl flex-col overflow-hidden rounded-lg border border-border bg-background shadow-lg"
            role="dialog"
            aria-label="Search results"
          >
            <div ref={clearButtonRef} className="sr-only" aria-hidden>
              {clearButton}
            </div>
            <div className="absolute top-0 right-0 z-50 p-6">
              <button
                type="button"
                aria-label="Clear"
                onClick={() =>
                  clearButtonRef.current?.querySelector('button')?.click()
                }
                className="p-2 rounded-md hover:bg-accent focus:outline-none cursor-pointer text-muted-foreground hover:text-foreground min-w-9 min-h-9"
              >
                <X className="h-4 w-4" />
              </button>
            </div>
            <div className="min-h-0 flex-1 overflow-y-auto p-4">
              {resultsHeader != null && (
                <div className="mb-4 border-b border-border pb-4">
                  {resultsHeader}
                </div>
              )}
              {resultsContent}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
