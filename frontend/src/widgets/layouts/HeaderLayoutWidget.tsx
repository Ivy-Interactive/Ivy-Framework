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
      className="remove-ancestor-padding h-[calc(100vh-70px)]"
      style={styles}
    >
      <div className="flex flex-col h-full">
        <div className="p-2 border-b bg-background">{slots.Header}</div>
        <ScrollArea className="flex-grow">
          <div className="p-4">{slots.Content}</div>
        </ScrollArea>
      </div>
    </div>
  );
};
