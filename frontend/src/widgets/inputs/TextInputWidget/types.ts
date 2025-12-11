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

export interface Affix {
  icon?: string;
  text?: string;
}

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
  prefix?: Affix;
  suffix?: Affix;
  maxLength?: number;
  'data-testid'?: string;
}
