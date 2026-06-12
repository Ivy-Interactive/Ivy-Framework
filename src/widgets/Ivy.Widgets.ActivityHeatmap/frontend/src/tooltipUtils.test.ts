import { describe, expect, it } from "vitest";
import { formatTooltipHeader } from "./tooltipUtils";
import type { Activity } from "./types";

describe("formatTooltipHeader", () => {
  it("formats a daily entry as a localized date without a time suffix", () => {
    const day: Activity = { date: "2025-01-15", count: 3 };
    expect(formatTooltipHeader(day)).toBe("Jan 15, 2025");
  });

  it("renders the calendar day of the entry", () => {
    expect(formatTooltipHeader({ date: "2025-01-01", count: 1 })).toBe("Jan 1, 2025");
    expect(formatTooltipHeader({ date: "2025-12-31", count: 1 })).toBe("Dec 31, 2025");
  });

  it("appends a zero-padded hour suffix for hourly entries", () => {
    const day: Activity = { date: "2025-01-15", hour: 9, count: 3 };
    expect(formatTooltipHeader(day)).toBe("Jan 15, 2025, 09:00");
  });

  it("includes the suffix for hour 0 (treats 0 as a real hour, not null)", () => {
    const day: Activity = { date: "2025-01-15", hour: 0, count: 3 };
    expect(formatTooltipHeader(day)).toBe("Jan 15, 2025, 00:00");
  });

  it("formats the last hour of the day", () => {
    const day: Activity = { date: "2025-01-15", hour: 23, count: 3 };
    expect(formatTooltipHeader(day)).toBe("Jan 15, 2025, 23:00");
  });

  it("omits the time suffix when hour is null", () => {
    const day = { date: "2025-01-15", hour: null, count: 3 } as Activity;
    expect(formatTooltipHeader(day)).toBe("Jan 15, 2025");
  });

  it("omits the time suffix when hour is undefined", () => {
    const day: Activity = { date: "2025-01-15", hour: undefined, count: 3 };
    expect(formatTooltipHeader(day)).toBe("Jan 15, 2025");
  });
});
