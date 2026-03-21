import React from 'react';

/**
 * Hook to manage the animated underline for Content variant tabs
 * Returns the active style (left position and width) for the underline indicator
 */
export function useAnimation(
  variant: 'Tabs' | 'Content',
  activeIndex: number,
  tabOrder: string[],
  visibleTabs: string[],
  tabRefs: React.MutableRefObject<(HTMLButtonElement | null)[]>
) {
  const [activeStyle, setActiveStyle] = React.useState({
    left: '0px',
    width: '0px',
  });
  const [isInitialRender, setIsInitialRender] = React.useState(true);

  React.useEffect(() => {
    if (variant !== 'Content') return;
    const activeElement = tabRefs.current[activeIndex];
    if (activeElement) {
      const { offsetLeft, offsetWidth } = activeElement;
      setActiveStyle({
        left: `${offsetLeft}px`,
        width: `${offsetWidth}px`,
      });

      // After first position update, enable animations
      if (isInitialRender) {
        requestAnimationFrame(() => {
          setIsInitialRender(false);
        });
      }
    }
  }, [activeIndex, tabOrder, variant, isInitialRender, visibleTabs, tabRefs]);

  return { activeStyle, isInitialRender };
}
