import React from 'react';
import { ScrollArea } from '@/components/ui/scroll-area';

interface FooterLayoutWidgetProps {
  slots?: {
    Footer?: React.ReactNode[];
    Content?: React.ReactNode[];
  };
}

export const FooterLayoutWidget: React.FC<FooterLayoutWidgetProps> = ({
  slots,
}) => {
  if (!slots?.Footer || !slots?.Content) {
    return (
      <div className="text-red-500">
        Error: FooterLayout requires both Footer and Content slots.
      </div>
    );
  }

  return (
    <div className="-ml-4 -mr-4 h-full flex flex-col overflow-hidden">
      <div className="flex-1 min-h-0 relative">
        <div className="absolute inset-0">
          <ScrollArea className="h-full">
            <div className="p-4">{slots.Content}</div>
          </ScrollArea>
        </div>
      </div>
      <div className="flex-none p-4 border-t w-full bg-white">
        {slots.Footer}
      </div>
    </div>
  );
};
