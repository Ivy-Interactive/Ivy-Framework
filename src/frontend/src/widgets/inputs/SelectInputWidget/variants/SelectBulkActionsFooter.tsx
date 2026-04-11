import React from "react";
import { cn } from "@/lib/utils";
import { Densities } from "@/types/density";

const selectTextVariant = {
  Small: "text-xs",
  Medium: "text-sm",
  Large: "text-base",
};

interface SelectBulkActionsFooterProps {
  density?: Densities;
  onSelectAll: () => void;
  onClearAll: () => void;
  selectAllDisabled: boolean;
  clearAllDisabled: boolean;
}

export const SelectBulkActionsFooter: React.FC<SelectBulkActionsFooterProps> = ({
  density = Densities.Medium,
  onSelectAll,
  onClearAll,
  selectAllDisabled,
  clearAllDisabled,
}) => {
  const textSize = selectTextVariant[density];
  return (
    <div
      className={cn(
        "border-t border-border mt-2 pt-2 flex justify-between items-center gap-2 shrink-0",
        textSize,
      )}
      role="group"
      aria-label="Bulk selection"
    >
      <button
        type="button"
        className={cn(
          "text-primary hover:underline underline-offset-2 disabled:opacity-50 disabled:cursor-not-allowed disabled:no-underline focus:outline-none focus-visible:ring-2 focus-visible:ring-ring rounded-sm px-0.5",
          textSize,
        )}
        disabled={selectAllDisabled}
        onClick={onSelectAll}
      >
        Select All
      </button>
      <button
        type="button"
        className={cn(
          "text-primary hover:underline underline-offset-2 disabled:opacity-50 disabled:cursor-not-allowed disabled:no-underline focus:outline-none focus-visible:ring-2 focus-visible:ring-ring rounded-sm px-0.5",
          textSize,
        )}
        disabled={clearAllDisabled}
        onClick={onClearAll}
      >
        Clear All
      </button>
    </div>
  );
};
