import { useContext } from 'react';
import { WidgetEventHandlerType } from '@/types/widgets';
import { EventHandlerContext } from './context';

export const useEventHandler = (): WidgetEventHandlerType => {
  const context = useContext(EventHandlerContext);
  if (!context) {
    throw new Error(
      'useEventHandler must be used within an EventHandlerProvider'
    );
  }
  return context.eventHandler;
};
