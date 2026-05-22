import { useRef, useState } from "react";
import "./style.css";
import { ActivityHeatmapProps, Activity } from "./types";
import { MONTH_NAMES, formatTooltipHeader, formatTooltipValue } from "./tooltipUtils";

function buildColorScheme(baseColor: string): string[] {
  return [
    "color-mix(in srgb, var(--color-neutral) 15%, transparent)",
    `color-mix(in srgb, ${baseColor} 25%, transparent)`,
    `color-mix(in srgb, ${baseColor} 50%, transparent)`,
    `color-mix(in srgb, ${baseColor} 75%, transparent)`,
    baseColor,
  ];
}

const weekdayFormatter = new Intl.DateTimeFormat(
  navigator.languages.length ? navigator.languages : navigator.language,
  { weekday: "short" }
);

const MONDAY = weekdayFormatter.format(new Date('2025-01-06'));
const WEDNESDAY = weekdayFormatter.format(new Date('2025-01-08'));
const FRIDAY = weekdayFormatter.format(new Date('2025-01-10'));

function getLevel(count: number, maxCount: number): number {
  if (count === 0 || maxCount === 0) return 0;
  if (count <= maxCount * 0.25) return 1;
  if (count <= maxCount * 0.5) return 2;
  if (count <= maxCount * 0.75) return 3;
  return 4;
}

function formatLocalDateKey(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}

function buildGrid(
  data: Activity[],
  startDate?: string,
  endDate?: string
): (Activity | null)[][] {
  const hasOverride = startDate || endDate;

  if (data.length === 0 && !hasOverride) {
    const today = new Date();
    const end = new Date(today);
    const start = new Date(today);
    start.setDate(start.getDate() - 364);
    return buildGridFromRange([], start, end);
  }

  const sorted = [...data].sort((a, b) => a.date.localeCompare(b.date));

  const firstStr = startDate ?? (sorted.length > 0 ? sorted[0].date : null);
  const lastStr = endDate ?? (sorted.length > 0 ? sorted[sorted.length - 1].date : null);

  const today = new Date();
  const firstDate = firstStr ? new Date(firstStr + "T00:00:00") : new Date(new Date().setDate(today.getDate() - 364));
  const lastDate = lastStr ? new Date(lastStr + "T00:00:00") : today;

  return buildGridFromRange(data, firstDate, lastDate);
}

function buildGridFromRange(
  data: Activity[],
  firstDate: Date,
  lastDate: Date
): (Activity | null)[][] {
  let rangeStart = firstDate;
  let rangeEnd = lastDate;
  if (
    !Number.isNaN(rangeStart.getTime()) &&
    !Number.isNaN(rangeEnd.getTime()) &&
    rangeStart > rangeEnd
  ) {
    const t = rangeStart;
    rangeStart = rangeEnd;
    rangeEnd = t;
  }

  // Pad left to preceding Sunday
  const start = new Date(rangeStart);
  start.setDate(start.getDate() - start.getDay());

  // Pad right to following Saturday
  const end = new Date(rangeEnd);
  end.setDate(end.getDate() + (6 - end.getDay()));

  const dataMap = new Map<string, Activity>();
  for (const day of data) {
    dataMap.set(day.date, day);
  }

  const weeks: (Activity | null)[][] = [];
  const current = new Date(start);

  while (current <= end) {
    const week: (Activity | null)[] = [];
    for (let d = 0; d < 7; d++) {
      const dateStr = formatLocalDateKey(current);
      week.push(dataMap.get(dateStr) ?? { date: dateStr, count: 0 });
      current.setDate(current.getDate() + 1);
    }
    weeks.push(week);
  }

  return weeks;
}

export function ActivityHeatmap({
  id,
  events = [],
  eventHandler,
  data = [],
  colorScheme = "primary",
  showTooltip = true,
  showMonthLabels = true,
  showDayLabels = true,
  startDate,
  endDate,
}: ActivityHeatmapProps) {
  const weeks = buildGrid(data, startDate, endDate);
  const maxCount = Math.max(0, ...data.map((d) => d.count));
  const colors = buildColorScheme(`var(--color-${colorScheme.toLowerCase()})`);
  const clickable = events.includes("OnDayClick");
  const gridContainer = useRef<HTMLDivElement>(null);

  const [tooltip, setTooltip] = useState<{ day: Activity; } | null>(null);
  const tooltipCoordinates = useRef<{ x: number; y: number } | null>(null);
  const tooltipDiv = useRef<HTMLDivElement>(null);

  // Compute month labels: for each week, check if the first non-null day is the first occurrence of a new month
  const monthLabels: string[] = weeks.map((week, wi) => {
    const firstDay = week[0];
    if (!firstDay) return "";
    const date = new Date(firstDay.date + "T00:00:00");
    if (date.getDate() <= 7) {
      // First week of the month
      const prevWeekFirstDay = wi > 0 ? weeks[wi - 1]?.[0] : null;
      if (!prevWeekFirstDay) return MONTH_NAMES[date.getMonth()] ?? "";
      const prevDate = new Date(prevWeekFirstDay.date + "T00:00:00");
      if (prevDate.getMonth() !== date.getMonth()) {
        return MONTH_NAMES[date.getMonth()] ?? "";
      }
    }
    return "";
  });

  const handleClick = (day: Activity) => {
    if (clickable) {
      eventHandler("OnDayClick", id, [day]);
    }
  };

  return (
    <div className="flex w-full relative rounded border-secondary bg-card">
      <div className=" overflow-x-auto p-0"
        style={{ direction: "rtl" }}>
        <div className="inline-flex flex-col gap-1 font-sans"
          style={{ direction: "ltr" }}>
          {showMonthLabels && (
            <div className="flex gap-0.5 text-[#57606a] w-fit">
              {showDayLabels && <div style={{ width: "28px" }} />}
              {weeks.map((_, wi) => (
                <div
                  key={wi}
                  className="text-center flex text-secondary-foreground opacity-50 last:hidden"
                  style={{ width: "11px", fontSize: "10px" }}
                >
                  {monthLabels[wi]}
                </div>
              ))}
            </div>
          )}

          <div className="flex gap-1">
            {showDayLabels && (
              <div className="flex flex-col justify-end absolute left-0 top-0 bottom-0 bg-card">
                <div
                  className="grid gap-0.5 text-secondary-foreground opacity-50 pt-0.5 *:pr-2 *:text-right"
                  style={{ gridTemplateRows: "repeat(7, 11px)", width: "28px" }}
                >
                  <div />
                  <div style={{ fontSize: "10px", lineHeight: "11px" }}>{MONDAY}</div>
                  <div />
                  <div style={{ fontSize: "10px", lineHeight: "11px" }}>{WEDNESDAY}</div>
                  <div />
                  <div style={{ fontSize: "10px", lineHeight: "11px" }}>{FRIDAY}</div>
                  <div />
                </div>
              </div>
            )}

            <div className="flex gap-0.5"
              ref={gridContainer}
              style={{ paddingLeft: showDayLabels ? 28 : 0 }}
              onMouseEnter={(e) => {
                if (!showTooltip) return;

                tooltipCoordinates.current = { x: e.clientX, y: e.clientY };
                if (tooltipDiv.current) {
                  tooltipDiv.current.style.opacity = "1";
                  tooltipDiv.current.style.visibility = "visible";
                }
              }}
              onMouseMove={(e) => {
                if (!showTooltip) return;

                tooltipCoordinates.current = { x: e.clientX, y: e.clientY };
                if (tooltipDiv.current) {
                  tooltipDiv.current.style.left = e.clientX + "px";
                  tooltipDiv.current.style.top = e.clientY + "px";
                  tooltipDiv.current.style.transform = getTooltipTransform(tooltipDiv.current, tooltipCoordinates.current, gridContainer.current);
                }
              }}
              onMouseLeave={() => {
                if (!showTooltip) return;

                tooltipCoordinates.current = null;
                if (tooltipDiv.current) {
                  tooltipDiv.current.style.opacity = "0";
                  tooltipDiv.current.style.visibility = "hidden";
                }
                setTooltip(null);
              }}
            >
              {weeks.map((week, wi) => (
                <div
                  key={wi}
                  className="grid gap-0.5"
                  style={{ gridTemplateRows: "repeat(7, 11px)" }}
                >
                  {week.map((day, di) => {
                    const level = day ? getLevel(day.count, maxCount) : 0;
                    const bg = colors[level] ?? colors[0]!;
                    return (
                      <div
                        key={di}
                        className={`w-[11px] h-[11px] rounded-sm hover:rounded-none hover:scale-110 transition-all duration-300 ease-in-out ${clickable && day?.count ? "cursor-pointer" : "cursor-default"}`}
                        style={{ backgroundColor: bg }}
                        onMouseEnter={() => {
                          if (showTooltip && day) {
                            setTooltip({ day });
                          }
                        }}
                        onClick={day ? () => handleClick(day) : undefined}
                      />
                    );
                  })}
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>

      {showTooltip && <div
        ref={tooltipDiv}
        className="fixed opacity-0 visibility-hidden z-50 bg-card text-xs text-foreground pointer-events-none rounded-[4px] px-2 py-3"
        style={{
          minWidth: "180px",
          boxShadow: "0 4px 6px -1px rgba(0,0,0,.1), 0 2px 4px -2px rgba(0,0,0,.1)",
          transition: "opacity 0.3s ease-in-out, visibility 0.3s ease-in-out, transform 0.3s ease-in-out",
        }}
      >
        {tooltip &&
          <>
            <div className="font-bold">{formatTooltipHeader(tooltip.day)}</div>
            <div className="flex gap-2 align-middle">
              <div className="w-[11px] h-[11px] my-auto rounded-full" style={{ backgroundColor: colors[getLevel(tooltip.day.count, maxCount)] ?? colors[0]! }}></div>
              <div>{formatTooltipValue(tooltip.day)}</div>
            </div>
          </>}
      </div>}
    </div>
  );
}

function getTooltipTransform(tooltipDiv: HTMLDivElement | null, tooltipCoordinates: { x: number; y: number }, gridContainer: HTMLDivElement | null): string {
  if (!tooltipDiv || !gridContainer) return "translate(0px, 0px)";
  const gridRect = gridContainer.getBoundingClientRect();
  const xOffset = tooltipCoordinates.x > gridRect.left + gridRect.width / 2 ? -200 : 12;
  const yOffset = tooltipCoordinates.y > window.innerHeight / 2 ? -80 : 12;
  return `translate(${xOffset}px, ${yOffset}px)`;
}