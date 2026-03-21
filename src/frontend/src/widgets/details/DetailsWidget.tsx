import React from 'react';
import { Details } from '@/components/ui/detail';
import { Densities } from '@/types/density';

type DetailsWidgetProps = {
  id: string;
  children: React.ReactNode;
  density?: Densities;
};

export const DetailsWidget = ({
  id,
  children,
  density,
}: DetailsWidgetProps) => {
  return (
    <Details density={density} key={id}>
      {children}
    </Details>
  );
};
