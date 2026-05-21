import { Activity } from "./types";

const preferredLanguage = navigator.languages.length ? navigator.languages : navigator.language;
const monthFormatter = new Intl.DateTimeFormat(preferredLanguage, { month: "short" });
export const MONTH_NAMES = Array.from({ length: 12 }, (_, i) => monthFormatter.format(new Date(0, i)));

export function formatTooltipHeader(day: Activity): string {
  const date = new Date(day.date + "T00:00:00");
  const month = MONTH_NAMES[date.getMonth()];
  const dayNum = date.getDate();
  const year = date.getFullYear();
  return `${month} ${dayNum}, ${year}`;
}

export function formatTooltipValue(day: Activity): string {
  const label = day.count === 1 ? "contribution" : "contributions";
  return `${day.count} ${label}`;
}
