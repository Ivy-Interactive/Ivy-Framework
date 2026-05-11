import "./style.css";
import { tokens } from "@ivy-interactive/ivy-design-system";
import { ActivityHeatmapProps, ContributionDay } from "./types";

function buildColorScheme(baseToken: string): string[] {
  return [
    tokens["color-border"],
    `color-mix(in srgb, ${baseToken} 25%, transparent)`,
    `color-mix(in srgb, ${baseToken} 50%, transparent)`,
    `color-mix(in srgb, ${baseToken} 75%, transparent)`,
    baseToken,
  ];
}

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

function formatLocalDateKey(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}

function buildGrid(
  data: ContributionDay[],
  startDate?: string,
  endDate?: string
): (ContributionDay | null)[][] {
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
  const lastDate = lastStr ? new Date(lastStr + "T00:00:00") : new Date();

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
      const dateStr = formatLocalDateKey(current);
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
  colorScheme = "primary",
  showTooltip = true,
  showMonthLabels = true,
  showDayLabels = true,
  startDate,
  endDate,
}: ActivityHeatmapProps) {
  const weeks = buildGrid(data, startDate, endDate);
  const maxCount = Math.max(0, ...data.map((d) => d.count));
  const colorTokenName = colorScheme.split(/(?=[A-Z])/).join("-").toLowerCase();
  // ToDo: check why `ColorInputVariant.Swatch` renders `secondary` and `muted` as `var(--border)` in frontend
  const colorToken = `color-${["secondary", "muted"].includes(colorTokenName) ? "border" : colorTokenName}` as keyof typeof tokens;
  const colors = buildColorScheme(tokens[colorToken]);
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
    <div className="flex w-full relative bg-background rounded border-secondary">
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
              <div className="flex flex-col justify-end absolute left-0 top-0 bottom-0 bg-background">
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

            <div className="flex gap-0.5 "
              style={{ paddingLeft: showDayLabels ? 28 : 0 }}>
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
      </div>
    </div>
  );
}
