import React, { createContext, useContext } from "react";
import { type BreakpointName, useBreakpoint } from "./use-responsive";

const BreakpointContext = createContext<BreakpointName>("desktop");

export const BreakpointProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const breakpoint = useBreakpoint();
  return <BreakpointContext.Provider value={breakpoint}>{children}</BreakpointContext.Provider>;
};

export const useCurrentBreakpoint = (): BreakpointName => useContext(BreakpointContext);
