import { Theme } from './types';

// Create a global reference that can be accessed from anywhere
// This solves the closure issue with useBackend
const globalThemeRef = {
  setTheme: null as ((theme: Theme) => void) | null,
};

// Export the global reference for direct access
export const setThemeGlobal = (theme: Theme) => {
  if (globalThemeRef.setTheme) {
    globalThemeRef.setTheme(theme);
    return true;
  }
  return false;
};

export { globalThemeRef };
