import React from 'react';
import { ScrollArea } from '@/components/ui/scroll-area';
import { getWidth } from '@/lib/styles';

interface HeaderLayoutWidgetProps {
  width?: string;
  slots?: {
    Header?: React.ReactNode[];
    Content?: React.ReactNode[];
  };
}

export const HeaderLayoutWidget: React.FC<HeaderLayoutWidgetProps> = ({
  slots,
  width,
}) => {
  if (!slots?.Header || !slots?.Content) {
    return (
      <div className="text-red-500">
        Error: HeaderLayout requires both Header and Content slots.
      </div>
    );
  }

  const styles: React.CSSProperties = {
    ...getWidth(width),
  };

  return (
    <div
      className="remove-ancestor-padding flex flex-col w-full h-full"
      style={styles}
    >
      <div className="flex-none p-2 border-b bg-background w-full">
        {slots.Header}
      </div>
      <div className="flex-1 min-h-0 w-full overflow-hidden">
        <ScrollArea className="h-full w-full">
          <div className="p-4 w-full">{slots.Content}</div>
        </ScrollArea>
      </div>
    </div>
  );
};
