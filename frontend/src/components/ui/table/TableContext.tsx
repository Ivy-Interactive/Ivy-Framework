import React, { createContext } from 'react';
import type { VariantProps } from 'class-variance-authority';
import { tableCellSizeVariants } from './table-variants';
import { Scales } from '@/types/scale';

type TableContextValue = VariantProps<typeof tableCellSizeVariants>;

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
