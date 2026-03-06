import { getHeight, getWidth } from '@/lib/styles';
import React, { useEffect } from 'react';

interface SmartSearchSlots {
  SearchInput?: React.ReactNode[];
  AskButton?: React.ReactNode[];
  ClearInputButton?: React.ReactNode[];
  OpenTrigger?: React.ReactNode[];
  ResultsHeader?: React.ReactNode[];
  ResultsContent?: React.ReactNode[];
  ClearButton?: React.ReactNode[];
  FollowUpChat?: React.ReactNode[];
}

interface SmartSearchWidgetProps {
  id: string;
  slots?: SmartSearchSlots;
  width?: string;
  height?: string;
  'data-testid'?: string;
}

/** Opens the smart search overlay by triggering the backend (clicks OpenTrigger so backend sends overlay as Sheet). */
function openSmartSearchOverlay(): void {
  document
    .querySelector<HTMLButtonElement>(
      '[data-testid="docs-smart-search-open-trigger"]'
    )
    ?.click();
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

  const slots = slotsProp ?? {};
  const clearInputButton = slots.ClearInputButton;
  const openTrigger = slots.OpenTrigger;

  // When the sidebar search is focused or ivy-docs-open-smart-search fires, open the overlay by notifying the backend.
  useEffect(() => {
    const handleMouseDown = (e: MouseEvent) => {
      const el = e.target as HTMLElement | null;
      if (el?.closest?.('[data-testid="sidebar-search"]')) {
        e.preventDefault();
        e.stopPropagation();
        openSmartSearchOverlay();
      }
    };
    const handleFocus = (e: FocusEvent) => {
      const el = e.target as HTMLElement | null;
      if (el?.closest?.('[data-testid="sidebar-search"]')) {
        e.preventDefault();
        e.stopPropagation();
        openSmartSearchOverlay();
      }
    };

    document.body.addEventListener('mousedown', handleMouseDown, true);
    document.body.addEventListener('focus', handleFocus, true);
    window.addEventListener(
      'ivy-docs-open-smart-search',
      openSmartSearchOverlay
    );

    return () => {
      document.body.removeEventListener('mousedown', handleMouseDown, true);
      document.body.removeEventListener('focus', handleFocus, true);
      window.removeEventListener(
        'ivy-docs-open-smart-search',
        openSmartSearchOverlay
      );
    };
  }, []);

  return (
    <div
      id={id}
      role="search"
      aria-label="Ivy docs smart search"
      style={styles}
      className="overflow-y-auto pt-4"
      data-testid={dataTestId}
    >
      <div className="sr-only" aria-hidden>
        {clearInputButton}
      </div>
      <div className="sr-only" aria-hidden>
        {openTrigger}
      </div>
      {/* Search input and ask button are used in the sidebar and in the overlay/answer Sheet content from the backend. */}
    </div>
  );
};
