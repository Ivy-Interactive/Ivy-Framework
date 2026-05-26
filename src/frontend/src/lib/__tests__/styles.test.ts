import { describe, it, expect } from "vitest";
import { getColor, getWantedSizeType, isFullSize } from "../styles";

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

describe("getWantedSizeType / isFullSize", () => {
  it("parses Ivy Full size strings", () => {
    expect(getWantedSizeType("Full")).toBe("full");
    expect(isFullSize("Full")).toBe(true);
  });

  it("parses only the wanted segment before comma", () => {
    expect(getWantedSizeType("units:80,full:")).toBe("units");
    expect(isFullSize("units:80,full:")).toBe(false);
  });

  it("parses type before colon", () => {
    expect(getWantedSizeType("full:")).toBe("full");
    expect(isFullSize("full:")).toBe(true);
  });
});
