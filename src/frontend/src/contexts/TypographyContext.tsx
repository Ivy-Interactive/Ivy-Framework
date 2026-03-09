import { createContext, useContext } from 'react';
import { typography } from '@/lib/styles';

export const TypographyContext =
  createContext<Record<string, string>>(typography);

export const useTypography = () => useContext(TypographyContext);
