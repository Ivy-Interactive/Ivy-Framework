import React, { createContext } from 'react';
import type { VariantProps } from 'class-variance-authority';
import { detailValueSizeVariant } from './detail-variant';
import { Densities } from '@/types/density';

type DetailContextValue = VariantProps<typeof detailValueSizeVariant>;

// eslint-disable-next-line react-refresh/only-export-components
export const DetailContext = createContext<DetailContextValue>({
  density: Densities.Medium,
});

export const DetailProvider: React.FC<{
  density?: Densities;
  children: React.ReactNode;
}> = ({ density = Densities.Medium, children }) => {
  return (
    <DetailContext.Provider value={{ density }}>
      {children}
    </DetailContext.Provider>
  );
};
