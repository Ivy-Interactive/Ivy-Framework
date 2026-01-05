import { Scales } from '@/types/scale';

export type VariantType = 'Date' | 'DateTime' | 'Time';

export interface DateTimeInputWidgetProps {
  id: string;
  value?: string;
  placeholder?: string;
  disabled: boolean;
  variant?: VariantType;
  nullable?: boolean;
  invalid?: string;
  format?: string;
  scale?: Scales;
  'data-testid'?: string;
}

export interface BaseVariantProps {
  id: string;
  value?: string;
  placeholder?: string;
  disabled: boolean;
  nullable?: boolean;
  invalid?: string;
  format?: string;
  scale?: Scales;
  'data-testid'?: string;
}

export interface DateVariantProps extends BaseVariantProps {
  onDateChange: (date: Date | undefined) => void;
}

export interface DateTimeVariantProps extends BaseVariantProps {
  onDateChange: (date: Date | undefined) => void;
  onTimeChange: (time: string) => void;
}

export interface TimeVariantProps extends BaseVariantProps {
  onTimeChange: (time: string) => void;
}
