const listeners = new Set<() => void>();
let isOpen = false;
let cachedSnapshot = { isOpen: false };

function getState() {
  if (cachedSnapshot.isOpen !== isOpen) {
    cachedSnapshot = { isOpen };
  }
  return cachedSnapshot;
}

function setOpen(open: boolean) {
  if (isOpen === open) return;
  isOpen = open;
  listeners.forEach(l => l());
}

function subscribe(listener: () => void): () => void {
  listeners.add(listener);
  return () => listeners.delete(listener);
}

export const mcpPanelStore = {
  getState,
  setOpen,
  subscribe,
};
