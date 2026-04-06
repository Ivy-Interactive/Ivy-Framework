import { useCallback, useEffect, useMemo, useState } from "react";
import {
  CompactSelection,
  DataEditorRef,
  GridSelection,
  Highlight,
  Item,
} from "@glideapps/glide-data-grid";

const activeMatchColor = (isDark: boolean) => (isDark ? "#eab308" : "#fde047");

function singleCellGridSelection(dataCol: number, row: number): GridSelection {
  return {
    current: {
      cell: [dataCol, row],
      range: { x: dataCol, y: row, width: 1, height: 1 },
      rangeStack: [],
    },
    columns: CompactSelection.empty(),
    rows: CompactSelection.empty(),
  };
}

function clampPct(n: number): number {
  return Math.min(100, Math.max(0, Math.round(n)));
}

export function useSearchNavigation(
  gridRef: React.RefObject<DataEditorRef | null>,
  containerRef: React.RefObject<HTMLDivElement | null>,
  setGridSelection: (selection: GridSelection) => void,
  isDark: boolean,
  showSearch: boolean,
  setShowSearch: React.Dispatch<React.SetStateAction<boolean>>,
) {
  const [activeCell, setActiveCell] = useState<[number, number] | null>(null);

  const setMatchProgressOnBar = useCallback(
    (pct: number) => {
      const bar = containerRef.current?.querySelector(".gdg-search-bar") as HTMLElement | null;
      bar?.style.setProperty("--ivy-search-match-progress", `${clampPct(pct)}%`);
    },
    [containerRef],
  );

  useEffect(() => {
    if (!showSearch) setMatchProgressOnBar(0);
  }, [showSearch, setMatchProgressOnBar]);

  const onSearchResultsChanged = useCallback(
    (results: readonly Item[], navIndex: number) => {
      if (results.length === 0) {
        setActiveCell(null);
        setMatchProgressOnBar(0);
        return;
      }
      if (navIndex >= 0 && navIndex < results.length) {
        setMatchProgressOnBar(((navIndex + 1) / results.length) * 100);
        const [dataCol, row] = results[navIndex];
        setActiveCell([dataCol, row]);
        setGridSelection(singleCellGridSelection(dataCol, row));
        gridRef.current?.scrollTo(dataCol, row, "both", 0, 0);
      } else {
        setMatchProgressOnBar(0);
      }
    },
    [gridRef, setGridSelection, setMatchProgressOnBar],
  );

  const onSearchClose = useCallback(() => {
    setShowSearch(false);
    setActiveCell(null);
  }, [setShowSearch]);

  const highlightRegions = useMemo<readonly Highlight[] | undefined>(() => {
    if (activeCell === null || !showSearch) return undefined;
    const [col, row] = activeCell;
    return [
      {
        range: { x: col, y: row, width: 1, height: 1 },
        color: activeMatchColor(isDark),
        style: "solid",
      },
    ];
  }, [activeCell, showSearch, isDark]);

  return { onSearchResultsChanged, onSearchClose, highlightRegions };
}
