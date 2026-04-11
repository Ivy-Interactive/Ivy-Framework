import { describe, it, expect } from "vitest";
import { validateAndCapValue, TYPE_LIMITS } from "../NumberInputWidget";

// ---------------------------------------------------------------------------
// validateAndCapValue
// ---------------------------------------------------------------------------

describe("validateAndCapValue", () => {
  it("returns null for null input", () => {
    expect(validateAndCapValue(null, "int")).toBeNull();
  });

  it("returns value unchanged when no targetType", () => {
    expect(validateAndCapValue(42, undefined)).toBe(42);
  });

  it("returns value unchanged for unrecognized type", () => {
    expect(validateAndCapValue(42, "unknown")).toBe(42);
  });

  it("caps byte max (300 → 255)", () => {
    expect(validateAndCapValue(300, "byte")).toBe(255);
  });

  it("caps byte min (-1 → 0)", () => {
    expect(validateAndCapValue(-1, "byte")).toBe(0);
  });

  it("floors integer types (3.7 int → 3)", () => {
    expect(validateAndCapValue(3.7, "int")).toBe(3);
  });

  it("does not floor float types (3.7 float → 3.7)", () => {
    expect(validateAndCapValue(3.7, "float")).toBe(3.7);
  });

  it("does not floor double types", () => {
    expect(validateAndCapValue(1.5, "double")).toBe(1.5);
  });

  it("does not floor decimal types", () => {
    expect(validateAndCapValue(2.99, "decimal")).toBe(2.99);
  });

  it("caps sbyte range (-200 → -128)", () => {
    expect(validateAndCapValue(-200, "sbyte")).toBe(-128);
  });

  it("caps sbyte max (200 → 127)", () => {
    expect(validateAndCapValue(200, "sbyte")).toBe(127);
  });

  it("caps short min boundary", () => {
    expect(validateAndCapValue(-40000, "short")).toBe(-32768);
  });

  it("caps ushort max boundary", () => {
    expect(validateAndCapValue(70000, "ushort")).toBe(65535);
  });

  it("caps uint max boundary", () => {
    expect(validateAndCapValue(5000000000, "uint")).toBe(4294967295);
  });

  it("allows values within type range", () => {
    expect(validateAndCapValue(100, "byte")).toBe(100);
    expect(validateAndCapValue(-50, "sbyte")).toBe(-50);
    expect(validateAndCapValue(1000, "short")).toBe(1000);
    expect(validateAndCapValue(50000, "ushort")).toBe(50000);
  });

  // Boundary value tests for each type in TYPE_LIMITS
  for (const [typeName, limits] of Object.entries(TYPE_LIMITS)) {
    describe(`${typeName} boundaries`, () => {
      it(`min value (${limits.min}) stays unchanged`, () => {
        const result = validateAndCapValue(limits.min, typeName);
        expect(result).toBe(limits.min);
      });

      it(`max value (${limits.max}) stays unchanged`, () => {
        const result = validateAndCapValue(limits.max, typeName);
        expect(result).toBe(limits.max);
      });
    });
  }
});

// ---------------------------------------------------------------------------
// TYPE_LIMITS
// ---------------------------------------------------------------------------

describe("TYPE_LIMITS", () => {
  const expectedTypes = [
    "byte",
    "sbyte",
    "short",
    "ushort",
    "int",
    "uint",
    "long",
    "ulong",
    "float",
    "double",
    "decimal",
  ];

  it("contains all expected numeric types", () => {
    for (const type of expectedTypes) {
      expect(TYPE_LIMITS).toHaveProperty(type);
    }
  });

  it("each entry has min and max", () => {
    for (const [, limits] of Object.entries(TYPE_LIMITS)) {
      expect(limits).toHaveProperty("min");
      expect(limits).toHaveProperty("max");
      expect(typeof limits.min).toBe("number");
      expect(typeof limits.max).toBe("number");
      expect(limits.min).toBeLessThanOrEqual(limits.max);
    }
  });

  it("unsigned types have min=0", () => {
    expect(TYPE_LIMITS.byte.min).toBe(0);
    expect(TYPE_LIMITS.ushort.min).toBe(0);
    expect(TYPE_LIMITS.uint.min).toBe(0);
    expect(TYPE_LIMITS.ulong.min).toBe(0);
  });
});
