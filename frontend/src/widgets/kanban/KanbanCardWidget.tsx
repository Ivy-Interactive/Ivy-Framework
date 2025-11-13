import React from 'react';

interface KanbanCardWidgetProps {
  id: string;
  cardId?: string;
  priority?: number;
  slots?: {
    default?: React.ReactNode[];
  };
}

export const KanbanCardWidget: React.FC<KanbanCardWidgetProps> = ({
  slots,
}) => {
  // Render children so they're available to parent KanbanWidget
  // Props are accessible via the widget node in WidgetRenderer
  return <>{slots?.default}</>;
};
