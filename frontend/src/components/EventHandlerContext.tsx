import { WidgetEventHandlerType } from "@/types/widgets";
import React, { createContext, useContext } from "react";

interface EventHandlerContextProps {
  eventHandler: WidgetEventHandlerType;
}

const EventHandlerContext = createContext<EventHandlerContextProps | undefined>(
  undefined
);

export const EventHandlerProvider: React.FC<{
  eventHandler: WidgetEventHandlerType;
  children: React.ReactNode;
}> = ({ eventHandler, children }) => (
  <EventHandlerContext.Provider value={{ eventHandler }}>
    {children}
  </EventHandlerContext.Provider>
);

export const useEventHandler = (): WidgetEventHandlerType => {
  const context = useContext(EventHandlerContext);
  if (!context) {
    throw new Error(
      "useEventHandler must be used within an EventHandlerProvider"
    );
  }
  return context.eventHandler;
};

export type EventHandler = (event: string, id: string, args: any[]) => void;
