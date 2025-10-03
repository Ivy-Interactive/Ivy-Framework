import React from 'react';

interface FieldWidgetProps {
  id: string;
  label: string;
  description?: string;
  required: boolean;
  children?: React.ReactNode;
}

export const FieldWidget: React.FC<FieldWidgetProps> = ({
  label,
  description,
  required,
  children,
}) => (
  <div className="flex flex-col gap-2 flex-1 min-w-0">
    {label && (
      <label className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70">
        {label} {required && <span className="font-mono text-primary">*</span>}
      </label>
    )}
    {children}
    {description && (
      <p className="text-sm text-muted-foreground">{description}</p>
    )}
  </div>
);
