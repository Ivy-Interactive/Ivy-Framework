'use client';

import React from 'react';

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
  children?: React.ReactNode;
}

export const KanbanCardWidget: React.FC<KanbanCardWidgetProps> = ({
  children,
}) => {
  // KanbanCardWidget just wraps the Card widget content from backend
  // Render children (Card widget) as-is
  return <>{children}</>;
};
