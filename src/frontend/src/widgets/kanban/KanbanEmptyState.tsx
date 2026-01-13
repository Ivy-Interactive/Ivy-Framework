import React from 'react';

export const KanbanEmptyState: React.FC = () => {
  return (
    <div className="flex items-center justify-center p-8 text-gray-500">
      <div className="text-center">
        <p className="text-lg font-medium">No kanban data available</p>
        <p className="text-sm">
          The backend did not provide any kanban data to display.
        </p>
      </div>
    </div>
  );
};
