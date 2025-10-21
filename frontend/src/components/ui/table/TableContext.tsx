import React, { createContext, useContext } from 'react';
import type { VariantProps } from 'class-variance-authority';
import { tableCellSizeVariants } from './table-variants';

type TableContextValue = VariantProps<typeof tableCellSizeVariants>;

const TableContext = createContext<TableContextValue>({
  size: 'Medium',
});

export const TableProvider: React.FC<{
  size?: VariantProps<typeof tableCellSizeVariants>['size'];
  children: React.ReactNode;
}> = ({ size, children }) => {
  return (
    <TableContext.Provider value={{ size }}>{children}</TableContext.Provider>
  );
};

export const useTableSize = () => {
  const context = useContext(TableContext);
  return context.size;
};
