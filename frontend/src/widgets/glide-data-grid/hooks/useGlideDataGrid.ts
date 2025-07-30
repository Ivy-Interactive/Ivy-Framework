import { useContext } from 'react';
import {
  GlideDataGridContext,
  type GlideDataGridContextValue,
} from '../context/GlideDataGridContext';

export const useGlideDataGrid = (): GlideDataGridContextValue => {
  const context = useContext(GlideDataGridContext);
  if (!context) {
    // Todo: Ivy Error Handling?
    throw new Error(
      'useGlideDataGrid must be used within a GlideDataGridProvider'
    );
  }
  return context;
};
