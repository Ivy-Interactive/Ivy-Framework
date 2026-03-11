import React, { createContext } from 'react';
import type { VariantProps } from 'class-variance-authority';
import { tableCellSizeVariant } from './table-variant';
import { Densities } from '@/types/density';

type TableContextValue = VariantProps<typeof tableCellSizeVariant>;

// eslint-disable-next-line react-refresh/only-export-components
export const TableContext = createContext<TableContextValue>({
  density: Densities.Medium,
});

export const TableProvider: React.FC<{
  density?: Densities;
  children: React.ReactNode;
}> = ({ density = Densities.Medium, children }) => {
  return (
    <TableContext.Provider value={{ density }}>
      {children}
    </TableContext.Provider>
  );
};
