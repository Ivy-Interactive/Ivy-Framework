import { getHeight, getWidth } from "@/lib/styles";
import { getWireframePalette } from "./wireframeColors";
import { Densities } from "@/types/density";
import React, { useLayoutEffect, useMemo, useRef, useState } from "react";

interface WireframeRedXWidgetProps {
  id: string;
  color?: string;
  width?: string;
  height?: string;
  density?: Densities;
}

const DEFAULT_WIDTH = 200;
const DEFAULT_HEIGHT = 100;

/** Deterministic PRNG so a given X keeps the same hand across renders. */
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

/** Catmull-Rom around a closed loop of points, as cubics. */
const smoothClosed = (pts: [number, number][]) => {
  if (pts.length < 3) return "";
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

/**
 * A brush stroke as a closed filled outline rather than a stroked line: SVG cannot
 * taper a stroke, so the width is baked into the shape. Half-width follows a sine
 * profile -- nothing at the tips, fullest in the middle -- and the centre line bows
 * slightly, which is what stops it reading as a ruler-drawn line.
 */
const taperedStroke = (
  x1: number,
  y1: number,
  x2: number,
  y2: number,
  halfWidth: number,
  bow: number,
  rnd: () => number,
) => {
  const steps = 16;
  const dx = x2 - x1;
  const dy = y2 - y1;
  const len = Math.hypot(dx, dy) || 1;
  const px = -dy / len;
  const py = dx / len;

  const left: [number, number][] = [];
  const right: [number, number][] = [];

  for (let i = 0; i <= steps; i++) {
    const t = i / steps;
    const arc = Math.sin(Math.PI * t);
    const drift = arc * bow + (rnd() - 0.5) * halfWidth * 0.3;
    const cx = x1 + dx * t + px * drift;
    const cy = y1 + dy * t + py * drift;
    // The ends keep a fraction of the width rather than tapering to nothing: this is
    // a marker being dragged across, not a calligraphic brush coming to a point.
    const w = halfWidth * (0.4 + 0.6 * Math.pow(arc, 0.6));
    left.push([cx + px * w, cy + py * w]);
    right.push([cx - px * w, cy - py * w]);
  }

  return smoothClosed([...left, ...right.reverse()]);
};

export const WireframeRedXWidget: React.FC<WireframeRedXWidgetProps> = ({
  id,
  // Mirrors the Colors.Red default on the C# widget: an unchanged Color prop is
  // not serialised, so the fallback has to be restated here.
  color = "Red",
  width,
  height,
  density = Densities.Medium,
}) => {
  const palette = getWireframePalette(color);
  const ref = useRef<HTMLDivElement>(null);
  const [size, setSize] = useState({ w: 0, h: 0 });

  // Measured rather than drawn into a stretched viewBox, so the stroke keeps an even
  // taper and weight whatever shape the box is.
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

  let weight = 5.5;

  switch (density) {
    case Densities.Small:
      weight = 3.5;
      break;
    case Densities.Large:
      weight = 7.5;
      break;
    default:
      break;
  }

  // Deepened towards black rather than towards palette.text: on the dark palettes
  // (Black, Zinc) text is the *light* contrast colour, so mixing with it washed the
  // stroke out to grey instead of darkening it.
  const ink = `color-mix(in srgb, ${palette.border} 75%, #000000)`;

  const { w, h } = size;
  const measured = w > 0 && h > 0;

  const strokes = useMemo(() => {
    if (!measured) return [];
    const seed = hash(id);
    const rnd = makeRng(seed);

    // Scale the weight with the box, but only gently and within bounds, so a small
    // X is not hairline and a large one is not a blob.
    const scale = Math.max(0.75, Math.min(1.6, Math.min(w, h) / 100));
    const halfWidth = weight * scale;
    const inset = halfWidth + 2;
    const bow = Math.min(w, h) * 0.035;

    const left = inset;
    const right = w - inset;
    const top = inset;
    const bottom = h - inset;
    if (right <= left || bottom <= top) return [];

    return [
      taperedStroke(left, top, right, bottom, halfWidth, bow, rnd),
      taperedStroke(right, top, left, bottom, halfWidth, -bow, rnd),
    ];
  }, [measured, w, h, id, weight]);

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
          {strokes.map((d, i) => (
            <path key={i} d={d} fill={ink} stroke="none" />
          ))}
        </svg>
      )}
    </div>
  );
};
