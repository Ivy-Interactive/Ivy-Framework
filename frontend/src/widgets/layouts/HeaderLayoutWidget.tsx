import React from 'react';
import { ScrollArea } from '@/components/ui/scroll-area';
import { getWidth } from '@/lib/styles';
import { cn } from '@/lib/utils';

interface HeaderLayoutWidgetProps {
  width?: string;
  showHeaderDivider?: boolean;
  contentScroll?: 'None' | 'Auto';
  slots?: {
    Header?: React.ReactNode[];
    Content?: React.ReactNode[];
  };
}

export const HeaderLayoutWidget: React.FC<HeaderLayoutWidgetProps> = ({
  slots,
  width,
  showHeaderDivider = true,
  contentScroll = 'Auto',
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

  // When Scroll.None is set, contentScroll will be 'None', otherwise 'Auto' or undefined
  const shouldScroll = contentScroll !== 'None';

  return (
    <div
      className="remove-parent-padding flex flex-col w-full h-full"
      style={styles}
    >
      <div
        className={cn(
          'flex-none p-2 bg-background w-full',
          showHeaderDivider && 'border-b'
        )}
      >
        {slots.Header}
      </div>
      <div className="flex-1 min-h-0 w-full overflow-hidden">
        {shouldScroll ? (
          <ScrollArea className="h-full w-full">
            <div className="p-4 w-full">{slots.Content}</div>
          </ScrollArea>
        ) : (
          <div className="h-full w-full">{slots.Content}</div>
        )}
      </div>
    </div>
  );
};
