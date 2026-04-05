import { useCallback, useMemo, useState } from "react";
import {
  CompactSelection,
  DataEditorRef,
  GridSelection,
  Highlight,
  Item,
} from "@glideapps/glide-data-grid";

const activeMatchColor = (isDark: boolean) => (isDark ? "#b45309" : "#fde047");

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

export function useSearchNavigation(
  gridRef: React.RefObject<DataEditorRef | null>,
  setGridSelection: (selection: GridSelection) => void,
  isDark: boolean,
  showSearch: boolean,
  setShowSearch: React.Dispatch<React.SetStateAction<boolean>>,
) {
  const [activeCell, setActiveCell] = useState<[number, number] | null>(null);

  const onSearchResultsChanged = useCallback(
    (results: readonly Item[], navIndex: number) => {
      if (results.length === 0) {
        setActiveCell(null);
        return;
      }
      if (navIndex < 0 || navIndex >= results.length) return;
      const [dataCol, row] = results[navIndex];
      setActiveCell([dataCol, row]);
      setGridSelection(singleCellGridSelection(dataCol, row));
      gridRef.current?.scrollTo(dataCol, row, "both", 0, 0);
    },
    [setGridSelection],
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
