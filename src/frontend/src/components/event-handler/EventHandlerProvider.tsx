import React from 'react';
import { WidgetEventHandlerType } from '@/types/widgets';
import { EventHandlerContext } from './context';

export const EventHandlerProvider: React.FC<{
  eventHandler: WidgetEventHandlerType;
  children: React.ReactNode;
}> = ({ eventHandler, children }) => (
  <EventHandlerContext.Provider value={{ eventHandler }}>
    {children}
  </EventHandlerContext.Provider>
);
