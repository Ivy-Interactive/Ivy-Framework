import React, { createContext } from 'react';
import type { VariantProps } from 'class-variance-authority';
import { tableCellSizeVariant } from './table-variant';
import { Scales } from '@/types/scale';

type TableContextValue = VariantProps<typeof tableCellSizeVariant>;

// eslint-disable-next-line react-refresh/only-export-components
export const TableContext = createContext<TableContextValue>({
  scale: Scales.Medium,
});

export const TableProvider: React.FC<{
  scale?: Scales;
  children: React.ReactNode;
}> = ({ scale = Scales.Medium, children }) => {
  return (
    <TableContext.Provider value={{ scale }}>{children}</TableContext.Provider>
  );
};
