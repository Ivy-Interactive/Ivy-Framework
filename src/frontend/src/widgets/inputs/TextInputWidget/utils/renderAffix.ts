import React from 'react';
import Icon from '@/components/Icon';
import { Affix } from '../types';

/**
 * Renders either text or icon for prefix/suffix display.
 * Icon takes priority if both are set.
 */
export const renderAffix = (affix?: Affix): React.ReactNode => {
  if (!affix) return null;

  if (affix.icon) {
    return React.createElement(Icon, {
      name: affix.icon,
      className: 'w-4 h-4',
    });
  }

  if (affix.text) {
    return React.createElement('span', { className: 'text-sm' }, affix.text);
  }

  return null;
};
