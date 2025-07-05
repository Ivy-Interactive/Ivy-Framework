import { useEventHandler } from "@/components/EventHandlerContext";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import { getWidth } from "@/lib/styles";
import React from "react";

interface DialogWidgetProps {
  id: string;
  children?: React.ReactNode;
  width?: string;
}

export const DialogWidget: React.FC<DialogWidgetProps> = ({
  id,
  children,
  width,
}) => {
  const eventHandler = useEventHandler();
  const styles = {
    ...getWidth(width),
  };
  return (
    <Dialog open={true} onOpenChange={(_) => eventHandler("OnClose", id, [])}>
      <DialogContent style={styles}>{children}</DialogContent>
    </Dialog>
  );
};
