import { Skeleton } from '@/components/ui/skeleton';
import { getHeight, getWidth } from '@/lib/styles';
import React from 'react';

interface SkeletonWidgetProps {
  id: string;
  width: string;
  height: string;
}

export const SkeletonWidget: React.FC<SkeletonWidgetProps> = ({
  width = 'Full',
  height = 'Full',
}) => {
  const styles = {
    ...getWidth(width),
    ...getHeight(height),
  };

  return <Skeleton style={styles} className="rounded-xl bg-muted" />;
};
