import { Densities } from "@/types/density";

export type VariantType = "Date" | "DateTime" | "Time" | "Month" | "Week" | "Year";

export type WeekDay = 0 | 1 | 2 | 3 | 4 | 5 | 6;

export interface DateTimeInputWidgetProps {
  id: string;
  value?: string;
  placeholder?: string;
  disabled: boolean;
  variant?: VariantType;
  nullable?: boolean;
  invalid?: string;
  format?: string;
  firstDayOfWeek?: WeekDay | string;
  min?: string;
  max?: string;
  step?: string;
  density?: Densities;
  "data-testid"?: string;
}

export interface BaseVariantProps {
  id: string;
  value?: string;
  placeholder?: string;
  disabled: boolean;
  nullable?: boolean;
  invalid?: string;
  format?: string;
  firstDayOfWeek?: WeekDay;
  min?: string;
  max?: string;
  step?: string;
  density?: Densities;
  "data-testid"?: string;
}

export interface DateChangeProp {
  onDateChange: (date: Date | undefined) => void;
}

export interface TimeChangeProp {
  onTimeChange: (time: string) => void;
}

export type DateVariantProps = BaseVariantProps & DateChangeProp;

export type DateTimeVariantProps = BaseVariantProps & DateChangeProp & TimeChangeProp;

export type TimeVariantProps = BaseVariantProps & TimeChangeProp;

export type MonthVariantProps = BaseVariantProps & DateChangeProp;

export type WeekVariantProps = BaseVariantProps & DateChangeProp;

export type YearVariantProps = BaseVariantProps & DateChangeProp;
