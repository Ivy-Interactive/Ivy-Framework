import React from "react";
import { StreamSubscriber } from "./types";
import { StreamHandlerContext } from "./context";

export const StreamHandlerProvider: React.FC<{
  subscribeToStream: StreamSubscriber;
  children: React.ReactNode;
}> = ({ subscribeToStream, children }) => (
  <StreamHandlerContext.Provider value={{ subscribeToStream }}>
    {children}
  </StreamHandlerContext.Provider>
);
