/**
 * Custom hook for managing dropdown open/closed state
 * Can be used by consumers to control and track the dropdown state
 */

import { useState, useCallback, useRef, useEffect } from "react";

export interface DropdownState {
  /** Whether the dropdown is currently open */
  isOpen: boolean;
  /** Function to open the dropdown */
  open: () => void;
  /** Function to close the dropdown */
  close: () => void;
  /** Function to toggle the dropdown state */
  toggle: () => void;
  /** Function to set the dropdown state directly */
  setIsOpen: (isOpen: boolean) => void;
  /**
   * Ref object that always contains the current state
   * Useful for accessing state in callbacks without stale closures
   */
  stateRef: React.MutableRefObject<boolean>;
}

/**
 * Hook to manage dropdown open/closed state
 *
 * @param initialState - Initial state of the dropdown (default: false)
 * @returns DropdownState object with state and control functions
 *
 * @example
 * Basic usage:
 * ```tsx
 * const dropdownState = useDropdownState();
 *
 * <QueryEditor
 *   value={query}
 *   columns={columns}
 *   onChange={handleChange}
 *   isOpen={dropdownState.isOpen}
 *   onOpenChange={dropdownState.setIsOpen}
 * />
 *
 * // Track when dropdown opens/closes
 * useEffect(() => {
 *   console.log('Dropdown is:', dropdownState.isOpen ? 'open' : 'closed');
 * }, [dropdownState.isOpen]);
 * ```
 *
 * @example
 * Using stateRef to always get the current state:
 * ```tsx
 * const dropdownState = useDropdownState();
 *
 * const handleSomething = () => {
 *   // Always get the latest state, even in stale closures
 *   const isCurrentlyOpen = dropdownState.stateRef.current;
 *   console.log('Current state:', isCurrentlyOpen);
 * };
 * ```
 */
export function useDropdownState(initialState: boolean = false): DropdownState {
  const [isOpen, setIsOpen] = useState<boolean>(initialState);
  const stateRef = useRef<boolean>(initialState);

  // Keep ref in sync with state
  useEffect(() => {
    stateRef.current = isOpen;
  }, [isOpen]);

  const open = useCallback(() => {
    setIsOpen(true);
  }, []);

  const close = useCallback(() => {
    setIsOpen(false);
  }, []);

  const toggle = useCallback(() => {
    setIsOpen((prev) => !prev);
  }, []);

  return {
    isOpen,
    open,
    close,
    toggle,
    setIsOpen,
    stateRef,
  };
}
