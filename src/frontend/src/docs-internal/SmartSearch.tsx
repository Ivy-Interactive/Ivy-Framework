/**
 * Renderer for Ivy.Docs.Shared.Internal.SmartSearch (C#). Docs-only overlay + sidebar
 * hijack; not a reusable widget, so it lives under docs-internal instead of widgets/.
 */
import { Dialog, DialogContent } from '@/components/ui/dialog';
import { getHeight, getWidth } from '@/lib/styles';
import { cn } from '@/lib/utils';
import React, { useEffect, useRef } from 'react';

function getSmartSearchListButtons(root: HTMLElement): HTMLButtonElement[] {
  // We only want the list items, not the "Ask Agent" button in the footer or the dialog close 'X'
  // Since list items are rendered using Ivy.ListItem, we can explicitly query for those wrappers
  const listContainer = root.querySelector('[data-smart-search-list]');
  if (!listContainer) return [];
  return Array.from(
    listContainer.querySelectorAll(
      'ivy-widget[type="Ivy.ListItem"] > button, ivy-widget[type="Ivy.ListItem"] > a'
    )
  ) as HTMLButtonElement[];
}

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

export interface SmartSearchProps {
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

/** Registered in widgetMap as Ivy.Docs.Shared.Internal.SmartSearch */
export const SmartSearch: React.FC<SmartSearchProps> = ({
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

  const selectedIndexRef = useRef(0);

  useEffect(() => {
    if (!overlayPanel?.length) return;

    // Reset selected index on list change
    selectedIndexRef.current = 0;
    // We add a class to the DOM list element to mark it, and if it already has it, we know the list hasn't literally just mounted
    const attemptHighlight = () => {
      const root = dialogContentRef.current;
      if (!root) {
        requestAnimationFrame(attemptHighlight);
        return;
      }
      const buttons = getSmartSearchListButtons(root);
      if (buttons.length === 0) return;

      buttons.forEach((btn, i) => {
        btn.onmouseenter = () => {
          selectedIndexRef.current = i;
          buttons.forEach((b, j) => {
            b.classList.toggle('bg-accent', selectedIndexRef.current === j);
            b.classList.toggle(
              'text-accent-foreground',
              selectedIndexRef.current === j
            );
          });
        };
        btn.classList.toggle('bg-accent', i === selectedIndexRef.current);
        btn.classList.toggle(
          'text-accent-foreground',
          i === selectedIndexRef.current
        );
      });

      if (buttons[selectedIndexRef.current]) {
        buttons[selectedIndexRef.current].scrollIntoView({
          block: 'nearest',
          behavior: 'smooth',
        });
      }
    };

    requestAnimationFrame(attemptHighlight);

    const observer = new MutationObserver(() => {
      selectedIndexRef.current = 0;
      attemptHighlight();
    });

    const root = dialogContentRef.current;
    if (root) {
      observer.observe(root, {
        childList: true,
        subtree: true,
        characterData: true,
      });
    }

    return () => {
      observer.disconnect();
    };
  }, [overlayPanel]);

  useEffect(() => {
    const onKeyDownCapture = (e: KeyboardEvent) => {
      if (
        e.key !== 'ArrowDown' &&
        e.key !== 'ArrowUp' &&
        e.key !== 'Enter' &&
        e.key !== 'Tab'
      )
        return;

      // Ensure we process earlier than the SearchVariant's standard React onKeyDown handler
      // We check if the input is our search input OR we are inside the dialog
      const isSearchInput =
        e.target instanceof HTMLInputElement && e.target.type === 'search';

      const isDialogContent = !!(e.target as HTMLElement | null)?.closest?.(
        '[data-smart-search-overlay]'
      );

      if (!isSearchInput && !isDialogContent) return;

      const overlayRoot = document.querySelector(
        '[data-smart-search-overlay]'
      ) as HTMLElement | null;
      // If there is no overlay rendering, we cannot handle Arrow/Enter for SmartSearch results
      if (!overlayRoot) return;

      const buttons = getSmartSearchListButtons(overlayRoot);

      if (buttons.length === 0 && e.key === 'Enter') {
        if (isSearchInput) {
          // Stop SearchVariant from blurring the input
          e.preventDefault();
          e.stopPropagation();
          e.stopImmediatePropagation();
          overlayRoot
            .querySelector<HTMLElement>('[data-testid="docs-smart-search-ask"]')
            ?.click();
        }
        return;
      }

      if (buttons.length === 0) return;

      if (e.key === 'Enter') {
        e.preventDefault();
        e.stopPropagation();
        e.stopImmediatePropagation();
        buttons[selectedIndexRef.current]?.click();
        return;
      }

      if (e.key === 'ArrowDown' || (e.key === 'Tab' && !e.shiftKey)) {
        e.preventDefault();
        e.stopPropagation();
        selectedIndexRef.current = Math.min(
          selectedIndexRef.current + 1,
          buttons.length - 1
        );
      } else if (e.key === 'ArrowUp' || (e.key === 'Tab' && e.shiftKey)) {
        e.preventDefault();
        e.stopPropagation();
        selectedIndexRef.current = Math.max(selectedIndexRef.current - 1, 0);
      }

      buttons.forEach((b, j) => {
        b.classList.toggle('bg-accent', selectedIndexRef.current === j);
        b.classList.toggle(
          'text-accent-foreground',
          selectedIndexRef.current === j
        );
      });

      if (buttons[selectedIndexRef.current]) {
        buttons[selectedIndexRef.current].scrollIntoView({
          block: 'nearest',
          behavior: 'smooth',
        });
      }

      if (
        !isSearchInput &&
        isDialogContent &&
        document.activeElement instanceof HTMLInputElement
      ) {
        document.activeElement.focus();
      }
    };

    document.addEventListener('keydown', onKeyDownCapture, true);
    return () =>
      document.removeEventListener('keydown', onKeyDownCapture, true);
  }, []);

  const closeOverlay = () => {
    document
      .querySelector<HTMLButtonElement>(
        '[data-testid="docs-smart-search-close-overlay"]'
      )
      ?.click();
  };

  const isClosingRef = useRef(false);

  useEffect(() => {
    if (overlayPanel && overlayPanel.length > 0) {
      isClosingRef.current = false;
      return () => {
        isClosingRef.current = true;
        setTimeout(() => {
          isClosingRef.current = false;
        }, 200);
      };
    }
  }, [overlayPanel]);

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
      // Prevent focus loops triggered by Radix returning focus abruptly upon unmount
      if (isClosingRef.current) return;
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
            onCloseAutoFocus={e => e.preventDefault()}
            data-smart-search-overlay
            style={{
              width: '36rem',
              maxWidth: 'min(36rem, calc(100vw - 2rem))',
            }}
            className={cn(
              'alert-animate-enter',
              '!top-4 sm:!top-8 !translate-y-0'
            )}
          >
            <div data-smart-search-list>{overlayPanel}</div>
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
