import { useCallback, useMemo, useRef, useState } from "react";
import { GridCellKind, GridMouseEventArgs } from "@glideapps/glide-data-grid";
import { GridCell, Item } from "@glideapps/glide-data-grid";
import { useDebounce } from "../../../hooks/use-debounce";

interface UseLinkCellHoverProps {
  getCellContent: (cell: Item) => GridCell;
  visibleRows: number;
}

export const useLinkCellHover = ({ getCellContent, visibleRows }: UseLinkCellHoverProps) => {
  const [linkTooltipPos, setLinkTooltipPos] = useState<{ x: number; y: number } | null>(null);
  const posRef = useRef(linkTooltipPos);
  posRef.current = linkTooltipPos;

  const setPos = useCallback(
    (...args: unknown[]) => setLinkTooltipPos(args[0] as { x: number; y: number } | null),
    [],
  );
  const debouncedSetPos = useDebounce(setPos, 50);

  const onItemHovered = useCallback(
    (args: GridMouseEventArgs) => {
      if (args.kind !== "cell") {
        debouncedSetPos(null);
        return;
      }
      const [, row] = args.location;
      if (row >= visibleRows) {
        debouncedSetPos(null);
        return;
      }
      const cell = getCellContent(args.location);
      const cellData =
        cell.kind === GridCellKind.Custom
          ? (cell.data as { kind?: string; url?: string })
          : undefined;
      const isLinkCell = cellData?.kind === "link-cell" && !!cellData?.url;

      if (isLinkCell) {
        debouncedSetPos({ x: args.bounds.x + args.bounds.width / 2, y: args.bounds.y });
      } else {
        debouncedSetPos(null);
      }
    },
    [getCellContent, visibleRows, debouncedSetPos],
  );

  const virtualRef = useMemo(
    () => ({
      getBoundingClientRect: () => {
        const pos = posRef.current;
        const x = pos?.x ?? 0;
        const y = pos?.y ?? 0;
        return new DOMRect(x, y, 0, 0);
      },
    }),
    [],
  );

  return { isLinkHovered: linkTooltipPos !== null, virtualRef, onItemHovered, linkTooltipPos };
};
