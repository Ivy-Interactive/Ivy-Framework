'use client';

import React from 'react';
import { Card } from '@/components/ui/card';

interface KanbanCardWidgetProps {
  id: string;
  title?: string;
  description?: string;
  assignee?: string;
  priority?: number;
  children?: React.ReactNode;
}

export const KanbanCardWidget: React.FC<KanbanCardWidgetProps> = ({
  title,
  description,
  assignee,
  priority,
  children,
}) => {
  const getPriorityColor = (priority: number) => {
    switch (priority) {
      case 1:
        return 'bg-red-100 text-red-700 border-red-200';
      case 2:
        return 'bg-yellow-100 text-yellow-700 border-yellow-200';
      case 3:
        return 'bg-green-100 text-green-700 border-green-200';
      default:
        return 'bg-gray-100 text-gray-700 border-gray-200';
    }
  };

  return (
    <Card className="h-40 p-4 hover:shadow-md transition-all duration-200 flex flex-col">
      {/* Header with title */}
      <div className="mb-3 flex-shrink-0">
        <h3 className="font-semibold text-sm text-gray-900 leading-tight line-clamp-2 overflow-hidden text-ellipsis">
          {title || 'Untitled Task'}
        </h3>
      </div>

      {/* Description - flexible content area */}
      <div className="flex-1 min-h-0 mb-3 overflow-hidden">
        {description && (
          <p className="text-xs text-gray-600 line-clamp-4 leading-relaxed overflow-hidden text-ellipsis break-words">
            {description}
          </p>
        )}
      </div>

      {/* Footer with metadata - always at bottom */}
      <div className="flex items-center justify-between gap-2 flex-shrink-0">
        <div className="flex items-center gap-2">
          {/* Priority badge */}
          <span
            className={`px-2 py-1 text-xs font-medium rounded-md border ${getPriorityColor(priority || 1)}`}
          >
            P{priority || 1}
          </span>
        </div>

        {/* Assignee avatar */}
        <div className="flex items-center gap-1">
          {assignee && assignee !== 'Unassigned' ? (
            <div className="h-6 w-6 rounded-full bg-blue-100 flex items-center justify-center">
              <span className="text-xs font-medium text-blue-700">
                {assignee.slice(0, 2).toUpperCase()}
              </span>
            </div>
          ) : (
            <div className="h-6 w-6 rounded-full bg-gray-100 flex items-center justify-center">
              <span className="text-xs text-gray-400 font-medium">?</span>
            </div>
          )}
        </div>
      </div>

      {children}
    </Card>
  );
};
