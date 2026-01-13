import { useEffect, useState, useCallback } from 'react';

import { Theme, ThemeProviderProps } from './types';
import { globalThemeRef } from './utils';
import { ThemeProviderContext } from './context';

export function ThemeProvider({
  children,
  defaultTheme = 'light',
  storageKey = 'ivy-ui-theme',
  ...props
}: ThemeProviderProps) {
  const [theme, setThemeState] = useState<Theme>(
    () => (localStorage.getItem(storageKey) as Theme) || defaultTheme
  );

  // Function to set theme that updates localStorage and state
  const setTheme = useCallback(
    (newTheme: Theme) => {
      localStorage.setItem(storageKey, newTheme);
      setThemeState(newTheme);
    },
    [storageKey]
  );

  // Store the setTheme function in the global ref
  // This allows direct access from outside the React component tree
  useEffect(() => {
    globalThemeRef.setTheme = setTheme;
    return () => {
      globalThemeRef.setTheme = null;
    };
  }, [setTheme]);

  // Apply theme to document element
  useEffect(() => {
    const root = window.document.documentElement;

    root.classList.remove('light', 'dark');

    if (theme === 'system') {
      const systemTheme = window.matchMedia('(prefers-color-scheme: dark)')
        .matches
        ? 'dark'
        : 'light';

      root.classList.add(systemTheme);
      return;
    }

    root.classList.add(theme);
  }, [theme]);

  const value = {
    theme,
    setTheme,
  };

  return (
    <ThemeProviderContext.Provider {...props} value={value}>
      {children}
    </ThemeProviderContext.Provider>
  );
}
