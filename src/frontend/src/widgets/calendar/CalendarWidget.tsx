import React, { useState, useMemo, useCallback } from "react";
import {
  format,
  startOfMonth,
  endOfMonth,
  startOfWeek,
  endOfWeek,
  addDays,
  addMonths,
  addWeeks,
  subMonths,
  subWeeks,
  subDays,
  isSameMonth,
  isToday,
  differenceInMinutes,
  startOfDay,
  endOfDay,
  isWithinInterval,
  max,
  min,
} from "date-fns";
import { getWidth, getHeight } from "@/lib/styles";
import { useCalendarData } from "./useCalendarData";
import { useCalendarHandlers } from "./useCalendarHandlers";
import { useRovingTabIndex } from "./useRovingTabIndex";
import type { CalendarWidgetProps, CalendarView, CalendarEvent } from "./types";
import { Densities } from "@/types/density";
import { CALENDAR_DENSITY_CONFIG } from "./constants";
import { cn } from "@/lib/utils";
import { Densities } from "@/types/density";

const HOURS = Array.from({ length: 24 }, (_, i) => i);

// ─── Toolbar ───────────────────────────────────────────────────────────────────

interface ToolbarProps {
  currentDate: Date;
  view: CalendarView;
  onNavigate: (action: "prev" | "next" | "today") => void;
  onViewChange: (view: CalendarView) => void;
  dc: (typeof CALENDAR_DENSITY_CONFIG)[Densities];
}

const Toolbar: React.FC<ToolbarProps> = ({ currentDate, view, onNavigate, onViewChange, dc }) => {
  const label = useMemo(() => {
    switch (view) {
      case "month":
        return format(currentDate, "MMMM yyyy");
      case "week": {
        const weekStart = startOfWeek(currentDate, { weekStartsOn: 1 });
        const weekEnd = endOfWeek(currentDate, { weekStartsOn: 1 });
        if (weekStart.getMonth() === weekEnd.getMonth()) {
          return `${format(weekStart, "MMM d")} - ${format(weekEnd, "d, yyyy")}`;
        }
        return `${format(weekStart, "MMM d")} - ${format(weekEnd, "MMM d, yyyy")}`;
      }
      case "day":
        return format(currentDate, "EEEE, MMMM d, yyyy");
      case "agenda":
        return format(currentDate, "MMMM yyyy");
      default:
        return "";
    }
  }, [currentDate, view]);

  const views: { key: CalendarView; label: string }[] = [
    { key: "month", label: "Month" },
    { key: "week", label: "Week" },
    { key: "day", label: "Day" },
    { key: "agenda", label: "Agenda" },
  ];

  return (
    <div className="flex items-center justify-between px-2 py-2 border-b border-border">
      <div className="flex items-center gap-1">
        <button
          onClick={() => onNavigate("today")}
          className={cn(
            dc.toolbarButtonPadding,
            dc.toolbarButtonText,
            "font-medium rounded-md border border-border bg-background hover:bg-accent transition-colors",
          )}
        >
          Today
        </button>
        <button
          onClick={() => onNavigate("prev")}
          className="p-1.5 rounded-md hover:bg-accent transition-colors"
          aria-label="Previous"
        >
          <ChevronLeft />
        </button>
        <button
          onClick={() => onNavigate("next")}
          className="p-1.5 rounded-md hover:bg-accent transition-colors"
          aria-label="Next"
        >
          <ChevronRight />
        </button>
        <span className={cn("ml-2", dc.toolbarTitleText)}>{label}</span>
      </div>
      <div className="flex items-center rounded-md border border-border overflow-hidden">
        {views.map((v) => (
          <button
            key={v.key}
            onClick={() => onViewChange(v.key)}
            className={cn(
              dc.toolbarButtonPadding,
              dc.toolbarButtonText,
              "font-medium transition-colors",
              view === v.key
                ? "bg-primary text-primary-foreground"
                : "bg-background hover:bg-accent",
            )}
          >
            {v.label}
          </button>
        ))}
      </div>
    </div>
  );
};

// ─── Icons ─────────────────────────────────────────────────────────────────────

const ChevronLeft = () => (
  <svg
    width="16"
    height="16"
    viewBox="0 0 16 16"
    fill="none"
    stroke="currentColor"
    strokeWidth="2"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <path d="M10 4l-4 4 4 4" />
  </svg>
);

const ChevronRight = () => (
  <svg
    width="16"
    height="16"
    viewBox="0 0 16 16"
    fill="none"
    stroke="currentColor"
    strokeWidth="2"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <path d="M6 4l4 4-4 4" />
  </svg>
);

// ─── Event Pill ────────────────────────────────────────────────────────────────

interface EventPillProps {
  event: CalendarEvent;
  onClick?: (eventId: string) => void;
  compact?: boolean;
  draggable?: boolean;
  onDragStart?: () => void;
  tabIndex?: number;
  onKeyDown?: React.KeyboardEventHandler;
  dc: (typeof CALENDAR_DENSITY_CONFIG)[Densities];
}

const EventPill = React.forwardRef<HTMLButtonElement, EventPillProps>(
  ({ event, onClick, compact, draggable, onDragStart, tabIndex, onKeyDown, dc }, ref) => {
    const bgColor = event.color
      ? `var(--${event.color.toLowerCase()}, var(--primary))`
      : "var(--primary)";
    const fgColor = "var(--primary-foreground)";

    return (
      <button
        ref={ref}
        onClick={(e) => {
          e.stopPropagation();
          onClick?.(event.id);
        }}
        draggable={draggable}
        onDragStart={
          draggable
            ? (e) => {
                e.dataTransfer.setData("text/plain", event.id);
                e.dataTransfer.effectAllowed = "move";
                onDragStart?.();
              }
            : undefined
        }
        className={cn(
          "w-full text-left rounded truncate cursor-pointer transition-opacity hover:opacity-80",
          dc.eventPillPadding,
          compact ? cn(dc.eventPillText, "leading-snug") : dc.eventPillText,
          draggable && "cursor-grab active:cursor-grabbing",
        )}
        style={{
          backgroundColor: bgColor,
          color: fgColor,
        }}
        title={event.title}
        tabIndex={tabIndex}
        onKeyDown={onKeyDown}
      >
        {event.content ? (
          <div className="w-full">{event.content}</div>
        ) : (
          <>
            {!compact && !event.allDay && (
              <span className="mr-1 opacity-80">{format(event.start, "HH:mm")}</span>
            )}
            {event.title}
          </>
        )}
      </button>
    );
  },
);
EventPill.displayName = "EventPill";

// ─── Month Day Cell (extracted for per-cell hook usage) ───────────────────────

interface MonthDayCellProps {
  day: Date;
  dayEvents: CalendarEvent[];
  inMonth: boolean;
  today: boolean;
  maxVisible: number;
  onSelectSlot: (start: string, end: string) => void;
  onEventClick: (eventId: string) => void;
  enableDragDrop?: boolean;
  events: CalendarEvent[];
  onEventMove?: (eventId: string, newStart: string, newEnd: string) => void;
  draggedEventId: string | null;
  setDraggedEventId: (id: string | null) => void;
  dc: (typeof CALENDAR_DENSITY_CONFIG)[Densities];
}

const MonthDayCell: React.FC<MonthDayCellProps> = ({
  day,
  dayEvents,
  inMonth,
  today,
  maxVisible,
  onSelectSlot,
  onEventClick,
  enableDragDrop,
  events,
  onEventMove,
  draggedEventId,
  setDraggedEventId,
  dc,
}) => {
  const visibleEvents = dayEvents.slice(0, maxVisible);
  const { setItemRef, getTabIndex, onKeyDown } = useRovingTabIndex(visibleEvents.length);

  return (
    <div
      role="gridcell"
      tabIndex={0}
      aria-label={format(day, "EEEE, MMMM d, yyyy")}
      className={cn(
        "border-r border-border last:border-r-0 p-1 overflow-hidden cursor-pointer hover:bg-accent/30 transition-colors",
        dc.monthCellMinHeight,
        !inMonth && "bg-muted/30",
        draggedEventId && "hover:bg-primary/10",
      )}
      onClick={() => {
        const dayStart = startOfDay(day);
        onSelectSlot(dayStart.toISOString(), endOfDay(day).toISOString());
      }}
      onKeyDown={(e) => {
        if (e.key === "Enter" || e.key === " ") {
          e.preventDefault();
          const dayStart = startOfDay(day);
          onSelectSlot(dayStart.toISOString(), endOfDay(day).toISOString());
        }
      }}
      onDragOver={
        enableDragDrop
          ? (e) => {
              e.preventDefault();
              e.currentTarget.classList.add("bg-primary/10");
            }
          : undefined
      }
      onDragLeave={
        enableDragDrop
          ? (e) => {
              e.currentTarget.classList.remove("bg-primary/10");
            }
          : undefined
      }
      onDrop={
        enableDragDrop
          ? (e) => {
              e.preventDefault();
              e.currentTarget.classList.remove("bg-primary/10");
              const eventId = e.dataTransfer.getData("text/plain");
              if (eventId && onEventMove) {
                const droppedEvent = events.find((ev) => ev.id === eventId);
                if (droppedEvent) {
                  const duration = droppedEvent.end.getTime() - droppedEvent.start.getTime();
                  const newStart = new Date(day);
                  newStart.setHours(droppedEvent.start.getHours(), droppedEvent.start.getMinutes());
                  const newEnd = new Date(newStart.getTime() + duration);
                  onEventMove(eventId, newStart.toISOString(), newEnd.toISOString());
                }
              }
              setDraggedEventId(null);
            }
          : undefined
      }
    >
      <div className="flex justify-end mb-0.5">
        <span
          className={cn(
            dc.dayBadgeSize,
            "flex items-center justify-center rounded-full",
            today && "bg-primary text-primary-foreground font-bold",
            !inMonth && !today && "text-muted-foreground/50",
          )}
        >
          {format(day, "d")}
        </span>
      </div>
      <div className="flex flex-col gap-0.5">
        {visibleEvents.map((event, idx) => (
          <EventPill
            key={event.id}
            ref={setItemRef(idx)}
            event={event}
            onClick={onEventClick}
            compact
            draggable={enableDragDrop}
            onDragStart={enableDragDrop ? () => setDraggedEventId(event.id) : undefined}
            tabIndex={getTabIndex(idx)}
            onKeyDown={(e) => onKeyDown(e, idx)}
            dc={dc}
          />
        ))}
        {dayEvents.length > maxVisible && (
          <span className="text-[10px] text-muted-foreground pl-1">
            +{dayEvents.length - maxVisible} more
          </span>
        )}
      </div>
    </div>
  );
};

// ─── Month View ────────────────────────────────────────────────────────────────

interface MonthViewProps {
  currentDate: Date;
  events: CalendarEvent[];
  onEventClick: (eventId: string) => void;
  onSelectSlot: (start: string, end: string) => void;
  onEventMove?: (eventId: string, newStart: string, newEnd: string) => void;
  enableDragDrop?: boolean;
  dc: (typeof CALENDAR_DENSITY_CONFIG)[Densities];
}

const MonthView: React.FC<MonthViewProps> = ({
  currentDate,
  events,
  onEventClick,
  onSelectSlot,
  onEventMove,
  enableDragDrop,
  dc,
}) => {
  const [draggedEventId, setDraggedEventId] = useState<string | null>(null);
  const weeks = useMemo(() => {
    const monthStart = startOfMonth(currentDate);
    const monthEnd = endOfMonth(currentDate);
    const calStart = startOfWeek(monthStart, { weekStartsOn: 1 });
    const calEnd = endOfWeek(monthEnd, { weekStartsOn: 1 });

    const weeks: Date[][] = [];
    let day = calStart;
    while (day <= calEnd) {
      const week: Date[] = [];
      for (let i = 0; i < 7; i++) {
        week.push(day);
        day = addDays(day, 1);
      }
      weeks.push(week);
    }
    return weeks;
  }, [currentDate]);

  const eventsByDay = useMemo(() => {
    const map = new Map<string, CalendarEvent[]>();
    events.forEach((event) => {
      let day = startOfDay(event.start);
      const eventEnd = startOfDay(event.end);
      while (day <= eventEnd) {
        const key = format(day, "yyyy-MM-dd");
        if (!map.has(key)) map.set(key, []);
        map.get(key)!.push(event);
        day = addDays(day, 1);
      }
    });
    return map;
  }, [events]);

  const dayNames = useMemo(() => {
    const start = startOfWeek(new Date(), { weekStartsOn: 1 });
    return Array.from({ length: 7 }, (_, i) => format(addDays(start, i), "EEE"));
  }, []);

  return (
    <div className="flex flex-col flex-1 overflow-hidden">
      {/* Day headers */}
      <div className="grid grid-cols-7 border-b border-border">
        {dayNames.map((name) => (
          <div key={name} className="py-1.5 text-center text-xs font-medium text-muted-foreground">
            {name}
          </div>
        ))}
      </div>

      {/* Weeks */}
      <div
        className="grid flex-1"
        style={{ gridTemplateRows: `repeat(${weeks.length}, 1fr)` }}
        role="grid"
      >
        {weeks.map((week, wi) => (
          <div
            key={wi}
            className="grid grid-cols-7 border-b border-border last:border-b-0"
            role="row"
          >
            {week.map((day) => {
              const key = format(day, "yyyy-MM-dd");
              const dayEvents = eventsByDay.get(key) || [];
              const inMonth = isSameMonth(day, currentDate);
              const today = isToday(day);
              const MAX_VISIBLE = 3;

              return (
                <MonthDayCell
                  key={key}
                  day={day}
                  dayEvents={dayEvents}
                  inMonth={inMonth}
                  today={today}
                  maxVisible={MAX_VISIBLE}
                  onSelectSlot={onSelectSlot}
                  onEventClick={onEventClick}
                  enableDragDrop={enableDragDrop}
                  events={events}
                  onEventMove={onEventMove}
                  draggedEventId={draggedEventId}
                  setDraggedEventId={setDraggedEventId}
                  dc={dc}
                />
              );
            })}
          </div>
        ))}
      </div>
    </div>
  );
};

// ─── All-Day Cell (extracted for per-cell hook usage) ─────────────────────────

interface AllDayCellProps {
  day: Date;
  allDayEvents: CalendarEvent[];
  onEventClick: (eventId: string) => void;
  dc: (typeof CALENDAR_DENSITY_CONFIG)[Densities];
}

const AllDayCell: React.FC<AllDayCellProps> = ({ day, allDayEvents, onEventClick, dc }) => {
  const { setItemRef, getTabIndex, onKeyDown } = useRovingTabIndex(allDayEvents.length);

  return (
    <div
      key={day.toISOString()}
      className="flex-1 border-l border-border p-0.5 flex flex-col gap-0.5"
    >
      {allDayEvents.map((event, idx) => (
        <EventPill
          key={event.id}
          ref={setItemRef(idx)}
          event={event}
          onClick={onEventClick}
          compact
          tabIndex={getTabIndex(idx)}
          onKeyDown={(e) => onKeyDown(e, idx)}
          dc={dc}
        />
      ))}
    </div>
  );
};

// ─── TimeGrid Day Column (extracted for per-column hook usage) ────────────────

interface TimeGridDayColumnProps {
  day: Date;
  dayEvents: CalendarEvent[];
  events: CalendarEvent[];
  onEventClick: (eventId: string) => void;
  onSelectSlot: (start: string, end: string) => void;
  onEventMove?: (eventId: string, newStart: string, newEnd: string) => void;
  enableDragDrop?: boolean;
  dc: (typeof CALENDAR_DENSITY_CONFIG)[Densities];
}

const TimeGridDayColumn: React.FC<TimeGridDayColumnProps> = ({
  day,
  dayEvents,
  events,
  onEventClick,
  onSelectSlot,
  onEventMove,
  enableDragDrop,
  dc,
}) => {
  const dayStart = startOfDay(day);
  const { setItemRef, getTabIndex, onKeyDown } = useRovingTabIndex(dayEvents.length);

  return (
    <div key={day.toISOString()} className="flex-1 border-l border-border relative">
      {/* Hour grid lines */}
      {HOURS.map((hour) => (
        <div
          key={hour}
          role="button"
          tabIndex={0}
          aria-label={`${format(day, "EEEE, MMMM d")} at ${format(new Date(2000, 0, 1, hour), "HH:mm")}`}
          className="absolute w-full border-t border-border/50 cursor-pointer hover:bg-accent/20"
          style={{
            top: hour * dc.hourHeight,
            height: dc.hourHeight,
          }}
          onClick={() => {
            const slotStart = new Date(day);
            slotStart.setHours(hour, 0, 0, 0);
            const slotEnd = new Date(day);
            slotEnd.setHours(hour + 1, 0, 0, 0);
            onSelectSlot(slotStart.toISOString(), slotEnd.toISOString());
          }}
          onKeyDown={(e) => {
            if (e.key === "Enter" || e.key === " ") {
              e.preventDefault();
              const slotStart = new Date(day);
              slotStart.setHours(hour, 0, 0, 0);
              const slotEnd = new Date(day);
              slotEnd.setHours(hour + 1, 0, 0, 0);
              onSelectSlot(slotStart.toISOString(), slotEnd.toISOString());
            }
          }}
          onDragOver={
            enableDragDrop
              ? (e) => {
                  e.preventDefault();
                  e.currentTarget.classList.add("bg-primary/10");
                }
              : undefined
          }
          onDragLeave={
            enableDragDrop
              ? (e) => {
                  e.currentTarget.classList.remove("bg-primary/10");
                }
              : undefined
          }
          onDrop={
            enableDragDrop
              ? (e) => {
                  e.preventDefault();
                  e.currentTarget.classList.remove("bg-primary/10");
                  const eventId = e.dataTransfer.getData("text/plain");
                  if (eventId && onEventMove) {
                    const droppedEvent = events.find((ev) => ev.id === eventId);
                    if (droppedEvent) {
                      const duration = droppedEvent.end.getTime() - droppedEvent.start.getTime();
                      const newStart = new Date(day);
                      newStart.setHours(hour, 0, 0, 0);
                      const newEnd = new Date(newStart.getTime() + duration);
                      onEventMove(eventId, newStart.toISOString(), newEnd.toISOString());
                    }
                  }
                }
              : undefined
          }
        />
      ))}

      {/* Events */}
      {dayEvents.map((event, idx) => {
        const evtStart = max([event.start, dayStart]);
        const evtEnd = min([event.end, endOfDay(day)]);
        const startMin = differenceInMinutes(evtStart, dayStart);
        const durationMin = Math.max(differenceInMinutes(evtEnd, evtStart), 30);
        const topPx = (startMin / 60) * dc.hourHeight;
        const heightPx = (durationMin / 60) * dc.hourHeight;

        const bgColor = event.color
          ? `var(--${event.color.toLowerCase()}, var(--primary))`
          : "var(--primary)";
        const fgColor = "var(--primary-foreground)";

        return (
          <button
            key={event.id}
            ref={setItemRef(idx)}
            draggable={enableDragDrop}
            onDragStart={
              enableDragDrop
                ? (e) => {
                    e.dataTransfer.setData("text/plain", event.id);
                    e.dataTransfer.effectAllowed = "move";
                  }
                : undefined
            }
            className={cn(
              "absolute left-0.5 right-1 rounded overflow-hidden cursor-pointer hover:opacity-80 transition-opacity text-left",
              dc.timeEventPadding,
              enableDragDrop && "cursor-grab active:cursor-grabbing",
            )}
            style={{
              top: topPx,
              height: heightPx,
              backgroundColor: bgColor,
              color: fgColor,
              zIndex: 1,
            }}
            onClick={(e) => {
              e.stopPropagation();
              onEventClick(event.id);
            }}
            title={event.title}
            tabIndex={getTabIndex(idx)}
            onKeyDown={(e) => onKeyDown(e, idx)}
          >
            {event.content ? (
              <div className="w-full h-full overflow-hidden">{event.content}</div>
            ) : (
              <>
                <div className="font-medium truncate">{event.title}</div>
                {heightPx > 30 && (
                  <div className="opacity-80 truncate">
                    {format(event.start, "HH:mm")} - {format(event.end, "HH:mm")}
                  </div>
                )}
              </>
            )}
          </button>
        );
      })}
    </div>
  );
};

// ─── Time Grid (shared for Week & Day) ─────────────────────────────────────────

interface TimeGridProps {
  days: Date[];
  events: CalendarEvent[];
  onEventClick: (eventId: string) => void;
  onSelectSlot: (start: string, end: string) => void;
  onEventMove?: (eventId: string, newStart: string, newEnd: string) => void;
  enableDragDrop?: boolean;
  dc: (typeof CALENDAR_DENSITY_CONFIG)[Densities];
}

const TimeGrid: React.FC<TimeGridProps> = ({
  days,
  events,
  onEventClick,
  onSelectSlot,
  onEventMove,
  enableDragDrop,
  dc,
}) => {
  // Get events that overlap a given day (non-allDay events only)
  const getEventsForDay = useCallback(
    (day: Date) => {
      const dayStart = startOfDay(day);
      const dayEnd = endOfDay(day);
      return events.filter((e) => !e.allDay && e.start < dayEnd && e.end > dayStart);
    },
    [events],
  );

  // All-day events for header
  const getAllDayEvents = useCallback(
    (day: Date) => {
      return events.filter(
        (e) =>
          e.allDay &&
          isWithinInterval(startOfDay(day), {
            start: startOfDay(e.start),
            end: startOfDay(e.end),
          }),
      );
    },
    [events],
  );

  const hasAnyAllDay = days.some((d) => getAllDayEvents(d).length > 0);

  return (
    <div className="flex flex-col flex-1 overflow-hidden">
      {/* Header: day names */}
      <div className="flex border-b border-border flex-shrink-0">
        <div className={cn(dc.timeGutterWidth, "flex-shrink-0")} />
        {days.map((day) => (
          <div key={day.toISOString()} className="flex-1 text-center py-1.5 border-l border-border">
            <div className="text-xs text-muted-foreground">{format(day, "EEE")}</div>
            <div
              className={cn(
                "font-semibold mx-auto flex items-center justify-center rounded-full",
                dc.dayHeaderBadge,
                isToday(day) && "bg-primary text-primary-foreground",
              )}
            >
              {format(day, "d")}
            </div>
          </div>
        ))}
      </div>

      {/* All-day row */}
      {hasAnyAllDay && (
        <div className="flex border-b border-border flex-shrink-0">
          <div
            className={cn(dc.timeGutterWidth, "flex-shrink-0 flex items-center justify-end pr-2")}
          >
            <span className="text-[10px] text-muted-foreground">all-day</span>
          </div>
          {days.map((day) => {
            const allDayEvts = getAllDayEvents(day);
            return (
              <AllDayCell
                key={day.toISOString()}
                day={day}
                allDayEvents={allDayEvts}
                onEventClick={onEventClick}
                dc={dc}
              />
            );
          })}
        </div>
      )}

      {/* Time grid body */}
      <div className="flex-1 overflow-y-auto">
        <div className="flex relative" style={{ height: HOURS.length * dc.hourHeight }}>
          {/* Time labels */}
          <div className={cn(dc.timeGutterWidth, "flex-shrink-0 relative")}>
            {HOURS.map((hour) => (
              <div
                key={hour}
                className="absolute right-2 text-xs text-muted-foreground"
                style={{ top: hour * dc.hourHeight - 6 }}
              >
                {hour === 0 ? "" : format(new Date(2000, 0, 1, hour), "HH:mm")}
              </div>
            ))}
          </div>

          {/* Day columns */}
          {days.map((day) => {
            const dayEvents = getEventsForDay(day);
            return (
              <TimeGridDayColumn
                key={day.toISOString()}
                day={day}
                dayEvents={dayEvents}
                events={events}
                onEventClick={onEventClick}
                onSelectSlot={onSelectSlot}
                onEventMove={onEventMove}
                enableDragDrop={enableDragDrop}
                dc={dc}
              />
            );
          })}
        </div>
      </div>
    </div>
  );
};

// ─── Week View ─────────────────────────────────────────────────────────────────

interface WeekViewProps {
  currentDate: Date;
  events: CalendarEvent[];
  onEventClick: (eventId: string) => void;
  onSelectSlot: (start: string, end: string) => void;
  onEventMove?: (eventId: string, newStart: string, newEnd: string) => void;
  enableDragDrop?: boolean;
  dc: (typeof CALENDAR_DENSITY_CONFIG)[Densities];
}

const WeekView: React.FC<WeekViewProps> = (props) => {
  const days = useMemo(() => {
    const weekStart = startOfWeek(props.currentDate, { weekStartsOn: 1 });
    return Array.from({ length: 7 }, (_, i) => addDays(weekStart, i));
  }, [props.currentDate]);

  return (
    <TimeGrid
      days={days}
      events={props.events}
      onEventClick={props.onEventClick}
      onSelectSlot={props.onSelectSlot}
      onEventMove={props.onEventMove}
      enableDragDrop={props.enableDragDrop}
      dc={props.dc}
    />
  );
};

// ─── Day View ──────────────────────────────────────────────────────────────────

interface DayViewProps {
  currentDate: Date;
  events: CalendarEvent[];
  onEventClick: (eventId: string) => void;
  onSelectSlot: (start: string, end: string) => void;
  onEventMove?: (eventId: string, newStart: string, newEnd: string) => void;
  enableDragDrop?: boolean;
  dc: (typeof CALENDAR_DENSITY_CONFIG)[Densities];
}

const DayView: React.FC<DayViewProps> = (props) => {
  const days = useMemo(() => [props.currentDate], [props.currentDate]);

  return (
    <TimeGrid
      days={days}
      events={props.events}
      onEventClick={props.onEventClick}
      onSelectSlot={props.onSelectSlot}
      onEventMove={props.onEventMove}
      enableDragDrop={props.enableDragDrop}
      dc={props.dc}
    />
  );
};

// ─── Agenda Date Group (extracted for per-group hook usage) ───────────────────

interface AgendaDateGroupProps {
  dayEvents: CalendarEvent[];
  onEventClick: (eventId: string) => void;
  dc: (typeof CALENDAR_DENSITY_CONFIG)[Densities];
}

const AgendaDateGroup: React.FC<AgendaDateGroupProps> = ({ dayEvents, onEventClick, dc }) => {
  const date = dayEvents[0].start;
  const { setItemRef, getTabIndex, onKeyDown } = useRovingTabIndex(dayEvents.length);

  return (
    <div className="border-b border-border">
      <div className="flex">
        {/* Date column */}
        <div className={cn(dc.agendaDateWidth, "flex-shrink-0 py-3 px-3 border-r border-border")}>
          <div className="text-sm font-semibold">{format(date, "EEE")}</div>
          <div className={cn("text-2xl font-bold", isToday(date) && "text-primary")}>
            {format(date, "d")}
          </div>
          <div className="text-xs text-muted-foreground">{format(date, "MMM yyyy")}</div>
        </div>

        {/* Events column */}
        <div className="flex-1 py-2 px-2 flex flex-col gap-1">
          {dayEvents.map((event, idx) => {
            const bgColor = event.color
              ? `var(--${event.color.toLowerCase()}, var(--primary))`
              : "var(--primary)";

            return (
              <button
                key={event.id}
                ref={setItemRef(idx)}
                className={cn(
                  "flex items-center gap-3 rounded-md hover:bg-accent transition-colors w-full text-left",
                  dc.agendaRowPadding,
                )}
                onClick={() => onEventClick(event.id)}
                tabIndex={getTabIndex(idx)}
                onKeyDown={(e) => onKeyDown(e, idx)}
              >
                <div
                  className={cn(dc.agendaDotSize, "rounded-full flex-shrink-0")}
                  style={{ backgroundColor: bgColor }}
                />
                <div className="flex-1 min-w-0">
                  <div className="text-sm font-medium truncate">{event.title}</div>
                  <div className="text-xs text-muted-foreground">
                    {event.allDay
                      ? "All day"
                      : `${format(event.start, "HH:mm")} - ${format(event.end, "HH:mm")}`}
                  </div>
                </div>
              </button>
            );
          })}
        </div>
      </div>
    </div>
  );
};

// ─── Agenda View ───────────────────────────────────────────────────────────────

interface AgendaViewProps {
  currentDate: Date;
  events: CalendarEvent[];
  onEventClick: (eventId: string) => void;
  dc: (typeof CALENDAR_DENSITY_CONFIG)[Densities];
}

const AgendaView: React.FC<AgendaViewProps> = ({ currentDate, events, onEventClick, dc }) => {
  // Show events for the next 30 days from current date
  const filteredEvents = useMemo(() => {
    const rangeStart = startOfDay(currentDate);
    const rangeEnd = addDays(rangeStart, 30);
    return events
      .filter((e) => e.start < rangeEnd && e.end > rangeStart)
      .sort((a, b) => a.start.getTime() - b.start.getTime());
  }, [currentDate, events]);

  // Group events by date
  const grouped = useMemo(() => {
    const map = new Map<string, CalendarEvent[]>();
    filteredEvents.forEach((event) => {
      const key = format(event.start, "yyyy-MM-dd");
      if (!map.has(key)) map.set(key, []);
      map.get(key)!.push(event);
    });
    return Array.from(map.entries());
  }, [filteredEvents]);

  if (grouped.length === 0) {
    return (
      <div className="flex-1 flex items-center justify-center text-muted-foreground text-sm py-12">
        No events in the next 30 days.
      </div>
    );
  }

  return (
    <div className="flex-1 overflow-y-auto">
      {grouped.map(([dateKey, dayEvents]) => (
        <AgendaDateGroup key={dateKey} dayEvents={dayEvents} onEventClick={onEventClick} dc={dc} />
      ))}
    </div>
  );
};

// ─── Main CalendarWidget ───────────────────────────────────────────────────────

export const CalendarWidget: React.FC<CalendarWidgetProps> = ({
  id,
  defaultView = "month",
  defaultDate,
  enableDragDrop = false,
  showToolbar = true,
  density,
  width,
  height,
  density: _density = Densities.Medium,
  slots,
  widgetNodeChildren,
}) => {
  const dc = CALENDAR_DENSITY_CONFIG[density ?? Densities.Medium];
  const [currentDate, setCurrentDate] = useState(() => {
    if (defaultDate) {
      try {
        const d = new Date(defaultDate);
        if (!isNaN(d.getTime())) return d;
      } catch {
        // fall through
      }
    }
    return new Date();
  });
  const [view, setView] = useState<CalendarView>(
    (defaultView?.toLowerCase() as CalendarView) || "month",
  );

  const calendarEvents = useCalendarData(slots, widgetNodeChildren);
  const { handleEventClick, handleSelectSlot, handleEventMove } = useCalendarHandlers(id);

  const onNavigate = useCallback(
    (action: "prev" | "next" | "today") => {
      setCurrentDate((prev) => {
        if (action === "today") return new Date();
        const delta = action === "next" ? 1 : -1;
        switch (view) {
          case "month":
            return delta > 0 ? addMonths(prev, 1) : subMonths(prev, 1);
          case "week":
            return delta > 0 ? addWeeks(prev, 1) : subWeeks(prev, 1);
          case "day":
          case "agenda":
            return delta > 0 ? addDays(prev, 1) : subDays(prev, 1);
          default:
            return prev;
        }
      });
    },
    [view],
  );

  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
    boxSizing: "border-box",
  };

  // If no explicit height set, default to a reasonable size
  if (!height) {
    styles.height = "600px";
  }

  return (
    <div
      style={styles}
      className="flex flex-col border border-border rounded-md bg-background text-foreground overflow-hidden"
    >
      {showToolbar ? (
        <Toolbar
          currentDate={currentDate}
          view={view}
          onNavigate={onNavigate}
          onViewChange={setView}
          dc={dc}
        />
      ) : null}

      {view === "month" && (
        <MonthView
          currentDate={currentDate}
          events={calendarEvents}
          onEventClick={handleEventClick}
          onSelectSlot={handleSelectSlot}
          onEventMove={enableDragDrop ? handleEventMove : undefined}
          enableDragDrop={enableDragDrop}
          dc={dc}
        />
      )}
      {view === "week" && (
        <WeekView
          currentDate={currentDate}
          events={calendarEvents}
          onEventClick={handleEventClick}
          onSelectSlot={handleSelectSlot}
          onEventMove={enableDragDrop ? handleEventMove : undefined}
          enableDragDrop={enableDragDrop}
          dc={dc}
        />
      )}
      {view === "day" && (
        <DayView
          currentDate={currentDate}
          events={calendarEvents}
          onEventClick={handleEventClick}
          onSelectSlot={handleSelectSlot}
          onEventMove={enableDragDrop ? handleEventMove : undefined}
          enableDragDrop={enableDragDrop}
          dc={dc}
        />
      )}
      {view === "agenda" && (
        <AgendaView
          currentDate={currentDate}
          events={calendarEvents}
          onEventClick={handleEventClick}
          dc={dc}
        />
      )}
    </div>
  );
};
