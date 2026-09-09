import { getWireframePalette } from "./wireframeColors";
import { Densities } from "@/types/density";
import React, { useMemo } from "react";

interface WireframeCalloutWidgetProps {
  id: string;
  label?: string;
  color?: string;
  density?: Densities;
}

/** Deterministic PRNG so a given callout keeps the same wobble across renders. */
const makeRng = (seed: number) => {
  let a = seed >>> 0;
  return () => {
    a = (a + 0x6d2b79f5) >>> 0;
    let t = Math.imul(a ^ (a >>> 15), 1 | a);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
};

const hash = (value: string) => {
  let h = 2166136261;
  for (let i = 0; i < value.length; i++) {
    h = Math.imul(h ^ value.charCodeAt(i), 16777619);
  }
  return h >>> 0;
};

/**
 * A closed blob: points sampled round a circle with jittered angle and radius, then
 * joined with Catmull-Rom-derived cubics so the result stays smooth rather than
 * polygonal. Jitter is in pixels, so the hand feel is identical at every density.
 */
const blobPath = (
  cx: number,
  cy: number,
  r: number,
  rnd: () => number,
  wobble: number,
  segments = 9,
) => {
  const pts: [number, number][] = [];
  for (let i = 0; i < segments; i++) {
    const angle = (i / segments) * Math.PI * 2 + (rnd() - 0.5) * 0.18;
    const radius = r + (rnd() - 0.5) * wobble;
    pts.push([cx + Math.cos(angle) * radius, cy + Math.sin(angle) * radius]);
  }

  const at = (i: number) => pts[(i + pts.length) % pts.length];
  let d = `M ${at(0)[0]} ${at(0)[1]}`;
  for (let i = 0; i < pts.length; i++) {
    const [x0, y0] = at(i - 1);
    const [x1, y1] = at(i);
    const [x2, y2] = at(i + 1);
    const [x3, y3] = at(i + 2);
    const c1x = x1 + (x2 - x0) / 6;
    const c1y = y1 + (y2 - y0) / 6;
    const c2x = x2 - (x3 - x1) / 6;
    const c2y = y2 - (y3 - y1) / 6;
    d += ` C ${c1x} ${c1y}, ${c2x} ${c2y}, ${x2} ${y2}`;
  }
  return d + " Z";
};

export const WireframeCalloutWidget: React.FC<WireframeCalloutWidgetProps> = ({
  id,
  label,
  color,
  density = Densities.Medium,
}) => {
  const palette = getWireframePalette(color);

  let size = 35;
  let fontSize = 13;

  switch (density) {
    case Densities.Small:
      size = 28;
      fontSize = 11;
      break;
    case Densities.Large:
      size = 44;
      fontSize = 16;
      break;
    default:
      break;
  }

  // A felt-tip weight: heavy enough to read as ink rather than a hairline outline.
  const strokeWidth = size * 0.08;

  const sketch = useMemo(() => {
    const seed = hash(id);
    const cx = size / 2;
    const cy = size / 2;
    const r = size / 2 - strokeWidth - 0.5;
    const wobble = size * 0.05;

    // The fill is drawn slightly off the outline and a touch fatter, so the colour
    // spills past the ink on one side the way a hand-coloured marker fill does.
    const fill = blobPath(
      cx - size * 0.02,
      cy + size * 0.02,
      r + 0.6,
      makeRng(seed ^ 0x1b873593),
      wobble,
    );

    // Two overlapping passes of the same circle. SVG cannot taper a stroke, so the
    // varying weight comes from two slightly different outlines sitting on top of
    // each other -- thin where they agree, heavier where they diverge.
    const inkA = blobPath(cx, cy, r, makeRng(seed), wobble);
    const inkB = blobPath(cx, cy, r, makeRng(seed ^ 0x9e3779b9), wobble * 1.15);

    return { fill, inkA, inkB };
  }, [id, size, strokeWidth]);

  return (
    <div
      style={{
        position: "relative",
        display: "inline-flex",
        alignItems: "center",
        justifyContent: "center",
        width: size,
        height: size,
        filter: `drop-shadow(1px 2px 2px ${palette.shadow})`,
      }}
    >
      <svg
        width={size}
        height={size}
        viewBox={`0 0 ${size} ${size}`}
        style={{ position: "absolute", overflow: "visible" }}
      >
        <path d={sketch.fill} fill={palette.bg} stroke="none" />
        <path
          d={sketch.inkB}
          fill="none"
          stroke={palette.text}
          strokeWidth={strokeWidth * 0.8}
          strokeOpacity={0.55}
          strokeLinejoin="round"
          strokeLinecap="round"
        />
        <path
          d={sketch.inkA}
          fill="none"
          stroke={palette.text}
          strokeWidth={strokeWidth}
          strokeLinejoin="round"
          strokeLinecap="round"
        />
      </svg>
      <span
        style={{
          position: "relative",
          color: palette.text,
          fontFamily: "'Comic Sans MS', 'Segoe Print', 'Bradley Hand', cursive",
          fontSize: `${fontSize}px`,
          fontWeight: 700,
          lineHeight: 1,
          userSelect: "none",
        }}
      >
        {label}
      </span>
    </div>
  );
};
