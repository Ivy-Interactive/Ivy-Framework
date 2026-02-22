import { getHeight, getWidth } from '@/lib/styles';
import React from 'react';

interface SmartSearchWidgetProps {
  id: string;
  children?: React.ReactNode;
  width?: string;
  height?: string;
}

export const SmartSearchWidget: React.FC<SmartSearchWidgetProps> = ({
  children,
  width = 'Full',
  height = 'Full',
}) => {
  const styles = {
    ...getWidth(width),
    ...getHeight(height),
  };

  return (
    <div style={styles} className="overflow-y-auto pt-4">
      <div className="relative mx-auto flex max-w-6xl flex-col gap-2 px-4 sm:px-6 lg:px-8">
        {children}
      </div>
    </div>
  );
};
