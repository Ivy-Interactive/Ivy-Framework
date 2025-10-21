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
}) => {
  return (
    <div className="field flex flex-col flex-1 min-w-0">
      {label && (
        <label className="font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70">
          {label}{' '}
          {required && <span className="font-mono text-primary">*</span>}
        </label>
      )}
      {children}
      {description && <p className="text-muted-foreground">{description}</p>}
    </div>
  );
};
