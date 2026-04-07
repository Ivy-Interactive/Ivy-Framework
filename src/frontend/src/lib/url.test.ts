import { describe, it, expect, afterEach } from "vitest";
import { getFullUrl } from "./url";

describe("getFullUrl", () => {
  let metaElement: HTMLMetaElement | null = null;

  afterEach(() => {
    if (metaElement) {
      metaElement.remove();
      metaElement = null;
    }
  });

  it("returns path as-is when no ivy-host meta tag exists", () => {
    expect(getFullUrl("/api/upload")).toBe("/api/upload");
  });

  it("prepends ivy-host content when meta tag is present", () => {
    metaElement = document.createElement("meta");
    metaElement.name = "ivy-host";
    metaElement.content = "https://proxy.example.com";
    document.head.appendChild(metaElement);

    expect(getFullUrl("/api/upload")).toBe("https://proxy.example.com/api/upload");
  });

  it("handles empty path correctly", () => {
    expect(getFullUrl("")).toBe("");

    metaElement = document.createElement("meta");
    metaElement.name = "ivy-host";
    metaElement.content = "https://proxy.example.com";
    document.head.appendChild(metaElement);

    expect(getFullUrl("")).toBe("https://proxy.example.com");
  });
});
