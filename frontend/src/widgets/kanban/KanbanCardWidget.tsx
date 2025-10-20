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
  return (
    <Card className="p-3">
      <div className="flex items-start justify-between gap-2">
        <div className="flex flex-col gap-1 flex-1">
          <p className="m-0 flex-1 font-medium text-sm">
            {title || 'Untitled Task'}
          </p>
          {description && (
            <p className="m-0 text-gray-500 text-xs">{description}</p>
          )}
        </div>
        {assignee && (
          <div className="h-6 w-6 shrink-0 rounded-full bg-gray-200 flex items-center justify-center">
            <span className="text-xs font-medium">
              {assignee.slice(0, 2).toUpperCase()}
            </span>
          </div>
        )}
      </div>

      <div className="mt-2 flex items-center justify-between text-xs text-gray-500">
        <span className="px-2 py-1 bg-gray-100 rounded">
          Priority {priority || 1}
        </span>
        <span>{assignee || 'Unassigned'}</span>
      </div>

      {children}
    </Card>
  );
};
