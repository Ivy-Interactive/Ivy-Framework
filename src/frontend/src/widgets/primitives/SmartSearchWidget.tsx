import { getHeight, getWidth } from '@/lib/styles';
import React, { useEffect, useLayoutEffect, useRef, useState } from 'react';
import { useSyncExternalStore } from 'react';
import { X } from 'lucide-react';
import type { MenuItem } from '@/types/widgets';
import {
  ResizablePanelGroup,
  ResizablePanel,
  ResizableHandle,
} from '@/components/ui/resizable';
import {
  filterMenuItemsForSearch,
  pickSuggestionsFromSections,
} from '@/widgets/layouts/sidebar/sidebarSearchFilter';
import { sidebarSearchStore } from '@/widgets/layouts/sidebar/sidebarSearchStore';
import { SidebarSearchResultsList } from '@/widgets/layouts/sidebar/SidebarSearchResultsList';
import { cn } from '@/lib/utils';
import { mcpPanelStore } from './mcpPanelStore';

interface SmartSearchSlots {
  SearchInput?: React.ReactNode[];
  AskButton?: React.ReactNode[];
  ClearInputButton?: React.ReactNode[];
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
  const clearInputButtonRef = useRef<HTMLDivElement>(null);
  const slots = slotsProp ?? {};
  const searchInput = slots.SearchInput;
  const askButton = slots.AskButton;
  const clearInputButton = slots.ClearInputButton;
  const resultsHeader = slots.ResultsHeader;
  const resultsContent = slots.ResultsContent;
  const clearButton = slots.ClearButton;
  const followUpChat = slots.FollowUpChat;
  const hasResults = resultsContent != null && clearButton != null;

  const [isOpen, setIsOpen] = useState(false);
  const [windowQuery, setWindowQuery] = useState('');
  const [pageSuggestionsSelectedIndex, setPageSuggestionsSelectedIndex] =
    useState(0);

  const closeSearchOverlay = React.useCallback(() => {
    setIsOpen(false);
    setWindowQuery('');
    clearInputButtonRef.current
      ?.querySelector<HTMLButtonElement>('button')
      ?.click();
  }, []);

  // Sync MCP panel store and close search overlay when MCP results arrive (so overlay doesn’t show again after panel close).
  useEffect(() => {
    mcpPanelStore.setOpen(hasResults);
    if (hasResults) {
      queueMicrotask(() => closeSearchOverlay());
    }
  }, [hasResults, closeSearchOverlay]);

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

  const SUGGESTIONS_COUNT = 10;
  const suggestionItems = React.useMemo(() => {
    const fullMenu =
      sidebarSearchState.fullMenuItems ?? sidebarSearchState.items;
    if (!fullMenu.length) {
      return {
        searchResultsItems: [] as MenuItem[],
        flatItems: [] as MenuItem[],
      };
    }
    const picked = pickSuggestionsFromSections(fullMenu, SUGGESTIONS_COUNT);
    const flatItems = picked.map(({ item, path }) => ({
      ...item,
      path: path || undefined,
    }));
    const searchResultsItems: MenuItem[] = [
      {
        label: '',
        variant: 'Default',
        checked: false,
        disabled: false,
        expanded: true,
        children: flatItems,
      } as MenuItem,
    ];
    return { searchResultsItems, flatItems };
  }, [sidebarSearchState.fullMenuItems, sidebarSearchState.items]);

  const hasPageSuggestions = windowFiltered.flatItems.length > 0;
  const queryTrimmed = windowQuery.trim();
  const hasSuggestions =
    suggestionItems.flatItems.length > 0 && queryTrimmed === '';
  const listToShow = hasPageSuggestions
    ? windowFiltered
    : hasSuggestions
      ? suggestionItems
      : null;
  const listFlatLength = listToShow?.flatItems.length ?? 0;
  const showMcpOption = windowQuery.trim() !== '';
  const effectiveLength = listFlatLength + (showMcpOption ? 1 : 0);
  const pageSuggestionsIndex =
    effectiveLength > 0
      ? Math.min(pageSuggestionsSelectedIndex, Math.max(0, effectiveLength - 1))
      : 0;

  const handleOverlayKeyDown = React.useCallback(
    (e: React.KeyboardEvent) => {
      if (effectiveLength === 0) return;
      if (e.key === 'ArrowDown') {
        e.preventDefault();
        e.stopPropagation();
        setPageSuggestionsSelectedIndex(i =>
          Math.min(i + 1, effectiveLength - 1)
        );
      } else if (e.key === 'ArrowUp') {
        e.preventDefault();
        e.stopPropagation();
        setPageSuggestionsSelectedIndex(i => Math.max(i - 1, 0));
      } else if (e.key === 'Enter') {
        if (pageSuggestionsIndex < listFlatLength) {
          const item = listToShow?.flatItems[pageSuggestionsIndex];
          if (item?.tag) {
            e.preventDefault();
            e.stopPropagation();
            sidebarSearchState.eventHandler('OnSelect', sidebarSearchState.id, [
              item.tag,
            ]);
            closeSearchOverlay();
          }
        } else {
          e.preventDefault();
          e.stopPropagation();
          document
            .querySelector<HTMLButtonElement>(
              '[data-testid="docs-smart-search-ask"]'
            )
            ?.click();
        }
      }
    },
    [
      effectiveLength,
      listFlatLength,
      listToShow?.flatItems,
      pageSuggestionsIndex,
      sidebarSearchState,
      closeSearchOverlay,
    ]
  );

  const hasResultsRef = useRef(hasResults);
  useEffect(() => {
    hasResultsRef.current = hasResults;
  }, [hasResults]);

  // Open the smart search window when the sidebar search input is focused or clicked (never let sidebar search receive input).
  // If the answer panel is open, close it first so the overlay can show.
  // Use mousedown (capture) so we run before the input gets focus; click/focus fire after focus has already moved.
  useEffect(() => {
    const openWindow = () => {
      if (hasResultsRef.current) {
        clearButtonRef.current
          ?.querySelector<HTMLButtonElement>('button')
          ?.click();
      }
      setIsOpen(true);
    };

    const handleMouseDown = (e: MouseEvent) => {
      const el = e.target as HTMLElement | null;
      if (el?.closest?.('[data-testid="sidebar-search"]')) {
        e.preventDefault();
        e.stopPropagation();
        openWindow();
      }
    };
    const handleFocus = (e: FocusEvent) => {
      const el = e.target as HTMLElement | null;
      if (el?.closest?.('[data-testid="sidebar-search"]')) {
        e.preventDefault();
        e.stopPropagation();
        openWindow();
      }
    };

    document.body.addEventListener('mousedown', handleMouseDown, true);
    document.body.addEventListener('focus', handleFocus, true);
    window.addEventListener('ivy-docs-open-smart-search', openWindow);

    return () => {
      document.body.removeEventListener('mousedown', handleMouseDown, true);
      document.body.removeEventListener('focus', handleFocus, true);
      window.removeEventListener('ivy-docs-open-smart-search', openWindow);
    };
  }, []);

  // Focus the search input once the overlay and slotted input are in the DOM.
  const focusSearchInput = React.useCallback(() => {
    document
      .querySelector<HTMLInputElement>(
        '[data-testid="docs-smart-search-input"]'
      )
      ?.focus();
  }, []);

  useLayoutEffect(() => {
    if (!isOpen) return;
    focusSearchInput();
    const fallback = setTimeout(focusSearchInput, 100);
    return () => clearTimeout(fallback);
  }, [isOpen, focusSearchInput]);

  // Scroll selected suggestion (or MCP option) into view when navigating with keys (overlay only).
  useEffect(() => {
    if (!isOpen || hasResults || effectiveLength === 0) return;
    if (pageSuggestionsIndex < listFlatLength) {
      const el = document.querySelector(
        `[data-sidebar-result-index="${pageSuggestionsIndex}"]`
      );
      el?.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
    } else {
      document
        .querySelector('[data-smart-search-mcp-option]')
        ?.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
    }
  }, [
    isOpen,
    hasResults,
    effectiveLength,
    listFlatLength,
    pageSuggestionsIndex,
  ]);

  // Close search window on Escape.
  useEffect(() => {
    if (!isOpen) return;
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') closeSearchOverlay();
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, closeSearchOverlay]);

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

  const closeMcpPanel = () => {
    mcpPanelStore.setOpen(false);
    clearButtonRef.current?.querySelector<HTMLButtonElement>('button')?.click();
  };

  const [mcpPanelWidth, setMcpPanelWidth] = useState<number | null>(null);
  const MIN_PANEL_WIDTH_REM = 30;
  const minPanelWidthPx = MIN_PANEL_WIDTH_REM * 16;

  useEffect(() => {
    if (!hasResults) return;

    const updateWidth = () => {
      const boundary = document.querySelector('[data-docs-content-boundary]');
      if (!boundary) {
        setMcpPanelWidth(null);
        return;
      }
      const rect = boundary.getBoundingClientRect();
      const available = window.innerWidth - rect.right;
      setMcpPanelWidth(Math.max(minPanelWidthPx, Math.round(available)));
    };

    updateWidth();
    const ro = new ResizeObserver(updateWidth);
    const boundary = document.querySelector('[data-docs-content-boundary]');
    if (boundary) ro.observe(boundary);
    window.addEventListener('resize', updateWidth);
    return () => {
      ro.disconnect();
      window.removeEventListener('resize', updateWidth);
    };
  }, [hasResults, minPanelWidthPx]);

  const panelContainerWidth = mcpPanelWidth ?? 28 * 16;
  const mcpPanelMinSizePct =
    panelContainerWidth > 0
      ? Math.min(100, (minPanelWidthPx / panelContainerWidth) * 100)
      : 20;
  const mcpPanelDefaultSizePct = (2 / 3) * 100;

  return (
    <div
      id={id}
      role="search"
      aria-label="Ivy docs smart search"
      style={styles}
      className="overflow-y-auto pt-4"
      data-testid={dataTestId}
    >
      <div ref={clearInputButtonRef} className="sr-only" aria-hidden>
        {clearInputButton}
      </div>
      {/* Search overlay: only search bar + page suggestions (no MCP results here). Hide when MCP results show (panel opens). */}
      {isOpen && !hasResults && (
        <div
          className="fixed inset-0 z-40 flex items-start justify-center bg-black/20 backdrop-blur-sm p-4 pt-6"
          onClick={closeSearchOverlay}
          role="presentation"
        >
          <div
            className="relative flex max-h-[85vh] w-full max-w-4xl flex-col overflow-hidden rounded-lg border border-border bg-background shadow-lg"
            role="dialog"
            aria-label="Search"
            onClick={e => e.stopPropagation()}
            onKeyDownCapture={handleOverlayKeyDown}
          >
            <div className="flex min-h-0 flex-1 flex-col p-4 pt-4">
              {/* 1. Search input: same as sidebar (Search variant with icon and kbd inside input) */}
              <div className="shrink-0 w-full">{searchInput}</div>
              {/* 2. Search results or suggestions (useful pages when input is empty) */}
              <div className="min-h-0 flex-1 overflow-y-auto pt-4">
                {listToShow != null && (
                  <SidebarSearchResultsList
                    items={listToShow.searchResultsItems}
                    flatItems={listToShow.flatItems}
                    selectedIndex={pageSuggestionsIndex}
                    setSelectedIndex={setPageSuggestionsSelectedIndex}
                    onSelect={tag => {
                      sidebarSearchState.eventHandler(
                        'OnSelect',
                        sidebarSearchState.id,
                        [tag]
                      );
                      closeSearchOverlay();
                    }}
                    activeTag={sidebarSearchState.activeTag}
                  />
                )}
              </div>
              {showMcpOption && (
                <>
                  <div className="shrink-0 border-t border-border" />
                  <p className="shrink-0 py-3 px-2 text-sm text-muted-foreground">
                    Search with Ivy MCP
                  </p>
                  <div
                    data-smart-search-mcp-option
                    role="button"
                    tabIndex={-1}
                    className={cn(
                      'shrink-0 flex flex-wrap items-center gap-2 rounded-selector cursor-pointer py-2 px-2',
                      pageSuggestionsIndex === listFlatLength && 'bg-accent/30'
                    )}
                    onMouseEnter={() =>
                      setPageSuggestionsSelectedIndex(listFlatLength)
                    }
                    onClick={() =>
                      document
                        .querySelector<HTMLButtonElement>(
                          '[data-testid="docs-smart-search-ask"]'
                        )
                        ?.click()
                    }
                  >
                    {askButton}
                    <span className="text-muted-foreground">
                      How to use {windowQuery.trim()}?
                    </span>
                  </div>
                </>
              )}
            </div>
          </div>
        </div>
      )}

      {/* MCP results: right-side panel (right of TOC); hide TOC when not enough space. Uses Ivy Resizable. */}
      {hasResults && (
        <div
          className="fixed top-0 right-0 bottom-0 z-30 flex flex-col bg-background shadow-lg"
          style={{
            width: `${panelContainerWidth}px`,
            minWidth: `${MIN_PANEL_WIDTH_REM}rem`,
          }}
          role="dialog"
          aria-label="AI answer"
        >
          <ResizablePanelGroup
            direction="horizontal"
            className="h-full w-full"
            autoSaveId="ivy-docs-mcp-panel"
          >
            <ResizablePanel
              defaultSize={100 - mcpPanelDefaultSizePct}
              minSize={0}
              maxSize={100}
            />
            <ResizableHandle
              withHandle
              className="w-1 shrink-0 border-l border-border bg-transparent hover:bg-transparent cursor-col-resize"
            />
            <ResizablePanel
              defaultSize={mcpPanelDefaultSizePct}
              minSize={mcpPanelMinSizePct}
              order={2}
            >
              <div className="flex min-w-0 h-full flex-col">
                <div ref={clearButtonRef} className="sr-only" aria-hidden>
                  {clearButton}
                </div>
                <div className="flex shrink-0 items-center justify-end gap-2 border-b border-border bg-muted/30 px-3 py-2">
                  <span className="mr-auto text-sm font-medium text-foreground">
                    Answer
                  </span>
                  <button
                    type="button"
                    aria-label="Close panel"
                    onClick={closeMcpPanel}
                    className="rounded p-2 text-muted-foreground hover:bg-accent hover:text-foreground focus:outline-none min-w-9 min-h-9"
                  >
                    <X className="h-4 w-4" />
                  </button>
                </div>
                <div className="min-w-0 min-h-0 flex-1 overflow-y-auto overflow-x-hidden p-4 flex flex-col wrap-break-word">
                  {resultsHeader != null && (
                    <div className="mb-4 min-w-0 border-b border-border pb-4 wrap-break-word">
                      {resultsHeader}
                    </div>
                  )}
                  <div className="min-w-0 wrap-break-word">
                    {resultsContent}
                  </div>
                </div>
                {followUpChat != null && followUpChat.length > 0 && (
                  <div className="shrink-0 border-t border-border bg-background p-4 wrap-break-word">
                    {followUpChat}
                  </div>
                )}
              </div>
            </ResizablePanel>
          </ResizablePanelGroup>
        </div>
      )}
    </div>
  );
};
