export interface Activity {
  date: string; // "YYYY-MM-DD"
  count: number | undefined;
}

export type IvyEventHandler = (
  eventName: string,
  widgetId: string,
  args: unknown[]
) => void;

export interface ActivityHeatmapProps {
  id: string;
  events?: string[];
  eventHandler: IvyEventHandler;
  data?: Activity[];
  valueLabel?: string;
  colorScheme?: string;
  showTooltip?: boolean;
  showMonthLabels?: boolean;
  showDayLabels?: boolean;
  startDate?: string; // "YYYY-MM-DD"
  endDate?: string;   // "YYYY-MM-DD"
}