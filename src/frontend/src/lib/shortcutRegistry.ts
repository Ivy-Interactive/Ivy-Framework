import { keyToCode, type ParsedShortcut } from "./shortcut";

type ShortcutHandler = () => void;

export interface ShortcutRegistration {
  id: string;
  shortcut: ParsedShortcut;
  handler: ShortcutHandler;
  /** Returns true if this handler should be active (e.g., not in aria-hidden container) */
  isActive: () => boolean;
  /** If true, skip firing when target is INPUT/TEXTAREA/contentEditable */
  skipInInputs: boolean;
}

const registry = new Map<string, ShortcutRegistration>();

// Debounce state for keyboard shortcuts to prevent duplicate triggers during UI transitions
const recentShortcuts = new Map<string, number>();
const DEBOUNCE_MS = 300;
const SWEEP_INTERVAL_MS = 5 * DEBOUNCE_MS; // 1500ms
let sweepTimerId: number | null = null;

function startSweepIfNeeded() {
  if (sweepTimerId !== null) return;
  sweepTimerId = window.setInterval(() => {
    const now = Date.now();
    for (const [id, timestamp] of recentShortcuts.entries()) {
      if (now - timestamp > SWEEP_INTERVAL_MS) {
        recentShortcuts.delete(id);
      }
    }
    if (recentShortcuts.size === 0 && sweepTimerId !== null) {
      clearInterval(sweepTimerId);
      sweepTimerId = null;
    }
  }, SWEEP_INTERVAL_MS);
}

function handleKeyDown(event: KeyboardEvent) {
  const target = event.target as HTMLElement;
  const isInputField =
    target.tagName === "INPUT" || target.tagName === "TEXTAREA" || target.isContentEditable;

  for (const registration of registry.values()) {
    const { shortcut, handler, isActive, skipInInputs, id } = registration;

    // Skip if this shortcut should be excluded in input fields
    if (skipInInputs && isInputField) continue;

    // Check modifier match
    const modifierMatch =
      (shortcut.meta && event.metaKey) ||
      (shortcut.ctrl && event.ctrlKey) ||
      (!shortcut.meta && !shortcut.ctrl && !event.metaKey && !event.ctrlKey);

    const expectedCode = keyToCode(shortcut.key);

    const isShortcutPressed =
      modifierMatch &&
      event.shiftKey === shortcut.shift &&
      event.altKey === shortcut.alt &&
      event.code === expectedCode;

    if (!isShortcutPressed) continue;

    // Check if registration is active
    if (!isActive()) continue;

    // Debounce check
    const now = Date.now();
    const lastTrigger = recentShortcuts.get(id) || 0;
    if (now - lastTrigger < DEBOUNCE_MS) continue;

    recentShortcuts.set(id, now);
    startSweepIfNeeded();

    event.preventDefault();
    handler();
  }
}

let listenerInstalled = false;

function installListener() {
  if (listenerInstalled) return;
  window.addEventListener("keydown", handleKeyDown);
  listenerInstalled = true;
}

function removeListener() {
  if (!listenerInstalled) return;
  window.removeEventListener("keydown", handleKeyDown);
  listenerInstalled = false;
}

export function registerShortcut(registration: ShortcutRegistration): void {
  registry.set(registration.id, registration);
  installListener();
}

export function unregisterShortcut(id: string): void {
  registry.delete(id);
  recentShortcuts.delete(id);
  if (registry.size === 0) {
    removeListener();
    if (sweepTimerId !== null) {
      clearInterval(sweepTimerId);
      sweepTimerId = null;
    }
  }
}

/** For testing only — resets all internal state */
export function _resetForTesting(): void {
  registry.clear();
  recentShortcuts.clear();
  if (sweepTimerId !== null) {
    clearInterval(sweepTimerId);
    sweepTimerId = null;
  }
  removeListener();
}

/** For testing only — returns the current registry size */
export function _getRegistrySize(): number {
  return registry.size;
}

/** For testing only — returns whether the global listener is installed */
export function _isListenerInstalled(): boolean {
  return listenerInstalled;
}
