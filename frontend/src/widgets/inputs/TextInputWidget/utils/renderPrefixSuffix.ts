import React from 'react';
import Icon from '@/components/Icon';
import { PrefixSuffix } from '../types';

/**
 * Renders either text or icon for prefix/suffix display.
 * Uses discriminated union type to ensure only one type can be set.
 */
export const renderPrefixSuffix = (
  prefixSuffix?: PrefixSuffix
): React.ReactNode => {
  if (!prefixSuffix) return null;

  if (prefixSuffix.type === 'icon') {
    return React.createElement(Icon, {
      name: prefixSuffix.value,
      className: 'w-4 h-4',
    });
  }

  return React.createElement(
    'span',
    { className: 'text-sm' },
    prefixSuffix.value
  );
};
