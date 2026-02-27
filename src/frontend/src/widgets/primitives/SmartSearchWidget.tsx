import { getHeight, getWidth } from '@/lib/styles';
import React, { useEffect, useRef, useState } from 'react';
import { useSyncExternalStore } from 'react';
import { X } from 'lucide-react';
import type { MenuItem } from '@/types/widgets';
import { filterMenuItemsForSearch } from '@/widgets/layouts/sidebar/sidebarSearchFilter';
import { sidebarSearchStore } from '@/widgets/layouts/sidebar/sidebarSearchStore';
import { SidebarSearchResultsList } from '@/widgets/layouts/sidebar/SidebarSearchResultsList';

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

  const [isOpen, setIsOpen] = useState(false);
  const [windowQuery, setWindowQuery] = useState('');
  const [pageSuggestionsSelectedIndex, setPageSuggestionsSelectedIndex] =
    useState(0);

  const sidebarSearchState = useSyncExternalStore(
    sidebarSearchStore.subscribe,
    sidebarSearchStore.getState,
    sidebarSearchStore.getState
  );

  // Filter full menu by window query only (sidebar is unchanged)
  const windowFiltered = React.useMemo(() => {
    const q = windowQuery.trim();
    const fullMenu =
      sidebarSearchState.fullMenuItems ?? sidebarSearchState.items;
    if (!q || !fullMenu.length) {
      return {
        searchResultsItems: [] as MenuItem[],
        flatItems: [] as MenuItem[],
      };
    }
    return filterMenuItemsForSearch(fullMenu, q);
  }, [windowQuery, sidebarSearchState.fullMenuItems, sidebarSearchState.items]);

  const hasPageSuggestions = windowFiltered.flatItems.length > 0;
  const pageSuggestionsIndex = hasPageSuggestions
    ? Math.min(
        pageSuggestionsSelectedIndex,
        Math.max(0, windowFiltered.flatItems.length - 1)
      )
    : 0;

  // Open the smart search window when the sidebar search input is focused or clicked.
  // The sidebar search is rendered by the Ivy widget tree (data-testid="sidebar-search"),
  // not by SidebarInput from the UI library, so we use event delegation.
  useEffect(() => {
    const openWindow = () => setIsOpen(true);

    const handleFocus = (e: FocusEvent) => {
      const el = e.target as HTMLElement | null;
      if (el?.closest?.('[data-testid="sidebar-search"]')) openWindow();
    };
    const handleClick = (e: MouseEvent) => {
      const el = e.target as HTMLElement | null;
      if (el?.closest?.('[data-testid="sidebar-search"]')) openWindow();
    };

    document.body.addEventListener('focus', handleFocus, true);
    document.body.addEventListener('click', handleClick, true);
    window.addEventListener('ivy-docs-open-smart-search', openWindow);

    return () => {
      document.body.removeEventListener('focus', handleFocus, true);
      document.body.removeEventListener('click', handleClick, true);
      window.removeEventListener('ivy-docs-open-smart-search', openWindow);
    };
  }, []);

  // Focus the search input once the window (and search bar) is in the DOM.
  useEffect(() => {
    if (!isOpen) return;
    const id = requestAnimationFrame(() => {
      document
        .querySelector<HTMLInputElement>(
          '[data-testid="docs-smart-search-input"]'
        )
        ?.focus();
    });
    return () => cancelAnimationFrame(id);
  }, [isOpen]);

  // Track window search query so we can filter page suggestions only in the window (sidebar stays unchanged).
  useEffect(() => {
    if (!isOpen && !hasResults) return;

    const smartInput = document.querySelector<HTMLInputElement>(
      '[data-testid="docs-smart-search-input"]'
    );
    if (!smartInput) return;

    const handleInput = () => setWindowQuery(smartInput.value);
    smartInput.addEventListener('input', handleInput);
    return () => smartInput.removeEventListener('input', handleInput);
  }, [isOpen, hasResults]);

  // When there are no page matches in the window, send query to MCP after debounce.
  useEffect(() => {
    const q = windowQuery.trim();
    if (q.length < 2 || windowFiltered.flatItems.length > 0) return;

    const t = setTimeout(() => {
      window.dispatchEvent(
        new CustomEvent('ivy-docs-auto-ask-question', { detail: { query: q } })
      );
    }, 400);
    return () => clearTimeout(t);
  }, [windowQuery, windowFiltered.flatItems.length]);

  // When there are no page matches, sidebar dispatches ivy-docs-auto-ask-question.
  // Set the smart search input to that query and trigger Ask so MCP is called.
  const lastAutoAskQueryRef = useRef<string | null>(null);
  useEffect(() => {
    const handler = (event: Event) => {
      const detail = (event as CustomEvent<{ query?: string }>).detail;
      const query = detail?.query?.trim();
      if (!query || query === lastAutoAskQueryRef.current) return;

      const input = document.querySelector<HTMLInputElement>(
        '[data-testid="docs-smart-search-input"]'
      );
      const askButton = document.querySelector<HTMLButtonElement>(
        '[data-testid="docs-smart-search-ask"]'
      );
      if (!input || !askButton) return;

      lastAutoAskQueryRef.current = query;

      const descriptor =
        Object.getOwnPropertyDescriptor(Object.getPrototypeOf(input), 'value')
          ?.set ??
        Object.getOwnPropertyDescriptor(HTMLInputElement.prototype, 'value')
          ?.set;
      if (descriptor) {
        descriptor.call(input, query);
        input.dispatchEvent(new Event('input', { bubbles: true }));
      }
      askButton.click();
    };

    window.addEventListener(
      'ivy-docs-auto-ask-question',
      handler as EventListener
    );
    return () => {
      window.removeEventListener(
        'ivy-docs-auto-ask-question',
        handler as EventListener
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
      {(isOpen || hasResults) && (
        <div className="fixed inset-0 z-40 flex items-center justify-center bg-black/20 backdrop-blur-sm p-4">
          <div
            className="relative flex max-h-[85vh] w-full max-w-4xl flex-col overflow-hidden rounded-lg border border-border bg-background shadow-lg"
            role="dialog"
            aria-label="Search"
          >
            <div ref={clearButtonRef} className="sr-only" aria-hidden>
              {clearButton}
            </div>
            <div className="absolute top-0 right-0 z-50 p-4">
              <button
                type="button"
                aria-label="Close"
                onClick={() => {
                  setIsOpen(false);
                  setWindowQuery('');
                  lastAutoAskQueryRef.current = null;
                  clearButtonRef.current
                    ?.querySelector<HTMLButtonElement>('button')
                    ?.click();
                }}
                className="p-2 rounded-md hover:bg-accent focus:outline-none cursor-pointer text-muted-foreground hover:text-foreground min-w-9 min-h-9"
              >
                <X className="h-4 w-4" />
              </button>
            </div>
            <div className="shrink-0 border-b border-border p-4 pt-12">
              <div className="rounded-lg border border-border/40 bg-muted/30 p-2">
                {searchBar}
              </div>
            </div>
            <div className="min-h-0 flex-1 overflow-y-auto p-4">
              {hasPageSuggestions && (
                <div className="mb-4">
                  <h3 className="text-sm font-medium text-muted-foreground mb-2">
                    Pages
                  </h3>
                  <SidebarSearchResultsList
                    items={windowFiltered.searchResultsItems}
                    flatItems={windowFiltered.flatItems}
                    selectedIndex={pageSuggestionsIndex}
                    setSelectedIndex={setPageSuggestionsSelectedIndex}
                    onSelect={tag => {
                      sidebarSearchState.eventHandler(
                        'OnSelect',
                        sidebarSearchState.id,
                        [tag]
                      );
                      setIsOpen(false);
                    }}
                    activeTag={sidebarSearchState.activeTag}
                  />
                </div>
              )}
              {hasPageSuggestions &&
                (resultsHeader != null || resultsContent) && (
                  <hr className="my-4 border-border" />
                )}
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
