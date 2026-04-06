import { describe, expect, it } from "vitest";
import { generateYAxis } from "./sharedUtils";
import type { YAxisProps } from "./chartTypes";

describe("generateYAxis", () => {
  describe("multi-axis charts skip largeSpread", () => {
    const multiYAxis = [
      { label: "Cost ($)", tickFormatter: "C2" },
      { label: "Tokens", orientation: "Right" },
    ] as unknown as YAxisProps[];

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
});
