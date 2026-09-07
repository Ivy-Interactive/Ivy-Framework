function ivyColorNameToToken(name: string): string {
  return name.replace(/([a-z0-9])([A-Z])/g, "$1-$2").toLowerCase();
}

/** Resolves an Ivy `Colors` enum name (JSON) to a concrete color for SVG fill/stroke. */
export function resolveIvyColorForSvg(name: string | undefined): string | undefined {
  if (!name?.trim() || typeof document === "undefined") return undefined;

  const cssVar = `var(--color-${ivyColorNameToToken(name.trim())})`;
  const probe = document.createElement("span");
  probe.style.position = "absolute";
  probe.style.visibility = "hidden";
  probe.style.pointerEvents = "none";
  probe.style.color = cssVar;
  document.documentElement.appendChild(probe);
  const resolved = getComputedStyle(probe).color.trim();
  document.documentElement.removeChild(probe);

  if (!resolved || resolved === "rgba(0, 0, 0, 0)" || resolved === "transparent") {
    return undefined;
  }
  return resolved;
}
