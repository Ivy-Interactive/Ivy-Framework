export interface ContributionDay {
  date: string; // "YYYY-MM-DD"
  count: number;
}

export type IvyEventHandler = (
  eventName: string,
  widgetId: string,
  args: unknown[]
) => void;

export interface ActivityHeatmapProps {
  id: string;
  width?: string;
  height?: string;
  events?: string[];
  eventHandler: IvyEventHandler;
  data?: ContributionDay[];
  colorScheme?: string;
  showTooltip?: boolean;
  showMonthLabels?: boolean;
  showDayLabels?: boolean;
  startDate?: string; // "YYYY-MM-DD"
  endDate?: string;   // "YYYY-MM-DD"
}
