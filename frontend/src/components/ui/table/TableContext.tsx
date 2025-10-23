import React, { createContext } from 'react';
import type { VariantProps } from 'class-variance-authority';
import { tableCellSizeVariants } from './table-variants';
import { Sizes } from '@/types/sizes';

type TableContextValue = VariantProps<typeof tableCellSizeVariants>;

export const TableContext = createContext<TableContextValue>({
  size: Sizes.Medium,
});

export const TableProvider: React.FC<{
  size?: Sizes;
  children: React.ReactNode;
}> = ({ size, children }) => {
  return (
    <TableContext.Provider value={{ size }}>{children}</TableContext.Provider>
  );
};
