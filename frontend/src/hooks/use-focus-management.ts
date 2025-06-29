import { useCallback, useRef, useEffect } from 'react';

export type FocusDirection = 'next' | 'previous' | 'first' | 'last';

export interface FocusManager {
  focusNext: () => void;
  focusPrevious: () => void;
  focusFirst: () => void;
  focusLast: () => void;
  registerElement: (element: HTMLElement, priority?: number) => void;
  unregisterElement: (element: HTMLElement) => void;
}

const focusRegistry = new Map<string, HTMLElement[]>();

export const useFocusManagement = (groupId: string): FocusManager => {
  const getElements = useCallback(() => {
    return focusRegistry.get(groupId) || [];
  }, [groupId]);

  const focusNext = useCallback(() => {
    const elements = getElements();
    console.log('🔍 Focus Next - Group:', groupId, 'Elements:', elements.length, elements.map(el => el.id || el.tagName));
    if (elements.length === 0) return;

    const activeElement = document.activeElement as HTMLElement;
    const currentIndex = elements.indexOf(activeElement);
    console.log('🔍 Current active element:', activeElement?.id || activeElement?.tagName, 'Index:', currentIndex);
    
    if (currentIndex === -1 || currentIndex === elements.length - 1) {
      // Focus first element if no current focus or at last element
      console.log('🔍 Focusing first element:', elements[0]?.id || elements[0]?.tagName);
      elements[0]?.focus();
    } else {
      // Focus next element
      console.log('🔍 Focusing next element:', elements[currentIndex + 1]?.id || elements[currentIndex + 1]?.tagName);
      elements[currentIndex + 1]?.focus();
    }
  }, [getElements, groupId]);

  const focusPrevious = useCallback(() => {
    const elements = getElements();
    console.log('🔍 Focus Previous - Group:', groupId, 'Elements:', elements.length, elements.map(el => el.id || el.tagName));
    if (elements.length === 0) return;

    const activeElement = document.activeElement as HTMLElement;
    const currentIndex = elements.indexOf(activeElement);
    console.log('🔍 Current active element:', activeElement?.id || activeElement?.tagName, 'Index:', currentIndex);
    
    if (currentIndex === -1 || currentIndex === 0) {
      // Focus last element if no current focus or at first element
      console.log('🔍 Focusing last element:', elements[elements.length - 1]?.id || elements[elements.length - 1]?.tagName);
      elements[elements.length - 1]?.focus();
    } else {
      // Focus previous element
      console.log('🔍 Focusing previous element:', elements[currentIndex - 1]?.id || elements[currentIndex - 1]?.tagName);
      elements[currentIndex - 1]?.focus();
    }
  }, [getElements, groupId]);

  const focusFirst = useCallback(() => {
    const elements = getElements();
    elements[0]?.focus();
  }, [getElements]);

  const focusLast = useCallback(() => {
    const elements = getElements();
    elements[elements.length - 1]?.focus();
  }, [getElements]);

  const registerElement = useCallback((element: HTMLElement) => {
    const elements = getElements();
    elements.push(element);
    // Sort by priority (lower numbers = higher priority)
    elements.sort((a, b) => {
      const aPriority = parseInt(a.getAttribute('data-focus-priority') || '0');
      const bPriority = parseInt(b.getAttribute('data-focus-priority') || '0');
      return aPriority - bPriority;
    });
    focusRegistry.set(groupId, elements);
    console.log('🔍 Registered element:', element.id || element.tagName, 'Priority:', element.getAttribute('data-focus-priority'), 'Group:', groupId, 'Total elements:', elements.length);
  }, [groupId, getElements]);

  const unregisterElement = useCallback((element: HTMLElement) => {
    const elements = getElements();
    const index = elements.indexOf(element);
    if (index > -1) {
      elements.splice(index, 1);
      focusRegistry.set(groupId, elements);
    }
  }, [groupId, getElements]);

  return {
    focusNext,
    focusPrevious,
    focusFirst,
    focusLast,
    registerElement,
    unregisterElement
  };
};

// Hook for components that want to participate in focus management
export const useFocusable = (groupId: string, priority: number = 0) => {
  const focusManager = useFocusManagement(groupId);
  const elementRef = useRef<HTMLElement | null>(null);
  
  const ref = useCallback((element: HTMLElement | null) => {
    // Unregister previous element if it exists
    if (elementRef.current) {
      focusManager.unregisterElement(elementRef.current);
    }
    
    // Register new element
    if (element) {
      element.setAttribute('data-focus-priority', priority.toString());
      focusManager.registerElement(element);
      elementRef.current = element;
    } else {
      elementRef.current = null;
    }
  }, [focusManager, priority]);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      if (elementRef.current) {
        focusManager.unregisterElement(elementRef.current);
      }
    };
  }, [focusManager]);

  return { ref, focusManager };
}; 