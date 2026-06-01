export interface Activity {
  date: string; // "YYYY-MM-DD"
  hour?: number | null; // 0-23 for hourly; null/undefined for daily
  count: number | undefined;
}

export type ActivityInterval = "Daily" | "Hourly";

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
  interval?: ActivityInterval;
  startDate?: string; // "YYYY-MM-DD"
  endDate?: string;   // "YYYY-MM-DD"
  width?: string;
  height?: string;
}