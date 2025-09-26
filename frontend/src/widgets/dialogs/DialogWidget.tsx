import { Dialog, DialogContent } from '@/components/ui/dialog';
import React, { useEffect, useState } from 'react';
import { useEventHandler } from '@/components/event-handler';
import { getWidth } from '@/lib/styles';
import { cn } from '@/lib/utils';

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
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    setIsVisible(true);
  }, []);

  const styles = {
    ...getWidth(width),
  };

  return (
    <Dialog open={true} onOpenChange={() => eventHandler('OnClose', id, [])}>
      <DialogContent
        style={styles}
        className={cn(isVisible && 'alert-animate-enter')}
      >
        {children}
      </DialogContent>
    </Dialog>
  );
};
