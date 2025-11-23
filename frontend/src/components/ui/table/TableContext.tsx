import React, { createContext } from 'react';
import type { VariantProps } from 'class-variance-authority';
import { tableCellSizeVariants } from './table-variants';
import { Scale } from '@/types/scale';

type TableContextValue = VariantProps<typeof tableCellSizeVariants>;

export const TableContext = createContext<TableContextValue>({
  scale: Scale.Medium,
});

export const TableProvider: React.FC<{
  scale?: Scale;
  children: React.ReactNode;
}> = ({ scale, children }) => {
  return (
    <TableContext.Provider value={{ scale }}>{children}</TableContext.Provider>
  );
};
