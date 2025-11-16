import { ReactNode } from 'react';
import { X, RotateCw } from 'lucide-react';
import { useEventHandler } from '@/components/event-handler';
import { ScrollArea } from '@/components/ui/scroll-area';
import { getWidth } from '@/lib/styles';
import { buttonVariants } from '@/components/ui/button/variants';

interface BladeWidgetProps {
  id: string;
  title?: string;
  width?: string;
  index: number;
  children: ReactNode;
  slots: {
    BladeHeader?: React.ReactNode;
  };
}

export function BladeWidget({
  index,
  title,
  children,
  id,
  width,
  slots,
}: BladeWidgetProps) {
  const handleMouseDown = (e: React.MouseEvent<HTMLDivElement, MouseEvent>) => {
    if (e.button === 1) {
      e.preventDefault();
      eventHandler('OnClose', id, []);
    }
  };

  const eventHandler = useEventHandler();

  const styles = {
    ...getWidth(width),
  };

  // Only apply flex-1 when no explicit width is provided
  const flexClass = width ? '' : 'flex-1';

  return (
    <div
      style={styles}
      className={`flex flex-col bg-background border-r border-border h-full ${flexClass}`}
    >
      <div
        className="flex items-center justify-between px-4 bg-background text-foreground h-[70px] border-b border-border"
        onMouseDown={e => handleMouseDown(e)}
      >
        <div className="flex items-center h-[70px]">
          {!slots?.BladeHeader && title && (
            <h2 className="text-body">{title}</h2>
          )}
          <div>{slots?.BladeHeader}</div>
        </div>
        <div className="flex items-center h-[70px]">
          <button
            onClick={() => eventHandler('OnRefresh', id, [])}
            className={buttonVariants({ variant: 'ghost', size: 'icon' })}
          >
            <RotateCw className="h-4 w-4" />
          </button>
          {index > 0 && (
            <button
              onClick={() => eventHandler('OnClose', id, [])}
              className={buttonVariants({ variant: 'ghost', size: 'icon' })}
            >
              <X className="h-4 w-4" />
            </button>
          )}
        </div>
      </div>
      <div className="bg-background flex-1 min-h-0">
        {/* radix scrollarea breaks the nested containers widths*/}
        <ScrollArea
          type="hover"
          className="blade-container h-full [&>div>div[style]]:block!"
        >
          <div className="p-4">{children}</div>
        </ScrollArea>
      </div>
    </div>
  );
}
