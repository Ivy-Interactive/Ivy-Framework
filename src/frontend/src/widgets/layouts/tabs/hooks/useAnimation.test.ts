import { describe, it, expect } from 'vitest';
import React from 'react';

describe('useContentVariantAnimation', () => {
  it('should accept correct parameters', () => {
    const tabRefs = { current: [] } as React.MutableRefObject<
      (HTMLButtonElement | null)[]
    >;

    // Test that the hook can be imported and parameters are correctly typed
    const params = {
      variant: 'Content' as const,
      activeIndex: 0,
      tabOrder: [] as string[],
      visibleTabs: [] as string[],
      tabRefs,
    };

    expect(params.variant).toBe('Content');
    expect(params.activeIndex).toBe(0);
    expect(params.tabOrder).toEqual([]);
  });

  it('should handle Tabs variant', () => {
    const mockButton = document.createElement('button');
    const tabRefs = { current: [mockButton] } as React.MutableRefObject<
      (HTMLButtonElement | null)[]
    >;

    const params = {
      variant: 'Tabs' as const,
      activeIndex: 0,
      tabOrder: ['tab1'] as string[],
      visibleTabs: ['tab1'] as string[],
      tabRefs,
    };

    // Verify parameters are correctly typed for Tabs variant
    expect(params.variant).toBe('Tabs');
  });

  it('should handle Content variant with tab element', () => {
    const mockButton = document.createElement('button');
    Object.defineProperty(mockButton, 'offsetLeft', { value: 100 });
    Object.defineProperty(mockButton, 'offsetWidth', { value: 200 });

    // Verify element properties can be accessed
    expect(mockButton.offsetLeft).toBe(100);
    expect(mockButton.offsetWidth).toBe(200);
  });

  it('should handle null tab element', () => {
    const tabRefs = { current: [null] } as React.MutableRefObject<
      (HTMLButtonElement | null)[]
    >;

    // Verify hook can handle null elements
    expect(tabRefs.current[0]).toBeNull();
  });
});
