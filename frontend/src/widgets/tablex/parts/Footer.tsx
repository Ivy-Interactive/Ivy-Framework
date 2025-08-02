import React from 'react';
import { tableStyles } from '../styles';
import { useTable } from '../context/TableContext';

export const Footer: React.FC = () => {
  const { editable } = useTable();
  return (
    <div className={tableStyles.footerText}>
      {editable
        ? 'Click any cell to edit. Drag column borders to resize.'
        : 'Data fetched from gRPC service. Grid grows dynamically as you scroll.'}
    </div>
  );
};
