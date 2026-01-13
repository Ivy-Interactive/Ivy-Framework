import React from 'react';

interface FragmentWidgetProps {
  id: string;
  children?: React.ReactNode;
}

export const FragmentWidget: React.FC<FragmentWidgetProps> = ({ children }) => (
  <>{children}</>
);
