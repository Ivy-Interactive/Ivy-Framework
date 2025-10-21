'use client';

import React from 'react';

interface KanbanColumnWidgetProps {
  id: string;
  title?: string;
  color?: string;
  children?: React.ReactNode;
}

export const KanbanColumnWidget: React.FC<KanbanColumnWidgetProps> = ({
  title,
  color,
  children,
}) => {
  return (
    <div className="flex flex-col w-80 min-w-80 bg-gray-50 rounded-lg p-4 min-h-[600px]">
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center gap-2 min-w-0 flex-1">
          {color && (
            <div
              className="h-3 w-3 rounded-full flex-shrink-0"
              style={{ backgroundColor: color }}
            />
          )}
          <span className="font-semibold text-gray-800 truncate min-w-0">
            {title || 'Column'}
          </span>
        </div>
      </div>

      <div className="flex flex-col gap-3 flex-1">{children}</div>
    </div>
  );
};
