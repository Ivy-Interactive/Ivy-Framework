import React from 'react';
import { Details } from '@/components/ui/detail';
import { Scales } from '@/types/scale';

type DetailsWidgetProps = {
  id: string;
  children: React.ReactNode;
  scale?: Scales;
};

export const DetailsWidget = ({
  id,
  children,
  scale = Scales.Medium,
}: DetailsWidgetProps) => {
  return (
    <Details scale={scale} key={id}>
      {children}
    </Details>
  );
};
