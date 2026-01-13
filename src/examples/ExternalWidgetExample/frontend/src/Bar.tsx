import React from 'react';

interface BarProps {
  id: string;
  label?: string;
  color?: string;
}

export const Bar: React.FC<BarProps> = ({
  label = 'Bar',
  color = '#10b981',
}) => {
  return (
    <div
      className="px-3 py-2 rounded text-white font-medium text-sm"
      style={{ backgroundColor: color }}
    >
      {label}
    </div>
  );
};
