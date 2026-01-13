import React from 'react';

interface FooProps {
  id: string;
  title?: string;
  children?: React.ReactNode;
}

export const Foo: React.FC<FooProps> = ({
  title = 'Foo Container',
  children,
}) => {
  return (
    <div className="rounded-lg border bg-card p-4 shadow-sm">
      <h3 className="mb-3 text-lg font-semibold text-foreground">{title}</h3>
      <div className="flex flex-wrap gap-2">
        {children}
      </div>
    </div>
  );
};
