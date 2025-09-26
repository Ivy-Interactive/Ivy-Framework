import { createContext } from 'react';
import { ThemeProviderState } from './types';

const initialState: ThemeProviderState = {
  theme: 'light',
  setTheme: () => null,
};

export const ThemeProviderContext =
  createContext<ThemeProviderState>(initialState);
