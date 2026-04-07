import { useCallback, useState } from "react";
import { GridCellKind, GridMouseEventArgs } from "@glideapps/glide-data-grid";
import { GridCell, Item } from "@glideapps/glide-data-grid";

interface UseLinkCellHoverProps {
  getCellContent: (cell: Item) => GridCell;
  visibleRows: number;
}

export const useLinkCellHover = ({ getCellContent, visibleRows }: UseLinkCellHoverProps) => {
  const [linkTooltipPos, setLinkTooltipPos] = useState<{ x: number; y: number } | null>(null);

  const onItemHovered = useCallback(
    (args: GridMouseEventArgs) => {
      if (args.kind !== "cell") {
        setLinkTooltipPos(null);
        return;
      }
      const [, row] = args.location;
      if (row >= visibleRows) {
        setLinkTooltipPos(null);
        return;
      }
      const cell = getCellContent(args.location);
      const isLinkCell =
        cell.kind === GridCellKind.Custom && (cell.data as { kind?: string })?.kind === "link-cell";

      if (isLinkCell) {
        setLinkTooltipPos({ x: args.bounds.x + args.bounds.width / 2, y: args.bounds.y });
      } else {
        setLinkTooltipPos(null);
      }
    },
    [getCellContent, visibleRows],
  );

  return { linkTooltipPos, onItemHovered };
};
