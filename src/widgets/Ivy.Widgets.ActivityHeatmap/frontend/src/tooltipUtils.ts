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

export function getTooltipTransform(tooltipDiv: HTMLDivElement | null, tooltipCoordinates: { x: number; y: number }, gridContainer: HTMLDivElement | null): string {
  if (!tooltipDiv || !gridContainer) return "translate(0px, 0px)";
  const gridRect = gridContainer.getBoundingClientRect();
  const xOffset = tooltipCoordinates.x > gridRect.left + gridRect.width / 2 ? -(tooltipDiv.offsetWidth + 20) : 20;
  const yOffset = tooltipCoordinates.y > window.innerHeight / 2 ? -(tooltipDiv.offsetHeight + 20) : 20;
  return `translate(${xOffset}px, ${yOffset}px)`;
}