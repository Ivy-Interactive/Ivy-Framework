import { EventHandler } from "@/components/event-handler";
import { Densities } from "@/types/density";

export type NullableSelectValue = string | number | string[] | number[] | null | undefined;

export interface Option {
  value: string | number;
  label?: string;
  description?: string;
  group?: string;
  icon?: string;
  disabled?: boolean;
  tooltip?: string;
}

export interface SelectInputWidgetProps {
  id: string;
  placeholder?: string;
  value?: NullableSelectValue;
  variant?: "Select" | "List" | "Toggle" | "Slider" | "Radio";
  nullable?: boolean;
  disabled?: boolean;
  invalid?: string;
  options: Option[];
  eventHandler: EventHandler;
  selectMany: boolean;
  separator: string;
  maxSelections?: number;
  minSelections?: number;
  searchable?: boolean;
  searchMode?: "CaseInsensitive" | "CaseSensitive" | "Fuzzy";
  emptyMessage?: string;
  loading?: boolean;
  ghost?: boolean;
  "data-testid"?: string;
  density?: Densities;
  width?: string;
  events?: string[];
}
