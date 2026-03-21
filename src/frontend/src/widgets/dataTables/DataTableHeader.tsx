import React, { ReactNode } from 'react';
import { cn } from '@/lib/utils';

/**
 * Simple header component that wraps table options
 * Each option manages its own state independently
 */
export interface DataTableHeaderProps {
  children: ReactNode;
  className?: string;
}

export const DataTableHeader: React.FC<DataTableHeaderProps> = ({
  children,
  className,
}) => {
  return <div className={cn('w-full', className)}>{children}</div>;
};

/**
 * Legacy compatibility wrapper - will be removed after migration
 */
export const DataTableOptions: React.FC<{
  hasOptions: { allowFiltering: boolean; allowLlmFiltering: boolean };
}> = () => {
  // This component is temporarily kept for backward compatibility
  // It will be replaced by DataTableFilterOption in the next step
  return null;
};
