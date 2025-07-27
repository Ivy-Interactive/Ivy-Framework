import { ReactNode } from 'react';
import { X, RotateCw } from 'lucide-react';
import { useEventHandler } from '@/components/EventHandlerContext';
import { ScrollArea } from '@/components/ui/scroll-area';
import { getWidth } from '@/lib/styles';

interface BladeWidgetProps {
  id: string;
  title: string;
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

  return (
    <div
      style={styles}
      className="flex flex-col bg-background border-l border-border h-full flex-1"
    >
      <div
        className="flex items-center justify-between px-4 bg-background text-foreground h-[70px] border-b border-border"
        onMouseDown={e => handleMouseDown(e)}
      >
        <div className="h-[70px] flex items-center justify-between">
          {!slots?.BladeHeader && (
            <h2 className="text-sm font-medium">{title}</h2>
          )}
          <div>{slots?.BladeHeader}</div>
          <div className="flex items-center gap-1">
            <button
              onClick={() => eventHandler('OnRefresh', id, [])}
              className="hover:bg-accent rounded-sm transition-colors h-9 w-9 inline-flex items-center justify-center cursor-pointer"
            >
              <RotateCw className="h-4 w-4" />
            </button>
            {index > 0 && (
              <button
                onClick={() => eventHandler('OnClose', id, [])}
                className="hover:bg-accent rounded-sm transition-colors h-9 w-9 inline-flex items-center justify-center cursor-pointer"
              >
                <X className="h-4 w-4" />
              </button>
            )}
          </div>
        </div>
      </div>
      <div className="bg-muted h-full">
        <ScrollArea type="hover" className="blade-container h-full">
          <div className="p-4">{children}</div>
        </ScrollArea>
      </div>
    </div>
  );
}
