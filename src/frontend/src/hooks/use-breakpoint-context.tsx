import React, { createContext, useContext, type RefObject } from "react";
import { type BreakpointName, useBreakpoint } from "./use-responsive";

const BreakpointContext = createContext<BreakpointName>("desktop");

interface BreakpointProviderProps {
  children: React.ReactNode;
  /**
   * When provided, the breakpoint is derived from this element's width rather than
   * the viewport. Mount the provider inside the app content area and pass the
   * content container so responsive widgets react to the space actually available
   * to them (e.g. an expanded sidebar narrows the content without changing the
   * viewport width). Omit it for window-based behavior.
   */
  containerRef?: RefObject<HTMLElement | null>;
}

export const BreakpointProvider: React.FC<BreakpointProviderProps> = ({
  children,
  containerRef,
}) => {
  const breakpoint = useBreakpoint(containerRef);
  return <BreakpointContext.Provider value={breakpoint}>{children}</BreakpointContext.Provider>;
};

export const useCurrentBreakpoint = (): BreakpointName => useContext(BreakpointContext);
