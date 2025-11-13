import React from 'react';

interface KanbanColumnWidgetProps {
  id: string;
  title?: string;
  columnKey?: string;
  width?: string;
  height?: string;
  slots?: {
    default?: React.ReactNode[];
  };
}

export const KanbanColumnWidget: React.FC<KanbanColumnWidgetProps> = ({
  slots,
}) => {
  // Render children so they're available to parent KanbanWidget
  // Props are accessible via the widget node in WidgetRenderer
  return <>{slots?.default}</>;
};
