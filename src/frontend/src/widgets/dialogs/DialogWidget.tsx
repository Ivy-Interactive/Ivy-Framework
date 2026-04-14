import { Dialog, DialogContent } from "@/components/ui/dialog";
import React from "react";
import { useEventHandler } from "@/components/event-handler";
import { getWidth } from "@/lib/styles";
import { cn } from "@/lib/utils";

interface DialogWidgetProps {
  id: string;
  children?: React.ReactNode;
  width?: string;
  events?: string[];
}

export const DialogWidget: React.FC<DialogWidgetProps> = ({ id, children, width, events = [] }) => {
  const eventHandler = useEventHandler();
  const isVisible = true;

  const widthStyles = getWidth(width);
  const styles = {
    ...widthStyles,
    ...(width && widthStyles.width && !widthStyles.maxWidth ? { maxWidth: widthStyles.width } : {}),
  };

  return (
    <Dialog
      open={true}
      onOpenChange={() => {
        if (events.includes("OnClose")) eventHandler("OnClose", id, []);
      }}
    >
      <DialogContent
        style={styles}
        className={cn(isVisible && "alert-animate-enter")}
        aria-describedby={undefined}
        onOpenAutoFocus={(e) => {
          const container = e.currentTarget as HTMLElement | null;
          const target = container?.querySelector<HTMLElement>("[autofocus]");
          if (target) {
            e.preventDefault();
            target.focus();
          } else {
            e.preventDefault();
          }
        }}
        onCloseAutoFocus={(e) => e.preventDefault()}
      >
        {children}
      </DialogContent>
    </Dialog>
  );
};
