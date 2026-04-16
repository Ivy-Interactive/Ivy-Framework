import { describe, expect, it } from "vitest";
import {
  formatTickLabel,
  generateEChartGrid,
  generateTooltip,
  generateXAxis,
  generateYAxis,
} from "./sharedUtils";
import type { XAxisProps, YAxisProps } from "./chartTypes";

describe("formatTickLabel - date formats", () => {
  // Use a fixed UTC date for predictable results
  const testDate = Date.UTC(2026, 3, 7, 13, 30, 45); // 2026-04-07T13:30:45Z

  it('formats "MM/dd HH" correctly', () => {
    expect(formatTickLabel(testDate, "MM/dd HH")).toBe("04/07 13");
  });

  it('formats "MM/dd" correctly', () => {
    expect(formatTickLabel(testDate, "MM/dd")).toBe("04/07");
  });

  it('formats "MMM dd" correctly', () => {
    expect(formatTickLabel(testDate, "MMM dd")).toBe("Apr 07");
  });

  it('formats "MMM yyyy" correctly', () => {
    expect(formatTickLabel(testDate, "MMM yyyy")).toBe("Apr 2026");
  });

  it('formats "yyyy-MM-dd" correctly', () => {
    expect(formatTickLabel(testDate, "yyyy-MM-dd")).toBe("2026-04-07");
  });

  it('formats "MMM dd, yyyy" correctly', () => {
    expect(formatTickLabel(testDate, "MMM dd, yyyy")).toBe("Apr 07, 2026");
  });

  it("handles invalid dates gracefully", () => {
    expect(formatTickLabel("invalid", "MM/dd")).toBe("invalid");
  });

  it('does not interfere with "#,##0,,M" number format', () => {
    expect(formatTickLabel(5000000, "#,##0,,M")).toBe("5M");
  });

  it('does not interfere with "#,##0,K" number format', () => {
    expect(formatTickLabel(5000, "#,##0,K")).toBe("5K");
  });

  it("with explicit UTC timeZone produces same result as no timeZone", () => {
    expect(formatTickLabel(testDate, "MM/dd HH", "UTC")).toBe(
      formatTickLabel(testDate, "MM/dd HH"),
    );
  });

  it('with timeZone "local" produces a valid formatted string', () => {
    const result = formatTickLabel(testDate, "MM/dd HH", "local");
    expect(result).toMatch(/^\d{2}\/\d{2} \d{2}$/);
  });

  it('with explicit IANA timezone "America/New_York" formats correctly', () => {
    // 2026-04-07T13:30:45Z in America/New_York (UTC-4 in April) = 09:30
    expect(formatTickLabel(testDate, "MM/dd HH", "America/New_York")).toBe("04/07 09");
  });
});

describe("formatTickLabel - explicit formatterType", () => {
  const testDate = Date.UTC(2026, 3, 7, 13, 30, 45);

  it("with Number type, skips date detection and formats C2 as currency", () => {
    const result = formatTickLabel(1234.5, "C2", null, "Number");
    expect(result).toContain("1,234.50");
  });

  it("with Number type, formats P0 as percent", () => {
    const result = formatTickLabel(50, "P0", null, "Number");
    expect(result).toContain("50%");
  });

  it("with Date type, skips numeric prefix checks and formats as date", () => {
    const result = formatTickLabel(testDate, "MM/dd HH", null, "Date");
    expect(result).toBe("04/07 13");
  });

  it("with Date type, returns string value for non-date-pattern format", () => {
    // "C2" is not a date pattern, so Date type returns String(value)
    const result = formatTickLabel(1234.5, "C2", null, "Date");
    expect(result).toBe("1234.5");
  });

  it("with Auto type, preserves current behavior", () => {
    expect(formatTickLabel(1234.5, "C2", null, "Auto")).toContain("1,234.50");
    expect(formatTickLabel(testDate, "MM/dd HH", null, "Auto")).toBe("04/07 13");
  });

  it("with undefined type, preserves current behavior (same as Auto)", () => {
    expect(formatTickLabel(1234.5, "C2")).toContain("1,234.50");
    expect(formatTickLabel(testDate, "MM/dd HH")).toBe("04/07 13");
  });
});

describe("generateYAxis", () => {
  describe("multi-axis charts skip largeSpread", () => {
    const multiYAxis = [
      { label: "Cost ($)", tickFormatter: "C2" },
      { label: "Tokens", orientation: "Right" },
    ] as YAxisProps[];

    it("should not apply largeSpread min/max overrides when multiple Y-axes are configured", () => {
      const result = generateYAxis(
        /* largeSpread */ true,
        /* transformValue */ (v) => Math.sign(v) * Math.log10(Math.abs(v) + 1),
        /* minValue */ 0,
        /* maxValue */ 80_000_000,
        multiYAxis,
      );

      expect(Array.isArray(result)).toBe(true);
      const axes = result as Record<string, unknown>[];
      // Neither axis should have min/max set from largeSpread (no domainMin/domainMax was provided)
      for (const axis of axes) {
        expect(axis).not.toHaveProperty("min");
        expect(axis).not.toHaveProperty("max");
      }
    });

    it("should use standard K/M/B formatting (not log-unscale) for multi-axis charts", () => {
      const result = generateYAxis(
        /* largeSpread */ true,
        /* transformValue */ (v) => Math.sign(v) * Math.log10(Math.abs(v) + 1),
        /* minValue */ 0,
        /* maxValue */ 80_000_000,
        multiYAxis,
      );

      const axes = result as Record<string, unknown>[];
      // The Tokens axis (index 1) should use standard formatting, not log-unscale
      const tokensAxis = axes[1] as { axisLabel: { formatter: (v: number) => string | number } };
      // 50,000,000 should format as "50M" (standard), not log-unscaled
      expect(tokensAxis.axisLabel.formatter(50_000_000)).toBe("50M");
      expect(tokensAxis.axisLabel.formatter(5_000)).toBe("5K");
      expect(tokensAxis.axisLabel.formatter(500)).toBe(500);
    });

    it("should use tickFormatter when provided on a multi-axis chart", () => {
      const result = generateYAxis(
        true,
        (v) => Math.sign(v) * Math.log10(Math.abs(v) + 1),
        0,
        80_000_000,
        multiYAxis,
      );

      const axes = result as Record<string, unknown>[];
      // The Cost axis (index 0) has tickFormatter "C2", should format as currency
      const costAxis = axes[0] as { axisLabel: { formatter: (v: number) => string } };
      const formatted = costAxis.axisLabel.formatter(4.5);
      expect(formatted).toContain("4.50");
    });

    it("should use splitNumber 5 (not 3) for multi-axis charts even when largeSpread is true", () => {
      const result = generateYAxis(true, (v) => v, 0, 80_000_000, multiYAxis);

      const axes = result as Record<string, unknown>[];
      for (const axis of axes) {
        expect((axis as { splitNumber: number }).splitNumber).toBe(5);
      }
    });
  });

  describe("single-axis largeSpread still works", () => {
    it("should apply largeSpread min/max for a single axis", () => {
      const result = generateYAxis(
        /* largeSpread */ true,
        /* transformValue */ (v) => Math.sign(v) * Math.log10(Math.abs(v) + 1),
        /* minValue */ 0,
        /* maxValue */ 80_000_000,
      );

      // Single axis should have min/max set from largeSpread transform
      const axis = result as Record<string, unknown>;
      expect(axis).toHaveProperty("min");
      expect(axis).toHaveProperty("max");
    });

    it("should use log-unscale formatter for single axis with largeSpread", () => {
      const result = generateYAxis(
        true,
        (v) => Math.sign(v) * Math.log10(Math.abs(v) + 1),
        0,
        80_000_000,
      );

      const axis = result as { axisLabel: { formatter: (v: number) => string | number } };
      expect(axis.axisLabel.formatter(3)).toBe("999");
    });
  });

  describe("new AxisBase properties", () => {
    // Single-element yAxis returns a single object, not an array
    // Use `unknown` cast because YAxisProps types some fields as `null` instead of `string | null`
    const callSingleYAxis = (props: Record<string, unknown>) =>
      generateYAxis(false, undefined, 0, 100, [props as unknown as YAxisProps]) as Record<
        string,
        unknown
      >;

    it("should return inverse: true when reversed is set", () => {
      const axis = callSingleYAxis({ reversed: true });
      expect(axis).toHaveProperty("inverse", true);
    });

    it("should return axisLine.show: false when axisLine is false", () => {
      const axis = callSingleYAxis({ axisLine: false });
      expect((axis as { axisLine: { show: boolean } }).axisLine.show).toBe(false);
    });

    it("should return axisTick.show: false when tickLine is false", () => {
      const axis = callSingleYAxis({ tickLine: false });
      expect((axis as { axisTick: { show: boolean } }).axisTick.show).toBe(false);
    });

    it("should return name when set", () => {
      const axis = callSingleYAxis({ name: "Revenue" });
      expect(axis).toHaveProperty("name", "Revenue");
    });

    it("should return axisLabel.rotate when angle is set", () => {
      const axis = callSingleYAxis({ angle: 45 });
      expect((axis as { axisLabel: { rotate: number } }).axisLabel.rotate).toBe(45);
    });

    it("should use tickCount as splitNumber when set", () => {
      const axis = callSingleYAxis({ tickCount: 10 });
      expect((axis as { splitNumber: number }).splitNumber).toBe(10);
    });

    it("should return axisTick.length when tickSize is set", () => {
      const axis = callSingleYAxis({ tickSize: 12 });
      expect((axis as { axisTick: { length: number } }).axisTick.length).toBe(12);
    });

    it("should append unit to formatted tick labels", () => {
      const axis = callSingleYAxis({ unit: "%" });
      const typed = axis as { axisLabel: { formatter: (v: number) => string | number } };
      expect(typed.axisLabel.formatter(50)).toBe("50%");
    });

    it("should map scale Log to type log", () => {
      const axis = callSingleYAxis({ scale: "Log" });
      expect(axis).toHaveProperty("type", "log");
    });
  });

  describe("axisPointer.label.formatter", () => {
    const callSingleYAxis = (props: Record<string, unknown>) =>
      generateYAxis(false, undefined, 0, 100, [props as unknown as YAxisProps]) as Record<
        string,
        unknown
      >;

    it("should set axisPointer.label.formatter when tickFormatter is configured", () => {
      const axis = callSingleYAxis({ tickFormatter: "C2" });
      const axisPointer = (axis as { axisPointer: { label: { formatter?: unknown } } }).axisPointer;
      expect(axisPointer.label.formatter).toBeDefined();
      expect(typeof axisPointer.label.formatter).toBe("function");
    });

    it("should produce the same output as the axis tick label formatter", () => {
      const axis = callSingleYAxis({ tickFormatter: "C2" });
      const typed = axis as {
        axisLabel: { formatter: (v: number) => string | number };
        axisPointer: { label: { formatter: (params: { value: number }) => string } };
      };
      const tickFormatted = typed.axisLabel.formatter(4.5);
      const pointerFormatted = typed.axisPointer.label.formatter({ value: 4.5 });
      expect(pointerFormatted).toBe(tickFormatted);
    });

    it("should not set a custom axisPointer.label.formatter when tickFormatter is not set", () => {
      const axis = callSingleYAxis({});
      const axisPointer = (axis as { axisPointer: { label: { formatter?: unknown } } }).axisPointer;
      expect(axisPointer.label.formatter).toBeUndefined();
    });
  });
});

describe("generateXAxis", () => {
  const callGenerateXAxis = (axisProps: XAxisProps) =>
    generateXAxis("line", ["A", "B", "C"], [axisProps]);

  it("should return show: false when hide is true", () => {
    const result = callGenerateXAxis({ hide: true });
    expect(result).toHaveProperty("show", false);
  });

  it("should return show: true by default", () => {
    const result = callGenerateXAxis({});
    expect(result).toHaveProperty("show", true);
  });

  it("should return inverse: true when reversed is true", () => {
    const result = callGenerateXAxis({ reversed: true });
    expect(result).toHaveProperty("inverse", true);
  });

  it("should return axisLine.show: false when axisLine is false", () => {
    const result = callGenerateXAxis({ axisLine: false });
    expect(result.axisLine.show).toBe(false);
  });

  it("should return axisTick.show: false when tickLine is false", () => {
    const result = callGenerateXAxis({ tickLine: false });
    expect(result.axisTick.show).toBe(false);
  });

  it("should return name when set", () => {
    const result = callGenerateXAxis({ name: "Revenue" });
    expect(result).toHaveProperty("name", "Revenue");
  });

  it("should return axisLabel.rotate when angle is set", () => {
    const result = callGenerateXAxis({ angle: 45 });
    expect(result.axisLabel.rotate).toBe(45);
  });

  it("should return splitNumber when tickCount is set", () => {
    const result = callGenerateXAxis({ tickCount: 10 });
    expect(result).toHaveProperty("splitNumber", 10);
  });

  it("should return axisTick.length when tickSize is set", () => {
    const result = callGenerateXAxis({ tickSize: 12 });
    expect(result.axisTick.length).toBe(12);
  });

  it("should map scale Log to type log", () => {
    const result = callGenerateXAxis({ scale: "Log" });
    expect(result).toHaveProperty("type", "log");
  });

  it("should append unit to formatted tick labels", () => {
    const result = callGenerateXAxis({ unit: "kg" });
    const formatted = result.axisLabel.formatter(500);
    expect(formatted).toBe("500kg");
  });

  describe("axisPointer.label.formatter", () => {
    it("should set axisPointer.label.formatter when tickFormatter is configured", () => {
      const result = callGenerateXAxis({ tickFormatter: "N2" });
      expect(result.axisPointer.label.formatter).toBeDefined();
      expect(typeof result.axisPointer.label.formatter).toBe("function");
    });

    it("should produce the same output as the axis tick label formatter", () => {
      const result = callGenerateXAxis({ tickFormatter: "N2" });
      const tickFormatted = result.axisLabel.formatter(1234.5);
      const pointerFormatted = result.axisPointer.label.formatter!({ value: 1234.5 });
      expect(pointerFormatted).toBe(tickFormatted);
    });

    it("should not set a custom axisPointer.label.formatter when tickFormatter is not set", () => {
      const result = callGenerateXAxis({});
      expect(result.axisPointer.label.formatter).toBeUndefined();
    });
  });
});

describe("generateEChartGrid", () => {
  it("returns containLabel: true when Y axes are visible", () => {
    const yAxis: YAxisProps[] = [{ hide: false }];
    const result = generateEChartGrid(undefined, false, yAxis);
    expect(result).toHaveProperty("containLabel", true);
  });

  it("returns containLabel: false and left/right: 15 when all Y axes are hidden (X axes implicitly visible)", () => {
    const yAxis: YAxisProps[] = [{ hide: true }, { hide: true }];
    const result = generateEChartGrid(undefined, false, yAxis);
    expect(result).toHaveProperty("containLabel", false);
    expect(result).toHaveProperty("left", 15);
    expect(result).toHaveProperty("right", 15);
  });

  it("returns containLabel: true when yAxis is empty array", () => {
    const result = generateEChartGrid(undefined, false, []);
    expect(result).toHaveProperty("containLabel", true);
  });

  it("returns left/right 15 when Y axes hidden and X axes visible", () => {
    const yAxis: YAxisProps[] = [{ hide: true }];
    const xAxis: XAxisProps[] = [{ hide: false }];
    const result = generateEChartGrid(undefined, false, yAxis, xAxis);
    expect(result).toHaveProperty("left", 15);
    expect(result).toHaveProperty("right", 15);
    expect(result).toHaveProperty("containLabel", false);
  });

  it("returns left/right 0 when both axes hidden", () => {
    const yAxis: YAxisProps[] = [{ hide: true }];
    const xAxis: XAxisProps[] = [{ hide: true }];
    const result = generateEChartGrid(undefined, false, yAxis, xAxis);
    expect(result).toHaveProperty("left", 0);
    expect(result).toHaveProperty("right", 0);
    expect(result).toHaveProperty("containLabel", false);
  });

  it("returns percentage left/right when Y axes visible", () => {
    const yAxis: YAxisProps[] = [{ hide: false }];
    const xAxis: XAxisProps[] = [{ hide: false }];
    const result = generateEChartGrid(undefined, false, yAxis, xAxis);
    expect(result).toHaveProperty("left", "3%");
    expect(result).toHaveProperty("right", "4%");
    expect(result).toHaveProperty("containLabel", true);
  });
});

describe("generateTooltip - valueFormatter", () => {
  it("returns valueFormatter when valueFormat.formatter is provided", () => {
    const result = generateTooltip(undefined, "shadow", undefined, {
      formatter: "C2",
      formatterType: "Number",
    });
    expect(result).toHaveProperty("valueFormatter");
    expect(typeof result.valueFormatter).toBe("function");
  });

  it("valueFormatter produces same output as formatTickLabel", () => {
    const result = generateTooltip(undefined, "shadow", undefined, {
      formatter: "C2",
      formatterType: "Number",
    });
    const fromTooltip = result.valueFormatter!(1234.5);
    const fromTickLabel = formatTickLabel(1234.5, "C2", undefined, "Number");
    expect(fromTooltip).toBe(fromTickLabel);
  });

  it("does not set valueFormatter when no valueFormat is provided", () => {
    const result = generateTooltip(undefined, "shadow");
    expect(result).not.toHaveProperty("valueFormatter");
  });

  it("does not set valueFormatter when valueFormat.formatter is null", () => {
    const result = generateTooltip(undefined, "shadow", undefined, {
      formatter: null,
    });
    expect(result).not.toHaveProperty("valueFormatter");
  });

  it("does not set valueFormatter when valueFormat.formatter is undefined", () => {
    const result = generateTooltip(undefined, "shadow", undefined, {
      formatter: undefined,
    });
    expect(result).not.toHaveProperty("valueFormatter");
  });
});
