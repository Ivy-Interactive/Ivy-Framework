import { useContext } from 'react';
import { TableContext } from './TableContext';

export const useTableSize = () => {
  const context = useContext(TableContext);
  return context.size;
};
