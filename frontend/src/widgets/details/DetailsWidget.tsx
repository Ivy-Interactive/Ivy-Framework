import React from 'react';

type DetailsWidgetProps = {
  id: string;
  children: React.ReactNode;
};

export const DetailsWidget = ({ id, children }: DetailsWidgetProps) => {
  return (
    <div className="w-full [&>:last-child]:border-0" key={id}>
      {children}
    </div>
  );
};
