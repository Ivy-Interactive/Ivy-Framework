import { createContext, useContext } from "react";

export interface SidebarLayoutContextValue {
  collapsed: boolean;
  expand: () => void;
  toggle: () => void;
}

export const SidebarLayoutContext = createContext<SidebarLayoutContextValue>({
  collapsed: false,
  expand: () => {},
  toggle: () => {},
});

export const useSidebarLayout = () => useContext(SidebarLayoutContext);
