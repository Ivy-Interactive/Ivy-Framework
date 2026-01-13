import React from 'react';
import { DetailItem } from '@/components/ui/detail';

interface DetailWidgetProps {
  id: string;
  label: string;
  multiLine?: boolean;
  children?: React.ReactNode[];
}

export const DetailWidget: React.FC<DetailWidgetProps> = ({
  id,
  label,
  children,
  multiLine,
}) => {
  return (
    <DetailItem label={label} multiLine={multiLine} key={id}>
      {children}
    </DetailItem>
  );
};
