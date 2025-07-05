import { useEventHandler } from "@/components/EventHandlerContext";
import { ScrollArea } from "@/components/ui/scroll-area";
import { getWidth } from "@/lib/styles";
import { cn } from "@/lib/utils";
import { X, RotateCw } from "lucide-react";
import { ReactNode } from "react";

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
      eventHandler("OnClose", id, []);
    }
  };

  const eventHandler = useEventHandler();

  const styles = {
    ...getWidth(width),
  };

  return (
    <div
      style={styles}
      className={cn("flex flex-col h-screen bg-white border-l border-gray-200")}
    >
      <div
        className="flex items-center justify-between px-4 bg-white text-black h-[70px] border-b"
        onMouseDown={(e) => handleMouseDown(e)}
      >
        {!slots?.BladeHeader && (
          <h2 className="text-sm font-medium">{title}</h2>
        )}
        <div>{slots?.BladeHeader}</div>
        <div className="flex items-center gap-1">
          <button
            onClick={() => eventHandler("OnRefresh", id, [])}
            className="hover:bg-accent rounded-sm transition-colors h-9 w-9 inline-flex items-center justify-center"
          >
            <RotateCw className="h-4 w-4" />
          </button>
          {index > 0 && (
            <button
              onClick={() => eventHandler("OnClose", id, [])}
              className="hover:bg-accent rounded-sm transition-colors h-9 w-9 inline-flex items-center justify-center"
            >
              <X className="h-4 w-4" />
            </button>
          )}
        </div>
      </div>
      <div className="bg-gray-50">
        <ScrollArea
          type="hover"
          className="blade-container h-[calc(100vh-70px)]"
        >
          <div className="p-4">{children}</div>
        </ScrollArea>
      </div>
    </div>
  );
}
