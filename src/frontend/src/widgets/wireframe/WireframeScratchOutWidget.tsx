import { getHeight, getWidth } from "@/lib/styles";
import { getWireframePalette } from "./wireframeColors";
import { Densities } from "@/types/density";
import React, { useLayoutEffect, useMemo, useRef, useState } from "react";

interface WireframeScratchOutWidgetProps {
  id: string;
  color?: string;
  width?: string;
  height?: string;
  density?: Densities;
}

const DEFAULT_WIDTH = 200;
const DEFAULT_HEIGHT = 100;

/** Deterministic PRNG so a given scratch-out keeps the same scribble across renders. */
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

/** Catmull-Rom through the points as cubics, so the pen turns in loops, not sharp Vs. */
const smooth = (pts: [number, number][]) => {
  if (pts.length < 2) return "";
  let d = `M ${pts[0][0]} ${pts[0][1]}`;
  for (let i = 0; i < pts.length - 1; i++) {
    const [x0, y0] = pts[i - 1] ?? pts[i];
    const [x1, y1] = pts[i];
    const [x2, y2] = pts[i + 1];
    const [x3, y3] = pts[i + 2] ?? pts[i + 1];
    const c1x = x1 + (x2 - x0) / 6;
    const c1y = y1 + (y2 - y0) / 6;
    const c2x = x2 - (x3 - x1) / 6;
    const c2y = y2 - (y3 - y1) / 6;
    d += ` C ${c1x} ${c1y}, ${c2x} ${c2y}, ${x2} ${y2}`;
  }
  return d;
};

/**
 * One back-and-forth pass across the box: x advances while y flips between the top and
 * bottom edges, so the pen lays down near-vertical strokes joined by looping turns.
 */
const scribblePass = (
  w: number,
  h: number,
  rnd: () => number,
  strokeWidth: number,
  spacing: number,
  offset: number,
) => {
  const margin = strokeWidth * 1.1;
  const left = margin;
  const right = w - margin;
  const top = margin;
  const bottom = h - margin;
  if (right <= left || bottom <= top) return "";

  const columns = Math.max(4, Math.round((right - left) / spacing));
  const step = (right - left) / columns;
  // A slight rightward lean at the top, the way a hand pulls the stroke over.
  const lean = Math.min(w * 0.05, 10);

  const pts: [number, number][] = [];
  for (let i = 0; i <= columns; i++) {
    const atTop = i % 2 === 0;
    const x = left + i * step + offset + (rnd() - 0.5) * step * 0.4 + (atTop ? lean : -lean) * 0.5;
    const slack = (bottom - top) * 0.08;
    const y = atTop ? top + rnd() * slack : bottom - rnd() * slack;
    pts.push([Math.max(0, Math.min(w, x)), y]);
  }
  return smooth(pts);
};

export const WireframeScratchOutWidget: React.FC<WireframeScratchOutWidgetProps> = ({
  id,
  // Mirrors the Colors.Black default on the C# widget: an unchanged Color prop is
  // not serialised, so the fallback has to be restated here.
  color = "Black",
  width,
  height,
  density = Densities.Medium,
}) => {
  const palette = getWireframePalette(color);
  const ref = useRef<HTMLDivElement>(null);
  const [size, setSize] = useState({ w: 0, h: 0 });

  // Measured rather than drawn into a stretched viewBox, so the scribble keeps an even
  // stroke weight and spacing whatever the box is sized to.
  useLayoutEffect(() => {
    const el = ref.current;
    if (!el) return;

    const measure = () => {
      // offsetWidth/Height, not getBoundingClientRect: the latter reports the box AFTER
      // CSS transforms, so a widget inside a scaled WireframeTransform would measure its
      // shrunken on-screen size and then lay out its geometry in that wrong coordinate
      // space, while the SVG viewBox stretched it back up.
      const width = el.offsetWidth;
      const height = el.offsetHeight;
      setSize((prev) =>
        Math.abs(prev.w - width) < 0.5 && Math.abs(prev.h - height) < 0.5
          ? prev
          : { w: width, h: height },
      );
    };

    measure();
    const observer = new ResizeObserver(measure);
    observer.observe(el);
    return () => observer.disconnect();
  }, []);

  let strokeWidth = 4;

  switch (density) {
    case Densities.Small:
      strokeWidth = 2.5;
      break;
    case Densities.Large:
      strokeWidth = 5.5;
      break;
    default:
      break;
  }

  const { w, h } = size;
  const measured = w > 0 && h > 0;

  const passes = useMemo(() => {
    if (!measured) return [];
    const seed = hash(id);
    const spacing = strokeWidth * 1.8;
    // Three overlapping passes, each offset by a third of a column. A single zigzag
    // reads as a tidy spring; the overlap is what fills the area in as scribbled over.
    return [
      scribblePass(w, h, makeRng(seed), strokeWidth, spacing, 0),
      scribblePass(w, h, makeRng(seed ^ 0x9e3779b9), strokeWidth, spacing, spacing / 3),
      scribblePass(w, h, makeRng(seed ^ 0x85ebca6b), strokeWidth, spacing, (spacing * 2) / 3),
    ].filter(Boolean);
  }, [measured, w, h, id, strokeWidth]);

  const style: React.CSSProperties = {
    width: DEFAULT_WIDTH,
    height: DEFAULT_HEIGHT,
    ...getWidth(width),
    ...getHeight(height),
    position: "relative",
    display: "inline-block",
    boxSizing: "border-box",
  };

  return (
    <div ref={ref} style={style}>
      {measured && (
        <svg
          style={{
            position: "absolute",
            inset: 0,
            width: "100%",
            height: "100%",
            overflow: "visible",
            pointerEvents: "none",
          }}
          viewBox={`0 0 ${w} ${h}`}
        >
          {passes.map((d, i) => (
            <path
              key={i}
              d={d}
              fill="none"
              stroke={palette.border}
              strokeWidth={i === 0 ? strokeWidth : strokeWidth * 0.9}
              strokeOpacity={i === 0 ? 1 : 0.85}
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          ))}
        </svg>
      )}
    </div>
  );
};
