import { useContext, useEffect, useState, useCallback, useMemo } from 'react';
import { ThemeProviderContext } from './context';
import { getThemeColors, isDarkMode, ThemeColors } from '@/lib/color-utils';
import type { Theme } from '@glideapps/glide-data-grid';

export const useTheme = () => {
  const context = useContext(ThemeProviderContext);

  if (context === undefined)
    throw new Error('useTheme must be used within a ThemeProvider');

  return context;
};

export interface ThemeMonitorOptions {
  /**
   * Whether to monitor DOM changes for theme updates
   * @default true
   */
  monitorDOM?: boolean;

  /**
   * Whether to monitor system theme preference changes
   * @default true
   */
  monitorSystem?: boolean;

  /**
   * Delay in milliseconds before reading theme colors after a change
   * @default 50
   */
  updateDelay?: number;

  /**
   * Custom theme generator function
   */
  customThemeGenerator?: (
    colors: ThemeColors,
    isDark: boolean
  ) => Partial<Theme>;
}

export interface ThemeMonitorResult<T = ThemeColors> {
  /**
   * Current theme mode from context
   */
  theme: 'light' | 'dark' | 'system';

  /**
   * Function to set theme mode
   */
  setTheme: (theme: 'light' | 'dark' | 'system') => void;

  /**
   * Whether dark mode is currently active
   */
  isDark: boolean;

  /**
   * Current theme colors from CSS variables
   */
  colors: ThemeColors;

  /**
   * Custom theme object if generator provided
   */
  customTheme?: T;

  /**
   * Force refresh theme colors
   */
  refreshTheme: () => void;
}

/**
 * Enhanced theme hook with DOM monitoring and custom theme support
 */
export function useThemeWithMonitoring<T = Partial<Theme>>(
  options: ThemeMonitorOptions = {}
): ThemeMonitorResult<T> {
  const {
    monitorDOM = true,
    monitorSystem = true,
    updateDelay = 50,
    customThemeGenerator,
  } = options;

  const { theme, setTheme } = useTheme();
  const [isDark, setIsDark] = useState(isDarkMode());
  const [colors, setColors] = useState<ThemeColors>(() => getThemeColors());

  // Generate custom theme if generator provided
  const customTheme = useMemo(() => {
    if (customThemeGenerator) {
      return customThemeGenerator(colors, isDark) as T;
    }
    return undefined;
  }, [colors, isDark, customThemeGenerator]);

  // Function to update theme state
  const updateThemeState = useCallback(() => {
    requestAnimationFrame(() => {
      setTimeout(() => {
        const newIsDark = isDarkMode();
        const newColors = getThemeColors();
        setIsDark(newIsDark);
        setColors(newColors);
      }, updateDelay);
    });
  }, [updateDelay]);

  // Manual refresh function
  const refreshTheme = useCallback(() => {
    updateThemeState();
  }, [updateThemeState]);

  useEffect(() => {
    // Initial update
    updateThemeState();

    const cleanupFunctions: (() => void)[] = [];

    // Monitor DOM changes
    if (monitorDOM && typeof document !== 'undefined') {
      const observer = new MutationObserver(mutations => {
        mutations.forEach(mutation => {
          if (
            mutation.type === 'attributes' &&
            mutation.attributeName === 'class'
          ) {
            updateThemeState();
          }
        });
      });

      observer.observe(document.documentElement, {
        attributes: true,
        attributeFilter: ['class'],
      });

      cleanupFunctions.push(() => observer.disconnect());
    }

    // Monitor system theme preference
    if (monitorSystem && typeof window !== 'undefined') {
      const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');

      const handleMediaChange = () => {
        if (theme === 'system') {
          updateThemeState();
        }
      };

      if (mediaQuery.addEventListener) {
        mediaQuery.addEventListener('change', handleMediaChange);
        cleanupFunctions.push(() =>
          mediaQuery.removeEventListener('change', handleMediaChange)
        );
      } else {
        // Fallback for older browsers
        mediaQuery.addListener(handleMediaChange);
        cleanupFunctions.push(() =>
          mediaQuery.removeListener(handleMediaChange)
        );
      }
    }

    // Cleanup
    return () => {
      cleanupFunctions.forEach(cleanup => cleanup());
    };
  }, [theme, monitorDOM, monitorSystem, updateThemeState]);

  return {
    theme,
    setTheme,
    isDark,
    colors,
    customTheme,
    refreshTheme,
  };
}
