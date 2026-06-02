import { useEffect, useRef, useState } from "react";
import "./style.css";
import { ActivityHeatmapProps, Activity } from "./types";
import { computeMaxCount, getLevel } from "./levelUtils";
import { MONTH_NAMES, formatTooltipHeader, getTooltipTransform } from "./tooltipUtils";

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

// Hourly "punchcard" grid: one column per day, 24 rows (hours 0-23).
function buildHourlyGrid(
  data: Activity[],
  startDate?: string,
  endDate?: string
): (Activity | null)[][] {
  const sorted = [...data].sort((a, b) => a.date.localeCompare(b.date));
  const today = new Date();

  const firstStr = startDate ?? (sorted.length > 0 ? sorted[0].date : null);
  const lastStr = endDate ?? (sorted.length > 0 ? sorted[sorted.length - 1].date : null);

  // Default window for hourly is the last 7 days (24 * 365 would be unusable).
  let rangeStart = firstStr ? new Date(firstStr + "T00:00:00") : new Date(new Date().setDate(today.getDate() - 6));
  let rangeEnd = lastStr ? new Date(lastStr + "T00:00:00") : today;

  if (
    !Number.isNaN(rangeStart.getTime()) &&
    !Number.isNaN(rangeEnd.getTime()) &&
    rangeStart > rangeEnd
  ) {
    const t = rangeStart;
    rangeStart = rangeEnd;
    rangeEnd = t;
  }

  const dataMap = new Map<string, Activity>();
  for (const cell of data) {
    if (cell.hour != null && cell.hour >= 0 && cell.hour <= 23) {
      dataMap.set(`${cell.date}T${cell.hour}`, cell);
    }
  }

  const columns: (Activity | null)[][] = [];
  const current = new Date(rangeStart);
  current.setHours(0, 0, 0, 0);

  while (current <= rangeEnd) {
    const dateStr = formatLocalDateKey(current);
    const column: (Activity | null)[] = [];
    for (let h = 0; h < 24; h++) {
      column.push(dataMap.get(`${dateStr}T${h}`) ?? { date: dateStr, hour: h, count: 0 });
    }
    columns.push(column);
    current.setDate(current.getDate() + 1);
  }

  return columns;
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
  interval = "Daily",
  valueLabel,
  startDate,
  endDate,
}: ActivityHeatmapProps) {
  const isHourly = interval === "Hourly";
  const rowCount = isHourly ? 24 : 7;
  const labelWidth = isHourly ? 34 : 28;

  const columns = isHourly
    ? buildHourlyGrid(data, startDate, endDate)
    : buildGrid(data, startDate, endDate);
  const maxCount = computeMaxCount(data);
  const colors = buildColorScheme(`var(--color-${colorScheme.toLowerCase()})`);
  const clickable = events.includes("OnDayClick");
  const gridContainer = useRef<HTMLDivElement>(null);
  const scrollXContainer = useRef<HTMLDivElement>(null);

  const [tooltip, setTooltip] = useState<{ day: Activity; } | null>(null);
  const tooltipCoordinates = useRef<{ x: number; y: number } | null>(null);
  const tooltipDiv = useRef<HTMLDivElement>(null);

  // Row (y-axis) labels: weekdays for daily, hours for hourly.
  const rowLabels: string[] = isHourly
    ? Array.from({ length: 24 }, (_, h) => (h % 6 === 0 ? `${String(h).padStart(2, "0")}:00` : ""))
    : ["", MONDAY, "", WEDNESDAY, "", FRIDAY, ""];

  // Column (x-axis) labels: month name on month boundaries (daily) or a short date
  // on the first column / month boundaries (hourly).
  const columnLabels: string[] = columns.map((column, ci) => {
    const firstCell = column[0];
    if (!firstCell) return "";
    const date = new Date(firstCell.date + "T00:00:00");

    if (isHourly) {
      if (ci === 0 || date.getDate() === 1) {
        return `${MONTH_NAMES[date.getMonth()]} ${date.getDate()}`;
      }
      return "";
    }

    if (date.getDate() <= 7) {
      // First week of the month
      const prevColumnFirst = ci > 0 ? columns[ci - 1]?.[0] : null;
      if (!prevColumnFirst) return MONTH_NAMES[date.getMonth()] ?? "";
      const prevDate = new Date(prevColumnFirst.date + "T00:00:00");
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

  useEffect(() => {
    if (scrollXContainer.current) {
      scrollXContainer.current.scrollLeft = scrollXContainer.current!.scrollWidth;
    }
  }, [])

  return (
    <div className="ivy-activity-heatmap-container">
      <div className="overflow-x-auto h-100 bg-card ivy-activity-heatmap-scroll"
        ref={scrollXContainer}>
        <div className="inline-flex flex-col gap-1 font-sans h-100">
          {showMonthLabels && (
            <div className="sticky top-0 flex gap-0.5 text-[#57606a] w-full bg-card">
              {showDayLabels && <div style={{ width: `${labelWidth}px` }} />}
              {columns.map((_, ci) => (
                <div
                  key={ci}
                  className="text-center flex text-secondary-foreground opacity-50 last:hidden"
                  style={{ width: "11px", fontSize: "10px" }}
                >
                  {columnLabels[ci]}
                </div>
              ))}
            </div>
          )}

          <div className="flex gap-1">
            {showDayLabels && (
              <div className="flex flex-col justify-end sticky left-0 top-0 bottom-0 bg-card">
                <div
                  className="grid gap-0.5 text-secondary-foreground opacity-50 *:pr-2 *:text-right"
                  style={{ gridTemplateRows: `repeat(${rowCount}, 11px)`, width: `${labelWidth}px` }}
                >
                  {rowLabels.map((label, ri) => (
                    <div key={ri} style={{ fontSize: "10px", lineHeight: "11px" }}>
                      {label}
                    </div>
                  ))}
                </div>
              </div>
            )}

            <div className="flex gap-0.5"
              ref={gridContainer}
              onMouseEnter={(e) => {
                if (!showTooltip) return;

                tooltipCoordinates.current = { x: e.clientX, y: e.clientY };
                if (tooltipDiv.current) {
                  tooltipDiv.current.style.left = e.clientX + "px";
                  tooltipDiv.current.style.top = e.clientY + "px";
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
              {columns.map((column, ci) => (
                <div
                  key={ci}
                  className="grid gap-0.5"
                  style={{ gridTemplateRows: `repeat(${rowCount}, 11px)` }}
                >
                  {column.map((day, di) => {
                    const level = day ? getLevel(day.count, maxCount) : 0;
                    const bg = colors[level] ?? colors[0]!;
                    return (
                      <div
                        key={day ? `${day.date}T${day.hour ?? "d"}` : `${di}-${ci}`}
                        className={`w-[11px] h-[11px] rounded-sm hover:rounded-none hover:scale-110 ${clickable && day?.count ? "cursor-pointer" : "cursor-default"}`}
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
        className="fixed opacity-0 z-50 bg-card text-xs text-foreground pointer-events-none rounded-[4px] px-2 py-3 min-w-40 max-w-64"
        style={{
          boxShadow: "0 4px 6px -1px rgba(0,0,0,.1), 0 2px 4px -2px rgba(0,0,0,.1)",
          transition: "opacity 0.3s ease-in-out, visibility 0.3s ease-in-out, transform 0.3s ease-in-out",
        }}
      >
        {tooltip &&
          <>
            <div className="font-bold">{formatTooltipHeader(tooltip.day)}</div>
            <p>
              <span className="relative w-[11px] h-[11px] pr-4 inline-flex">
                <span className="absolute top-[2px] left-0 w-[11px] h-[11px] rounded-full self-center"
                  style={{ backgroundColor: colors[getLevel(tooltip.day.count, maxCount)] ?? colors[0]! }}>
                  {tooltip.day.count ? (
                    <span
                      className="absolute top-[2px] left-0 w-[11px] h-[11px] rounded-full self-center text-center"
                      style={{
                        backgroundColor: colors[getLevel(tooltip.day.count, maxCount)] ?? colors[0]!,
                        color: "white"
                      }}>
                      {tooltip.day.count}
                    </span>
                  ) : null}
                </span>
              </span>
              <span>{`${valueLabel ?? "Count"}: `}</span>
              <span className="font-bold">{`${tooltip.day.count ?? 0}`}</span>
            </p>
          </>}
      </div>}
    </div>
  );
}
