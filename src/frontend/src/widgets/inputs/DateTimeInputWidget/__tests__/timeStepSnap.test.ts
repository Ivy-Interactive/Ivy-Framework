import { describe, it, expect } from "vitest";
import {
  parseTimeSpanStepToSeconds,
  parseLocalTimeToSeconds,
  formatSecondsToHms,
  snapLocalTimeSeconds,
} from "../timeStepSnap";

// ---------------------------------------------------------------------------
// parseTimeSpanStepToSeconds
// ---------------------------------------------------------------------------

describe("parseTimeSpanStepToSeconds", () => {
  it("parses 1 hour", () => {
    expect(parseTimeSpanStepToSeconds("01:00:00")).toBe(3600);
  });

  it("parses 15 minutes", () => {
    expect(parseTimeSpanStepToSeconds("00:15:00")).toBe(900);
  });

  it("parses 30 seconds", () => {
    expect(parseTimeSpanStepToSeconds("00:00:30")).toBe(30);
  });

  it("parses days.hours:minutes:seconds format", () => {
    // 1 day (86400) + 2 hours (7200) + 30 minutes (1800) = 95400
    expect(parseTimeSpanStepToSeconds("1.02:30:00")).toBe(95400);
  });

  it("returns 1 for undefined", () => {
    expect(parseTimeSpanStepToSeconds(undefined)).toBe(1);
  });

  it("returns 1 for empty string", () => {
    expect(parseTimeSpanStepToSeconds("")).toBe(1);
  });

  it("parses fractional seconds", () => {
    expect(parseTimeSpanStepToSeconds("00:00:01.5")).toBe(1.5);
  });

  it("parses zero", () => {
    expect(parseTimeSpanStepToSeconds("00:00:00")).toBe(0);
  });
});

// ---------------------------------------------------------------------------
// parseLocalTimeToSeconds
// ---------------------------------------------------------------------------

describe("parseLocalTimeToSeconds", () => {
  it("parses 12:30:00 to 45000", () => {
    expect(parseLocalTimeToSeconds("12:30:00")).toBe(45000);
  });

  it("parses 00:00:00 to 0", () => {
    expect(parseLocalTimeToSeconds("00:00:00")).toBe(0);
  });

  it("parses 23:59:59 to 86399", () => {
    expect(parseLocalTimeToSeconds("23:59:59")).toBe(86399);
  });

  it("returns null for 24:00:00 (invalid hour)", () => {
    expect(parseLocalTimeToSeconds("24:00:00")).toBeNull();
  });

  it("returns null for empty string", () => {
    expect(parseLocalTimeToSeconds("")).toBeNull();
  });

  it("parses HH:mm format without seconds", () => {
    expect(parseLocalTimeToSeconds("12:30")).toBe(45000);
  });

  it("returns null for invalid format", () => {
    expect(parseLocalTimeToSeconds("abc")).toBeNull();
  });

  it("returns null for out-of-range minutes", () => {
    expect(parseLocalTimeToSeconds("12:60:00")).toBeNull();
  });

  it("returns null for out-of-range seconds", () => {
    expect(parseLocalTimeToSeconds("12:30:60")).toBeNull();
  });

  it("handles whitespace", () => {
    expect(parseLocalTimeToSeconds("  12:30:00  ")).toBe(45000);
  });
});

// ---------------------------------------------------------------------------
// formatSecondsToHms
// ---------------------------------------------------------------------------

describe("formatSecondsToHms", () => {
  it("formats 3661 to 01:01:01", () => {
    expect(formatSecondsToHms(3661)).toBe("01:01:01");
  });

  it("formats 0 to 00:00:00", () => {
    expect(formatSecondsToHms(0)).toBe("00:00:00");
  });

  it("formats 86399 to 23:59:59", () => {
    expect(formatSecondsToHms(86399)).toBe("23:59:59");
  });

  it("formats 3600 to 01:00:00", () => {
    expect(formatSecondsToHms(3600)).toBe("01:00:00");
  });

  it("formats 60 to 00:01:00", () => {
    expect(formatSecondsToHms(60)).toBe("00:01:00");
  });

  it("formats 1 to 00:00:01", () => {
    expect(formatSecondsToHms(1)).toBe("00:00:01");
  });
});

// ---------------------------------------------------------------------------
// snapLocalTimeSeconds
// ---------------------------------------------------------------------------

describe("snapLocalTimeSeconds", () => {
  it("snaps to nearest grid point", () => {
    // Grid: 0, 900, 1800, 2700... Step=900 (15 min)
    // Math.round(500/900)=1 → snaps to 900
    expect(snapLocalTimeSeconds(500, 900)).toBe(900);
    // Math.round(400/900)=0 → snaps to 0
    expect(snapLocalTimeSeconds(400, 900)).toBe(0);
    // Math.round(1350/900)=2 → snaps to 1800
    expect(snapLocalTimeSeconds(1350, 900)).toBe(1800);
  });

  it("returns value unchanged when stepSec <= 0", () => {
    expect(snapLocalTimeSeconds(500, 0)).toBe(500);
    expect(snapLocalTimeSeconds(500, -1)).toBe(500);
  });

  it("respects min bound", () => {
    // minSec=3600, step=900. Grid: 3600, 4500, 5400...
    // 3000 < min so clamped to 3600
    expect(snapLocalTimeSeconds(3000, 900, 3600)).toBe(3600);
  });

  it("respects max bound", () => {
    // step=3600, max=7200. Grid: 0, 3600, 7200
    // 8000 > max but nearest grid <= max is 7200
    expect(snapLocalTimeSeconds(8000, 3600, null, 7200)).toBe(7200);
  });

  it("value at grid boundary stays", () => {
    expect(snapLocalTimeSeconds(3600, 900)).toBe(3600);
  });

  it("snaps with min and max bounds", () => {
    // min=3600, max=7200, step=1800. Grid: 3600, 5400, 7200
    expect(snapLocalTimeSeconds(4000, 1800, 3600, 7200)).toBe(3600);
    expect(snapLocalTimeSeconds(5000, 1800, 3600, 7200)).toBe(5400);
    expect(snapLocalTimeSeconds(6500, 1800, 3600, 7200)).toBe(7200);
  });

  it("handles non-finite step", () => {
    expect(snapLocalTimeSeconds(500, Infinity)).toBe(500);
    expect(snapLocalTimeSeconds(500, NaN)).toBe(500);
  });
});
