import { describe, it, expect } from "vitest";
import { getColor } from "../styles";

describe("getColor", () => {
  it("resolves IvyGreen to var(--ivy-green)", () => {
    expect(getColor("IvyGreen", "color", "background")).toEqual({
      color: "var(--ivy-green)",
    });
  });

  it("resolves IvyGreen foreground role to var(--ivy-green-foreground)", () => {
    expect(getColor("IvyGreen", "color", "foreground")).toEqual({
      color: "var(--ivy-green-foreground)",
    });
  });

  it("resolves single-word PascalCase colors unchanged", () => {
    expect(getColor("Red", "color", "background")).toEqual({
      color: "var(--red)",
    });
  });

  it("resolves lowercase colors unchanged", () => {
    expect(getColor("primary", "color", "background")).toEqual({
      color: "var(--primary)",
    });
  });

  it("returns empty object for undefined color", () => {
    expect(getColor(undefined)).toEqual({});
  });
});
