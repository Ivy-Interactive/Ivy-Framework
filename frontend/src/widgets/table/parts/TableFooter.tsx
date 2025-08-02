import React from 'react';
import { useTable } from '../context/TableContext';
import { tableStyles } from '../styles/style';

export const Footer: React.FC = () => {
  const { editable } = useTable();
  return (
    <div className={tableStyles.text.footer}>
      {editable
        ? 'Click any cell to edit. Drag column borders to resize.'
        : 'Data fetched from gRPC service. Grid grows dynamically as you scroll.'}
    </div>
  );
};
