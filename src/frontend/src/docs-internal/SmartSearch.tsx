/**
 * Renderer for Ivy.Docs.Shared.Internal.SmartSearch (C#). Docs-only overlay + sidebar
 * hijack; not a reusable widget, so it lives under docs-internal instead of widgets/.
 */
import { Dialog, DialogContent } from '@/components/ui/dialog';
import { getHeight, getWidth } from '@/lib/styles';
import { cn } from '@/lib/utils';
import React, { useEffect, useRef } from 'react';

function getSmartSearchListButtons(root: HTMLElement): HTMLButtonElement[] {
  const body = root.querySelector('section[role="document"]');
  if (!body) return [];
  return Array.from(body.querySelectorAll('button')).filter(
    btn => !btn.closest('.rounded-field')
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

  useEffect(() => {
    if (!overlayPanel?.length) return;

    const onKeyDownCapture = (e: KeyboardEvent) => {
      const rootEl = (e.target as HTMLElement | null)?.closest?.(
        '[data-smart-search-overlay]'
      );
      if (!rootEl || !(rootEl instanceof HTMLElement)) return;
      const root = rootEl;
      if (e.key !== 'ArrowDown' && e.key !== 'ArrowUp' && e.key !== 'Enter')
        return;

      const input = root.querySelector<HTMLInputElement>(
        'input:not([type="hidden"]):not([disabled])'
      );
      const active = document.activeElement;
      const isInputActive =
        active === input ||
        (e.target instanceof HTMLInputElement && e.target === input);

      // Intercept before SearchVariant blurs → sidebarMenuRef (capture phase).
      if (isInputActive) {
        if (e.key === 'Enter') {
          e.preventDefault();
          e.stopPropagation();
          root
            .querySelector<HTMLElement>('[data-testid="docs-smart-search-ask"]')
            ?.click();
          return;
        }
        const buttons = getSmartSearchListButtons(root);
        if (e.key === 'ArrowDown' && buttons.length > 0) {
          e.preventDefault();
          e.stopPropagation();
          const first = buttons[0];
          first?.focus();
          first?.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
          return;
        }
        // No list yet, or ArrowUp in field: do not blur into sidebar.
        if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
          e.preventDefault();
          e.stopPropagation();
        }
        return;
      }

      const buttons = getSmartSearchListButtons(root);
      if (buttons.length === 0) return;

      const idx = buttons.indexOf(active as HTMLButtonElement);

      if (e.key === 'Enter' && idx >= 0) {
        e.preventDefault();
        e.stopPropagation();
        (active as HTMLButtonElement).click();
        return;
      }

      if (e.key === 'ArrowDown') {
        if (idx >= 0 && idx < buttons.length - 1) {
          e.preventDefault();
          e.stopPropagation();
          const next = buttons[idx + 1];
          next?.focus();
          next?.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
        }
        return;
      }

      if (e.key === 'ArrowUp') {
        if (idx === 0) {
          e.preventDefault();
          e.stopPropagation();
          input?.focus();
          return;
        }
        if (idx > 0) {
          e.preventDefault();
          e.stopPropagation();
          const prev = buttons[idx - 1];
          prev?.focus();
          prev?.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
        }
      }
    };

    document.addEventListener('keydown', onKeyDownCapture, true);
    return () =>
      document.removeEventListener('keydown', onKeyDownCapture, true);
  }, [overlayPanel]);

  const closeOverlay = () => {
    document
      .querySelector<HTMLButtonElement>(
        '[data-testid="docs-smart-search-close-overlay"]'
      )
      ?.click();
  };

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
