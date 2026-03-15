import { Separator } from '@/components/ui/separator';
import { getHeight, getWidth } from '@/lib/styles';
import { cn } from '@/lib/utils';
import React from 'react';

interface SeparatorWidgetProps {
  id: string;
  orientation: 'Vertical' | 'Horizontal';
  text?: string;
  textAlign?: 'Left' | 'Center' | 'Right' | 'Justify';
  height?: string;
  width?: string;
}

export const SeparatorWidget: React.FC<SeparatorWidgetProps> = ({
  orientation = 'Horizontal',
  text,
  textAlign = 'Center',
  width,
  height,
}) => {
  const styles =
    orientation === 'Vertical' ? getWidth(width) : getHeight(height);

  const separator = (
    <Separator
      orientation={orientation === 'Vertical' ? 'vertical' : 'horizontal'}
      className={cn(orientation === 'Vertical' && 'h-full')}
    />
  );

  if (text) {
    const textAlignClass = {
      Left: 'left-4',
      Center: 'left-1/2 -translate-x-1/2',
      Right: 'right-4',
      Justify: 'left-1/2 -translate-x-1/2',
    }[textAlign];

    return (
      <div className={cn('relative flex items-center')}>
        {separator}
        <span
          className={cn(
            'absolute px-2 text-small-label text-muted-foreground bg-background',
            textAlignClass
          )}
        >
          {text}
        </span>
      </div>
    );
  }

  return (
    <div style={styles} className={cn('flex items-center justify-center')}>
      {separator}
    </div>
  );
};
