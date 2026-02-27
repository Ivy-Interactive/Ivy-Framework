import type { MenuItem, WidgetEventHandlerType } from '@/types/widgets';

export interface SidebarSearchState {
  items: MenuItem[];
  searchActive: boolean;
  flatItems: MenuItem[];
  /** Full menu tree (for window-only search); set when searchActive is false */
  fullMenuItems: MenuItem[];
  eventHandler: WidgetEventHandlerType;
  id: string;
  activeTag: string | null | undefined;
}

let state: SidebarSearchState = {
  items: [],
  searchActive: false,
  flatItems: [],
  fullMenuItems: [],
  eventHandler: () => {},
  id: '',
  activeTag: null,
};

const listeners = new Set<() => void>();

function getState(): SidebarSearchState {
  return state;
}

function setState(next: Partial<SidebarSearchState>) {
  state = { ...state, ...next };
  listeners.forEach(l => l());
}

function subscribe(listener: () => void): () => void {
  listeners.add(listener);
  return () => listeners.delete(listener);
}

export const sidebarSearchStore = {
  getState,
  setState,
  subscribe,
};
