const listeners = new Set<() => void>();
let isOpen = false;
/** Panel width as fraction of viewport width (0 to 1). */
let panelWidthFraction = 0;
let cachedSnapshot = { isOpen: false, panelWidthFraction: 0 };

function getState() {
  if (
    cachedSnapshot.isOpen !== isOpen ||
    cachedSnapshot.panelWidthFraction !== panelWidthFraction
  ) {
    cachedSnapshot = { isOpen, panelWidthFraction };
  }
  return cachedSnapshot;
}

function setOpen(open: boolean) {
  if (isOpen === open) return;
  isOpen = open;
  if (!open) panelWidthFraction = 0;
  listeners.forEach(l => l());
}

function setPanelWidthFraction(fraction: number) {
  if (panelWidthFraction === fraction) return;
  panelWidthFraction = fraction;
  listeners.forEach(l => l());
}

function subscribe(listener: () => void): () => void {
  listeners.add(listener);
  return () => listeners.delete(listener);
}

export const mcpPanelStore = {
  getState,
  setOpen,
  setPanelWidthFraction,
  subscribe,
};
