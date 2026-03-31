import * as React from "react";
import { ChevronDownIcon, ChevronLeftIcon, ChevronRightIcon } from "lucide-react";
import { DayButton, DayPicker, useDayPicker, getDefaultClassNames } from "react-day-picker";
import { parse } from "date-fns";

import { cn } from "@/lib/utils";
import { Button, buttonVariant as buttonVariantStyles } from "@/components/ui/button";
import {
  calendarVariant,
  calendarButtonVariant,
  calendarCaptionVariant,
  calendarWeekdayVariant,
  calendarDayVariant,
} from "./calendar-variant";
import { Densities } from "@/types/density";

export function Calendar({
  className,
  classNames,
  showOutsideDays = true,
  captionLayout = "label",
  buttonVariant = "ghost",
  formatters,
  components,
  density = Densities.Medium,
  ...props
}: React.ComponentProps<typeof DayPicker> & {
  buttonVariant?: React.ComponentProps<typeof Button>["variant"];
  density?: Densities;
}) {
  const defaultClassNames = getDefaultClassNames();

  return (
    <DayPicker
      showOutsideDays={showOutsideDays}
      className={cn(
        calendarVariant({ density }),
        String.raw`rtl:**:[.rdp-button\_next>svg]:rotate-180`,
        String.raw`rtl:**:[.rdp-button\_previous>svg]:rotate-180`,
        className,
      )}
      captionLayout={captionLayout}
      formatters={{
        formatMonthDropdown: (date) => date.toLocaleString("default", { month: "short" }),
        ...formatters,
      }}
      classNames={{
        root: cn("w-fit", defaultClassNames.root),
        months: cn("flex gap-4 flex-col md:flex-row relative", defaultClassNames.months),
        month: cn("flex flex-col w-full gap-4", defaultClassNames.month),
        nav: cn(
          "flex items-center gap-1 w-full absolute top-0 inset-x-0 justify-between pointer-events-none",
          defaultClassNames.nav,
        ),
        button_previous: cn(
          buttonVariantStyles({ variant: buttonVariant }),
          calendarButtonVariant({ density }),
          "pointer-events-auto",
          defaultClassNames.button_previous,
        ),
        button_next: cn(
          buttonVariantStyles({ variant: buttonVariant }),
          calendarButtonVariant({ density }),
          "pointer-events-auto",
          defaultClassNames.button_next,
        ),
        month_caption: cn(calendarCaptionVariant({ density }), defaultClassNames.month_caption),
        dropdowns: cn(
          "w-full flex items-center text-sm font-medium justify-center h-(--cell-size) gap-1.5",
          defaultClassNames.dropdowns,
        ),
        dropdown_root: cn(
          "relative has-focus:border-ring border border-input shadow-xs has-focus:ring-ring/50 has-focus:ring-[3px] rounded-md",
          defaultClassNames.dropdown_root,
        ),
        dropdown: cn("absolute inset-0 opacity-0", defaultClassNames.dropdown),
        caption_label: cn(
          "select-none font-medium",
          captionLayout === "label"
            ? "text-sm"
            : "rounded-md pl-2 pr-1 flex items-center gap-1 text-sm h-8 [&>svg]:text-muted-foreground [&>svg]:size-3.5",
          defaultClassNames.caption_label,
        ),
        table: "w-full border-collapse",
        weekdays: cn("flex", defaultClassNames.weekdays),
        weekday: cn(
          calendarWeekdayVariant({ density }),
          "flex-1", // Ensure headers take equal space
          defaultClassNames.weekday,
        ),
        week: cn("flex w-full mt-2 justify-between", defaultClassNames.week),
        week_number_header: cn(
          "select-none w-(--cell-size) flex-none",
          defaultClassNames.week_number_header,
        ),
        week_number: cn(
          "text-[0.8rem] select-none text-muted-foreground w-(--cell-size) flex-none",
          defaultClassNames.week_number,
        ),
        day: cn(
          "relative flex-1 h-full p-0 text-center [&:first-child[data-selected=true]_button]:rounded-l-md [&:last-child[data-selected=true]_button]:rounded-r-md group/day aspect-square select-none",
          defaultClassNames.day,
        ),
        range_start: cn("rounded-l-md bg-accent", defaultClassNames.range_start),
        range_middle: cn("rounded-none", defaultClassNames.range_middle),
        range_end: cn("rounded-r-md bg-accent", defaultClassNames.range_end),
        today: cn(
          "bg-accent text-accent-foreground rounded-field data-[selected=true]:rounded-none",
          defaultClassNames.today,
        ),
        outside: cn(
          "text-muted-foreground aria-selected:text-muted-foreground",
          defaultClassNames.outside,
        ),
        disabled: cn("text-muted-foreground opacity-50", defaultClassNames.disabled),
        hidden: cn("hidden", defaultClassNames.hidden),
        ...classNames,
      }}
      components={{
        Root: ({ className, rootRef, ...props }) => {
          return <div data-slot="calendar" ref={rootRef} className={cn(className)} {...props} />;
        },
        Chevron: ({ className, orientation, ...props }) => {
          if (orientation === "left") {
            return <ChevronLeftIcon className={cn("size-4", className)} {...props} />;
          }

          if (orientation === "right") {
            return <ChevronRightIcon className={cn("size-4", className)} {...props} />;
          }

          return <ChevronDownIcon className={cn("size-4", className)} {...props} />;
        },
        DayButton: (props) => <CalendarDayButton {...props} density={density} />,
        WeekNumber: ({ children, ...props }) => {
          return (
            <td {...props} className="flex-none">
              <div className="flex size-(--cell-size) items-center justify-center text-center">
                {children}
              </div>
            </td>
          );
        },
        CaptionLabel: (props: { displayMonth?: Date; children?: React.ReactNode }) => (
          <MonthYearInput displayMonth={props.displayMonth} title={props.children} />
        ),
        ...components,
      }}
      {...props}
    />
  );
}

function MonthYearInput({ displayMonth, title }: { displayMonth?: Date; title?: React.ReactNode }) {
  const [monthStr, setMonthStr] = React.useState("");
  const [yearStr, setYearStr] = React.useState("");
  const { goToMonth } = useDayPicker();

  React.useEffect(() => {
    if (displayMonth) {
      setMonthStr(String(displayMonth.getMonth() + 1));
      setYearStr(String(displayMonth.getFullYear()));
    } else if (typeof title === "string") {
      const parsed = parse(title, "MMMM yyyy", new Date());
      if (!isNaN(parsed.getTime())) {
        setMonthStr(String(parsed.getMonth() + 1));
        setYearStr(String(parsed.getFullYear()));
      }
    }
  }, [displayMonth, title]);

  const handleCommit = () => {
    const m = parseInt(monthStr, 10);
    const y = parseInt(yearStr, 10);
    if (!isNaN(m) && !isNaN(y) && m >= 1 && m <= 12 && y >= 1900 && y <= 2100) {
      goToMonth(new Date(y, m - 1, 1));
    }
  };

  const onKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter") {
      e.preventDefault();
      handleCommit();
      (e.target as HTMLElement).blur();
    }
  };

  const inputClass =
    "w-6 text-center bg-transparent border-none outline-none focus:bg-accent rounded text-sm p-0 m-0";

  return (
    <div
      className="flex items-center border border-input rounded-field px-1 py-0.5 gap-0.5 bg-background pointer-events-auto mx-auto w-fit"
      onMouseDown={(e) => e.stopPropagation()}
      onClick={(e) => e.stopPropagation()}
      role="presentation"
    >
      <input
        value={monthStr}
        onChange={(e) => setMonthStr(e.target.value.replace(/\D/g, "").slice(0, 2))}
        onBlur={handleCommit}
        onKeyDown={onKeyDown}
        className={inputClass}
        placeholder="M"
      />
      <span className="text-muted-foreground text-xs">/</span>
      <input
        value={yearStr}
        onChange={(e) => setYearStr(e.target.value.replace(/\D/g, "").slice(0, 4))}
        onBlur={handleCommit}
        onKeyDown={onKeyDown}
        className={cn(inputClass, "w-10")}
        placeholder="YYYY"
      />
    </div>
  );
}

function CalendarDayButton({
  className,
  day,
  modifiers,
  density = Densities.Medium,
  ...props
}: React.ComponentProps<typeof DayButton> & {
  density?: Densities;
}) {
  const defaultClassNames = getDefaultClassNames();

  const ref = React.useRef<HTMLButtonElement>(null);
  React.useEffect(() => {
    if (modifiers.focused) ref.current?.focus();
  }, [modifiers.focused]);

  return (
    <Button
      ref={ref}
      variant="ghost"
      size="icon"
      data-day={day.date.toLocaleDateString()}
      data-selected-single={
        modifiers.selected &&
        !modifiers.range_start &&
        !modifiers.range_end &&
        !modifiers.range_middle
      }
      data-range-start={modifiers.range_start}
      data-range-end={modifiers.range_end}
      data-range-middle={modifiers.range_middle}
      className={cn(calendarDayVariant({ density }), defaultClassNames.day, className)}
      {...props}
    />
  );
}
