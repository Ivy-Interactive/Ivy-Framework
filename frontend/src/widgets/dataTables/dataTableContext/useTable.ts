import { useContext } from 'react';
import { TableContext } from './tableContext';
import { TableContextType } from './types';

export const useTable = (): TableContextType => {
  const context = useContext(TableContext);
  if (!context) {
    throw new Error('useTable must be used within a TableProvider');
  }
  return context;
};
