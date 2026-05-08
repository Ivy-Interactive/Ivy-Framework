import "./style.css";
import { ActivityHeatmapProps, ContributionDay } from "./types";

const COLOR_SCHEMES: Record<string, string[]> = {
  green: ["#ebedf0", "#9be9a8", "#40c463", "#30a14e", "#216e39"],
  blue: ["#ebedf0", "#b6d6f7", "#64a4e4", "#2c7fcc", "#1553a0"],
  purple: ["#ebedf0", "#d8b4fe", "#a855f7", "#7c3aed", "#4c1d95"],
  orange: ["#ebedf0", "#fed7aa", "#fb923c", "#ea580c", "#9a3412"],
  pink: ["#ebedf0", "#fbcfe8", "#f472b6", "#db2777", "#9d174d"],
};

const preferredLanguage = navigator.languages.length ? navigator.languages : navigator.language;
const monthFormatter = new Intl.DateTimeFormat(preferredLanguage, { month: "short" });
const weekdayFormatter = new Intl.DateTimeFormat(preferredLanguage, { weekday: "short" });
const MONTH_NAMES = Array.from({ length: 12 }, (_, i) => monthFormatter.format(new Date(0, i)));

const mondayDate = new Date('2025-01-06');
const MONDAY = weekdayFormatter.format(mondayDate);
const WEDNESDAY = weekdayFormatter.format(mondayDate.setDate(mondayDate.getDate() + 2));
const FRIDAY = weekdayFormatter.format(mondayDate.setDate(mondayDate.getDate() + 2));

function getLevel(count: number, maxCount: number): number {
  if (count === 0 || maxCount === 0) return 0;
  if (count <= maxCount * 0.25) return 1;
  if (count <= maxCount * 0.5) return 2;
  if (count <= maxCount * 0.75) return 3;
  return 4;
}

function buildGrid(data: ContributionDay[]): (ContributionDay | null)[][] {
  if (data.length === 0) {
    // Default to last 52 weeks ending today
    const today = new Date();
    const end = new Date(today);
    const start = new Date(today);
    start.setDate(start.getDate() - 364);
    return buildGridFromRange([], start, end);
  }

  const sorted = [...data].sort((a, b) => a.date.localeCompare(b.date));
  const firstDate = new Date(sorted[0].date + "T00:00:00");
  const lastDate = new Date(sorted[sorted.length - 1].date + "T00:00:00");

  return buildGridFromRange(data, firstDate, lastDate);
}

function buildGridFromRange(
  data: ContributionDay[],
  firstDate: Date,
  lastDate: Date
): (ContributionDay | null)[][] {
  // Pad left to preceding Sunday
  const start = new Date(firstDate);
  start.setDate(start.getDate() - start.getDay());

  // Pad right to following Saturday
  const end = new Date(lastDate);
  end.setDate(end.getDate() + (6 - end.getDay()));

  const dataMap = new Map<string, ContributionDay>();
  for (const day of data) {
    dataMap.set(day.date, day);
  }

  const weeks: (ContributionDay | null)[][] = [];
  const current = new Date(start);

  while (current <= end) {
    const week: (ContributionDay | null)[] = [];
    for (let d = 0; d < 7; d++) {
      const dateStr = current.toISOString().slice(0, 10);
      week.push(dataMap.get(dateStr) ?? { date: dateStr, count: 0 });
      current.setDate(current.getDate() + 1);
    }
    weeks.push(week);
  }

  return weeks;
}

function formatTooltip(day: ContributionDay): string {
  const date = new Date(day.date + "T00:00:00");
  const month = MONTH_NAMES[date.getMonth()];
  const dayNum = date.getDate();
  const year = date.getFullYear();
  const label = day.count === 1 ? "contribution" : "contributions";
  return `${month} ${dayNum}, ${year} — ${day.count} ${label}`;
}

export function ActivityHeatmap({
  id,
  events = [],
  eventHandler,
  data = [],
  colorScheme = "green",
  showTooltip = true,
  showMonthLabels = true,
  showDayLabels = true,
}: ActivityHeatmapProps) {
  const weeks = buildGrid(data);
  const maxCount = Math.max(0, ...data.map((d) => d.count));
  const colors = COLOR_SCHEMES[colorScheme] ?? COLOR_SCHEMES["green"]!;
  const clickable = events.includes("OnDayClick");

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

  const handleClick = (day: ContributionDay) => {
    if (clickable) {
      eventHandler("OnDayClick", id, [day]);
    }
  };

  return (
    <div className="inline-flex flex-col gap-1 font-sans text-xs">
      {showMonthLabels && (
        <div className="flex gap-0.5 text-[#57606a]">
          {showDayLabels && <div style={{ width: "28px" }} />}
          {weeks.map((_, wi) => (
            <div
              key={wi}
              className="text-center"
              style={{ width: "11px", fontSize: "10px" }}
            >
              {monthLabels[wi]}
            </div>
          ))}
        </div>
      )}

      <div className="flex gap-1">
        {showDayLabels && (
          <div
            className="grid gap-0.5 text-[#57606a] pt-0.5"
            style={{ gridTemplateRows: "repeat(7, 11px)", width: "24px" }}
          >
            <div />
            <div style={{ fontSize: "9px", lineHeight: "11px" }}>{MONDAY}</div>
            <div />
            <div style={{ fontSize: "9px", lineHeight: "11px" }}>{WEDNESDAY}</div>
            <div />
            <div style={{ fontSize: "9px", lineHeight: "11px" }}>{FRIDAY}</div>
            <div />
          </div>
        )}

        <div className="flex gap-0.5">
          {weeks.map((week, wi) => (
            <div
              key={wi}
              className="grid gap-0.5"
              style={{ gridTemplateRows: "repeat(7, 11px)" }}
            >
              {week.map((day, di) => {
                const level = day ? getLevel(day.count, maxCount) : 0;
                const bg = colors[level] ?? colors[0]!;
                const title =
                  showTooltip && day ? formatTooltip(day) : undefined;
                return (
                  <div
                    key={di}
                    className={`w-[11px] h-[11px] rounded-sm ${clickable && day?.count ? "cursor-pointer" : "cursor-default"}`}
                    style={{ backgroundColor: bg }}
                    title={title}
                    onClick={day ? () => handleClick(day) : undefined}
                  />
                );
              })}
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
