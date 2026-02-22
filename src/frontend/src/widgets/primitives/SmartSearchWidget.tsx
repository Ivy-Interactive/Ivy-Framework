import React from 'react';

interface SmartSearchWidgetProps {
  id: string;
  children?: React.ReactNode;
}

/**
 * Container for the smart search UI (search bar + answer sheet).
 * Provides layout styling for the docs app.
 */
export const SmartSearchWidget: React.FC<SmartSearchWidgetProps> = ({
  children,
}) => {
  return (
    <div className="w-full h-full p-4 overflow-y-auto">
      <div className="flex flex-col gap-2 max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 relative mt-8">
        {children}
      </div>
    </div>
  );
};
