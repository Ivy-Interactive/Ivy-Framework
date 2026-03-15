import { Densities } from '@/types/density';

export type VariantType =
  | 'Date'
  | 'DateTime'
  | 'Time'
  | 'Month'
  | 'Week'
  | 'Year';

export interface DateTimeInputWidgetProps {
  id: string;
  value?: string;
  placeholder?: string;
  disabled: boolean;
  variant?: VariantType;
  nullable?: boolean;
  invalid?: string;
  format?: string;
  density?: Densities;
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
  density?: Densities;
  'data-testid'?: string;
}

export interface DateChangeProp {
  onDateChange: (date: Date | undefined) => void;
}

export interface TimeChangeProp {
  onTimeChange: (time: string) => void;
}

export type DateVariantProps = BaseVariantProps & DateChangeProp;

export type DateTimeVariantProps = BaseVariantProps &
  DateChangeProp &
  TimeChangeProp;

export type TimeVariantProps = BaseVariantProps & TimeChangeProp;

export type MonthVariantProps = BaseVariantProps & DateChangeProp;

export type WeekVariantProps = BaseVariantProps & DateChangeProp;

export type YearVariantProps = BaseVariantProps & DateChangeProp;
