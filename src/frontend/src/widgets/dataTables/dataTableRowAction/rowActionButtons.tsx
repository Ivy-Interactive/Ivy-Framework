import React from "react";
import { MenuItem } from "@/types/widgets";
import { ActionRenderer } from "./actionRenderer";
import { getActionId } from "./utils";

interface RowActionButtonsProps {
  /**
   * Array of action configurations
   */
  actions: MenuItem[];
  /**
   * Y position of the overlay container (row top)
   */
  top: number;
  /**
   * Height of the overlay container (should match the grid row bounds)
   */
  height: number;
  /**
   * Whether buttons are visible
   */
  visible: boolean;
  /**
   * Click handler for action buttons
   */
  onActionClick: (action: MenuItem) => void;
  /**
   * Mouse enter handler to prevent losing hover state
   */
  onMouseEnter?: () => void;
  /**
   * Mouse leave handler
   */
  onMouseLeave?: () => void;
}

/**
 * Row action buttons that appear on hover at the right edge of the data table
 */
export const RowActionButtons: React.FC<RowActionButtonsProps> = ({
  actions,
  top,
  height,
  visible,
  onActionClick,
  onMouseEnter,
  onMouseLeave,
}) => {
  if (!visible || actions.length === 0) return null;

  return (
    <div
      className="absolute z-50 flex flex-row gap-1 items-center"
      style={{
        top: `${top}px`,
        height: `${height}px`,
        right: "8px",
        opacity: visible ? 1 : 0,
        pointerEvents: visible ? "auto" : "none",
      }}
      onMouseEnter={onMouseEnter}
      onMouseLeave={onMouseLeave}
    >
      {actions.map((action) => (
        <ActionRenderer key={getActionId(action)} action={action} onActionClick={onActionClick} />
      ))}
    </div>
  );
};
