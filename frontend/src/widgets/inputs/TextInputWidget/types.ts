import React from 'react';
import { Scales } from '@/types/scale';
import Icon from '@/components/Icon';

export type PrefixSuffix =
  | { type: 'text'; value: string }
  | { type: 'icon'; value: string };

export interface TextInputWidgetProps {
  id: string;
  placeholder?: string;
  value?: string;
  variant:
    | 'Text'
    | 'Textarea'
    | 'Email'
    | 'Tel'
    | 'Url'
    | 'Password'
    | 'Search';
  disabled: boolean;
  invalid?: string;
  events: string[];
  width?: string;
  height?: string;
  shortcutKey?: string;
  scale?: Scales;
  prefix?: PrefixSuffix;
  suffix?: PrefixSuffix;
  maxLength?: number;
  'data-testid'?: string;
}

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
