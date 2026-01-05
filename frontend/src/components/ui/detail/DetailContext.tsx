import React, { createContext } from 'react';
import type { VariantProps } from 'class-variance-authority';
import { detailValueSizeVariants } from './detail-variants';
import { Scales } from '@/types/scale';

type DetailContextValue = VariantProps<typeof detailValueSizeVariants>;

// eslint-disable-next-line react-refresh/only-export-components
export const DetailContext = createContext<DetailContextValue>({
  scale: Scales.Medium,
});

export const DetailProvider: React.FC<{
  scale?: Scales;
  children: React.ReactNode;
}> = ({ scale = Scales.Medium, children }) => {
  return (
    <DetailContext.Provider value={{ scale }}>
      {children}
    </DetailContext.Provider>
  );
};
