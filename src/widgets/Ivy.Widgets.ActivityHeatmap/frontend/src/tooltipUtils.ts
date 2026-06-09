import { Activity } from "./types";

const preferredLanguage = navigator.languages.length ? navigator.languages : navigator.language;

export function formatTooltipHeader(day: Activity): string {
  const dateString = new Date(day.date + "T00:00:00")
    .toLocaleDateString(preferredLanguage, { month: "short", day: "numeric", year: "numeric" });

  if (day.hour != null) {
    return `${dateString}, ${String(day.hour).padStart(2, "0")}:00`;
  }
  return dateString;
}

export function formatTooltipValue(day: Activity): string {
  const label = day.count === 1 ? "contribution" : "contributions";
  return `${day.count ?? 0} ${label}`;
}

export function getTooltipTransform(tooltipDiv: HTMLDivElement | null, tooltipCoordinates: { x: number; y: number }, gridContainer: HTMLDivElement | null): string {
  if (!tooltipDiv || !gridContainer) return "translate(0px, 0px)";
  const gridRect = gridContainer.getBoundingClientRect();
  const xOffset = tooltipCoordinates.x > gridRect.left + gridRect.width / 2 ? -(tooltipDiv.offsetWidth + 20) : 20;
  const yOffset = tooltipCoordinates.y > window.innerHeight / 2 ? -(tooltipDiv.offsetHeight + 20) : 20;
  return `translate(${xOffset}px, ${yOffset}px)`;
}

export function formatDimensionLabel(date: Date, isHourly?: boolean): string {
  if (isHourly) {
    return date.toLocaleDateString(preferredLanguage, { month: "short", day: "numeric" });
  }
  return date.toLocaleDateString(preferredLanguage, { month: "short" });
}
