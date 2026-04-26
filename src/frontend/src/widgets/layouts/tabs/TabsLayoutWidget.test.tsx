import { describe, it, expect } from "vitest";
import { Densities } from "@/types/density";
import { tabTriggerVariant, hoverHighlightVariant } from "./variants";

describe("TabsLayoutWidget Density API", () => {
  it("should have Medium as default density", () => {
    const result = tabTriggerVariant({ density: Densities.Medium });
    expect(result).toContain("px-3");
    expect(result).toContain("py-1.5");
    expect(result).toContain("text-sm");
    expect(result).toContain("h-10");
  });

  it("should apply Small density classes", () => {
    const result = tabTriggerVariant({ density: Densities.Small });
    expect(result).toContain("px-2");
    expect(result).toContain("py-1");
    expect(result).toContain("text-xs");
    expect(result).toContain("h-8");
  });

  it("should apply Large density classes", () => {
    const result = tabTriggerVariant({ density: Densities.Large });
    expect(result).toContain("px-4");
    expect(result).toContain("py-2");
    expect(result).toContain("text-base");
    expect(result).toContain("h-12");
  });

  it("should apply hover highlight height based on density", () => {
    const smallResult = hoverHighlightVariant({ density: Densities.Small });
    expect(smallResult).toContain("h-8");

    const mediumResult = hoverHighlightVariant({ density: Densities.Medium });
    expect(mediumResult).toContain("h-10");

    const largeResult = hoverHighlightVariant({ density: Densities.Large });
    expect(largeResult).toContain("h-12");
  });

  it("should use Medium density when no density is specified", () => {
    const result = tabTriggerVariant({});
    expect(result).toContain("px-3");
    expect(result).toContain("text-sm");
    expect(result).toContain("h-10");
  });
});
