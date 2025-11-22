import React, { ReactNode } from 'react';
import { cn } from '@/lib/utils';
import { tableStyles } from './styles/style';

/**
 * Footer component that overlaps the bottom of the DataTableEditor
 * Horizontal scrollbars from the editor will appear on top of this footer
 */
export interface DataTableFooterProps {
  children?: ReactNode;
  className?: string;
}

export const DataTableFooter: React.FC<DataTableFooterProps> = ({
  children,
  className,
}) => {
  return (
    <div className={cn(className)} style={tableStyles.tableEditor.footer}>
      {children}
    </div>
  );
};
