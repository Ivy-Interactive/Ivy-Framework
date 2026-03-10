import { Dialog, DialogContent } from '@/components/ui/dialog';
import { getHeight, getWidth } from '@/lib/styles';
import { cn } from '@/lib/utils';
import React, { useEffect, useRef } from 'react';

interface SmartSearchSlots {
  SearchInput?: React.ReactNode[];
  AskButton?: React.ReactNode[];
  ClearInputButton?: React.ReactNode[];
  OpenTrigger?: React.ReactNode[];
  CloseOverlay?: React.ReactNode[];
  OverlayPanel?: React.ReactNode[];
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
  const overlayPanel = slots.OverlayPanel;
  const dialogContentRef = useRef<HTMLDivElement>(null);
  const focusRafRef = useRef<number | null>(null);

  // Focus search input when overlay opens. Portal/async tree: rAF loop stops when input exists or effect cleans up.
  useEffect(() => {
    if (!overlayPanel || overlayPanel.length === 0) return;

    let cancelled = false;

    const tryFocus = () => {
      if (cancelled) return;
      if (focusRafRef.current != null) {
        cancelAnimationFrame(focusRafRef.current);
        focusRafRef.current = null;
      }

      const root = dialogContentRef.current;
      if (root) {
        const input = root.querySelector<HTMLInputElement>(
          'input:not([type="hidden"]):not([disabled])'
        );
        if (input) {
          input.focus();
          return;
        }
      }

      focusRafRef.current = requestAnimationFrame(tryFocus);
    };

    focusRafRef.current = requestAnimationFrame(tryFocus);

    return () => {
      cancelled = true;
      if (focusRafRef.current != null) {
        cancelAnimationFrame(focusRafRef.current);
        focusRafRef.current = null;
      }
    };
  }, [overlayPanel]);

  const closeOverlay = () => {
    document
      .querySelector<HTMLButtonElement>(
        '[data-testid="docs-smart-search-close-overlay"]'
      )
      ?.click();
  };

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
    <>
      {overlayPanel && overlayPanel.length > 0 && (
        <Dialog open={true} onOpenChange={() => closeOverlay()}>
          <DialogContent
            ref={dialogContentRef}
            style={{
              width: '36rem',
              maxWidth: 'min(36rem, calc(100vw - 2rem))',
            }}
            className={cn(
              'alert-animate-enter',
              '!top-4 sm:!top-8 !translate-y-0'
            )}
          >
            {overlayPanel}
          </DialogContent>
        </Dialog>
      )}
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
        <div className="sr-only" aria-hidden>
          {slots.CloseOverlay}
        </div>
      </div>
    </>
  );
};
