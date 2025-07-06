import { Dialog, DialogContent } from '@/components/ui/dialog';
import React from 'react';
import { useEventHandler } from '@/components/EventHandlerContext';
import { getWidth } from '@/lib/styles';

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
    <Dialog open={true} onOpenChange={() => eventHandler('OnClose', id, [])}>
      <DialogContent style={styles}>{children}</DialogContent>
    </Dialog>
  );
};
