import React from 'react';
import { tableStyles } from '../styles';

interface FooterProps {
  editable: boolean;
}

export const Footer: React.FC<FooterProps> = ({ editable }) => {
  return (
    <div className={tableStyles.footerText}>
      {editable
        ? 'Click any cell to edit. Drag column borders to resize.'
        : 'Data fetched from gRPC service. Grid grows dynamically as you scroll.'}
    </div>
  );
};
