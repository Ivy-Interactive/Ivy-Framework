import { Scales } from '@/types/scale';

export enum TextInputVariant {
  Text = 'Text',
  Textarea = 'Textarea',
  Email = 'Email',
  Tel = 'Tel',
  Url = 'Url',
  Password = 'Password',
  Search = 'Search',
}

export type PrefixSuffix =
  | { type: 'text'; value: string }
  | { type: 'icon'; value: string };

export interface TextInputWidgetProps {
  id: string;
  placeholder?: string;
  value?: string;
  variant: TextInputVariant;
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
