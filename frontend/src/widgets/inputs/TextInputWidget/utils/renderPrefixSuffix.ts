import React from 'react';
import Icon from '@/components/Icon';
import { PrefixSuffix } from '../types';

/**
 * Renders either text or icon for prefix/suffix display.
 * Icon takes priority if both are set.
 */
export const renderPrefixSuffix = (
  prefixSuffix?: PrefixSuffix
): React.ReactNode => {
  if (!prefixSuffix) return null;

  if (prefixSuffix.icon) {
    return React.createElement(Icon, {
      name: prefixSuffix.icon,
      className: 'w-4 h-4',
    });
  }

  if (prefixSuffix.text) {
    return React.createElement(
      'span',
      { className: 'text-sm' },
      prefixSuffix.text
    );
  }

  return null;
};
