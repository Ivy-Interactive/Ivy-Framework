import { useContext } from 'react';
import { TableContext } from './TableContext';

export const useTableScale = () => {
  const context = useContext(TableContext);
  return context.scale;
};
