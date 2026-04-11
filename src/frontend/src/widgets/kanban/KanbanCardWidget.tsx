"use client";

import React from "react";
import { Densities } from "@/types/density";

interface KanbanCardWidgetProps {
  id: string;
  cardId?: string; // CardId prop from backend KanbanCard widget
  status?: string; // Status prop from backend KanbanCard widget (column/status)
  title?: string;
  description?: string;
  assignee?: string;
  priority?: number;
  width?: string;
  height?: string;
  density?: Densities;
  children?: React.ReactNode;
}

export const KanbanCardWidget: React.FC<KanbanCardWidgetProps> = ({
  children,
  density: _density = Densities.Medium,
}) => {
  // KanbanCardWidget just wraps the Card widget content from backend
  // Render children (Card widget) as-is
  return <>{children}</>;
};
