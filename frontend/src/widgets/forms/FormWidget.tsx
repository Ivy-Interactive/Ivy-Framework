import React from 'react';

interface FormWidgetProps {
  id: string;
  children?: React.ReactNode;
}

export const FormWidget: React.FC<FormWidgetProps> = ({ children }) => (
  <div>{children}</div>
);
