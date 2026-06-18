import { describe, expect, it } from "vitest";
import { ENGLISH_LOCALE, formatTooltipHeader, resolveLocale } from "./tooltipUtils";
import type { Activity } from "./types";

describe("formatTooltipHeader (English locale)", () => {
  it("formats a daily entry as an English date without a time suffix", () => {
    const day: Activity = { date: "2025-01-15", count: 3 };
    expect(formatTooltipHeader(day, ENGLISH_LOCALE)).toBe("Jan 15, 2025");
  });

  it("renders the calendar day of the entry", () => {
    expect(formatTooltipHeader({ date: "2025-01-01", count: 1 }, ENGLISH_LOCALE)).toBe("Jan 1, 2025");
    expect(formatTooltipHeader({ date: "2025-12-31", count: 1 }, ENGLISH_LOCALE)).toBe("Dec 31, 2025");
  });

  it("appends a zero-padded hour suffix for hourly entries", () => {
    const day: Activity = { date: "2025-01-15", hour: 9, count: 3 };
    expect(formatTooltipHeader(day, ENGLISH_LOCALE)).toBe("Jan 15, 2025, 09:00");
  });

  it("includes the suffix for hour 0 (treats 0 as a real hour, not null)", () => {
    const day: Activity = { date: "2025-01-15", hour: 0, count: 3 };
    expect(formatTooltipHeader(day, ENGLISH_LOCALE)).toBe("Jan 15, 2025, 00:00");
  });

  it("formats the last hour of the day", () => {
    const day: Activity = { date: "2025-01-15", hour: 23, count: 3 };
    expect(formatTooltipHeader(day, ENGLISH_LOCALE)).toBe("Jan 15, 2025, 23:00");
  });

  it("omits the time suffix when hour is null", () => {
    const day = { date: "2025-01-15", hour: null, count: 3 } as Activity;
    expect(formatTooltipHeader(day, ENGLISH_LOCALE)).toBe("Jan 15, 2025");
  });

  it("omits the time suffix when hour is undefined", () => {
    const day: Activity = { date: "2025-01-15", hour: undefined, count: 3 };
    expect(formatTooltipHeader(day, ENGLISH_LOCALE)).toBe("Jan 15, 2025");
  });
});

describe("resolveLocale", () => {
  it("defaults to English when localization is disabled", () => {
    expect(resolveLocale(false)).toBe(ENGLISH_LOCALE);
    expect(resolveLocale(undefined)).toBe(ENGLISH_LOCALE);
  });

  it("uses the browser locale when localization is enabled", () => {
    expect(resolveLocale(true)).not.toBe(ENGLISH_LOCALE);
  });
});
