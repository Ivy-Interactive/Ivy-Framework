import { getHeight, getWidth } from "@/lib/styles";
import { getWireframePalette } from "./wireframeColors";
import { Densities } from "@/types/density";
import React, { useLayoutEffect, useMemo, useRef, useState } from "react";

type WireframeShapeKind =
  | "Rectangle"
  | "Ellipse"
  | "Triangle"
  | "Diamond"
  | "Pentagon"
  | "Hexagon"
  | "Octagon"
  | "Star"
  | "Cross"
  | "Parallelogram";

interface WireframeShapeWidgetProps {
  id: string;
  shape?: WireframeShapeKind;
  sides?: number;
  text?: string;
  color?: string;
  filled?: boolean;
  width?: string;
  height?: string;
  density?: Densities;
}

const DEFAULT_WIDTH = 140;
const DEFAULT_HEIGHT = 100;

/** Deterministic PRNG so a given shape keeps the same hand across renders. */
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

type Pt = [number, number];

/** Catmull-Rom around a closed loop of points, as cubics. */
const smoothClosed = (pts: Pt[]) => {
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
 * One edge as a cubic bowed off the straight line, optionally running past its corner.
 * The overshoot is what reads as a pen not stopping cleanly where it should.
 */
const sketchEdge = (a: Pt, b: Pt, rnd: () => number, amp: number, overshoot: number) => {
  const dx = b[0] - a[0];
  const dy = b[1] - a[1];
  const len = Math.hypot(dx, dy) || 1;
  const ux = dx / len;
  const uy = dy / len;
  const px = -uy;
  const py = ux;

  const sx = a[0] - ux * overshoot;
  const sy = a[1] - uy * overshoot;
  const ex = b[0] + ux * overshoot;
  const ey = b[1] + uy * overshoot;

  const o1 = (rnd() - 0.5) * amp;
  const o2 = (rnd() - 0.5) * amp;

  const c1x = sx + (ex - sx) / 3 + px * o1;
  const c1y = sy + (ey - sy) / 3 + py * o1;
  const c2x = sx + ((ex - sx) * 2) / 3 + px * o2;
  const c2y = sy + ((ey - sy) * 2) / 3 + py * o2;

  return `M ${sx} ${sy} C ${c1x} ${c1y}, ${c2x} ${c2y}, ${ex} ${ey}`;
};

/** Points on the ellipse inscribed in the box, starting at the top. */
const onEllipse = (
  cx: number,
  cy: number,
  rx: number,
  ry: number,
  count: number,
  phase = -Math.PI / 2,
): Pt[] =>
  Array.from({ length: count }, (_, i) => {
    const a = phase + (i / count) * Math.PI * 2;
    return [cx + Math.cos(a) * rx, cy + Math.sin(a) * ry] as Pt;
  });

/**
 * Every shape reduces to a closed run of points plus whether its corners are rounded.
 * Cornered shapes get per-edge strokes with overshoot; smooth ones get a single wobbly
 * loop. Each shape is inscribed in the box, so it stretches with Width and Height.
 */
const shapePoints = (
  shape: WireframeShapeKind,
  sides: number,
  w: number,
  h: number,
  inset: number,
): { pts: Pt[]; smooth: boolean } => {
  const l = inset;
  const r = w - inset;
  const t = inset;
  const b = h - inset;
  const cx = w / 2;
  const cy = h / 2;
  const rx = (r - l) / 2;
  const ry = (b - t) / 2;

  if (sides >= 3) {
    return { pts: onEllipse(cx, cy, rx, ry, Math.min(sides, 24)), smooth: false };
  }

  switch (shape) {
    case "Ellipse":
      return { pts: onEllipse(cx, cy, rx, ry, 22), smooth: true };
    case "Triangle":
      return {
        pts: [
          [cx, t],
          [r, b],
          [l, b],
        ],
        smooth: false,
      };
    case "Diamond":
      return {
        pts: [
          [cx, t],
          [r, cy],
          [cx, b],
          [l, cy],
        ],
        smooth: false,
      };
    case "Pentagon":
      return { pts: onEllipse(cx, cy, rx, ry, 5), smooth: false };
    case "Hexagon":
      return { pts: onEllipse(cx, cy, rx, ry, 6), smooth: false };
    case "Octagon":
      return { pts: onEllipse(cx, cy, rx, ry, 8), smooth: false };
    case "Star": {
      const outer = onEllipse(cx, cy, rx, ry, 5);
      const inner = onEllipse(cx, cy, rx * 0.42, ry * 0.42, 5, -Math.PI / 2 + Math.PI / 5);
      return { pts: outer.flatMap((p, i) => [p, inner[i]]), smooth: false };
    }
    case "Cross": {
      const aw = (r - l) * 0.3;
      const ah = (b - t) * 0.3;
      return {
        pts: [
          [cx - aw / 2, t],
          [cx + aw / 2, t],
          [cx + aw / 2, cy - ah / 2],
          [r, cy - ah / 2],
          [r, cy + ah / 2],
          [cx + aw / 2, cy + ah / 2],
          [cx + aw / 2, b],
          [cx - aw / 2, b],
          [cx - aw / 2, cy + ah / 2],
          [l, cy + ah / 2],
          [l, cy - ah / 2],
          [cx - aw / 2, cy - ah / 2],
        ],
        smooth: false,
      };
    }
    case "Parallelogram": {
      const skew = (r - l) * 0.22;
      return {
        pts: [
          [l + skew, t],
          [r, t],
          [r - skew, b],
          [l, b],
        ],
        smooth: false,
      };
    }
    case "Rectangle":
    default:
      return {
        pts: [
          [l, t],
          [r, t],
          [r, b],
          [l, b],
        ],
        smooth: false,
      };
  }
};

export const WireframeShapeWidget: React.FC<WireframeShapeWidgetProps> = ({
  id,
  // Mirrors the C# defaults: props still at their default are not serialised, so the
  // fallbacks have to be restated here.
  shape = "Rectangle",
  sides = 0,
  text,
  color = "Black",
  filled = false,
  width,
  height,
  density = Densities.Medium,
}) => {
  const palette = getWireframePalette(color);
  const ref = useRef<HTMLDivElement>(null);
  const [size, setSize] = useState({ w: 0, h: 0 });

  // Measured rather than drawn into a stretched viewBox, so the wobble and stroke weight
  // stay even however the shape is sized.
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

  let strokeWidth = 2.2;
  let fontSize = 13;

  switch (density) {
    case Densities.Small:
      strokeWidth = 1.5;
      fontSize = 11;
      break;
    case Densities.Large:
      strokeWidth = 2.8;
      fontSize = 16;
      break;
    default:
      break;
  }

  // Deepened towards black rather than towards palette.text, which is the light contrast
  // colour on the dark palettes and would wash the outline out to grey.
  const ink = `color-mix(in srgb, ${palette.border} 75%, #000000)`;
  const wash = `color-mix(in srgb, ${palette.bg} 30%, #ffffff)`;

  const { w, h } = size;
  const measured = w > 0 && h > 0;

  const sketch = useMemo(() => {
    if (!measured) return null;

    const seed = hash(id);
    const inset = strokeWidth + 2;
    if (w - inset * 2 < 6 || h - inset * 2 < 6) return null;

    const { pts, smooth } = shapePoints(shape, sides, w, h, inset);
    const amp = 3.4;
    const jitterRng = makeRng(seed);
    const jittered: Pt[] = pts.map(([x, y]) => [
      x + (jitterRng() - 0.5) * 2.2,
      y + (jitterRng() - 0.5) * 2.2,
    ]);

    if (smooth) {
      // A curve has no corners to overshoot, so its hand comes from two loops that do
      // not quite agree with each other.
      const secondLoopRng = makeRng(seed ^ 0x9e3779b9);
      const secondLoop: Pt[] = pts.map(([x, y]) => [
        x + (secondLoopRng() - 0.5) * 4,
        y + (secondLoopRng() - 0.5) * 4,
      ]);
      return {
        fill: smoothClosed(jittered),
        strokes: [smoothClosed(jittered)],
        second: [smoothClosed(secondLoop)],
      };
    }

    const edgeRng = makeRng(seed);
    const secondRng = makeRng(seed ^ 0x9e3779b9);
    const strokes: string[] = [];
    const second: string[] = [];

    for (let i = 0; i < jittered.length; i++) {
      const a = jittered[i];
      const b = jittered[(i + 1) % jittered.length];
      strokes.push(sketchEdge(a, b, edgeRng, amp, 2.6));
      second.push(sketchEdge(a, b, secondRng, amp * 1.45, 4));
    }

    // Straight closed path for the wash, so the fill follows the drawn edges.
    let fill = `M ${jittered[0][0]} ${jittered[0][1]}`;
    for (let i = 1; i < jittered.length; i++) fill += ` L ${jittered[i][0]} ${jittered[i][1]}`;
    fill += " Z";

    return { fill, strokes, second };
  }, [measured, w, h, id, shape, sides, strokeWidth]);

  const style: React.CSSProperties = {
    width: DEFAULT_WIDTH,
    height: DEFAULT_HEIGHT,
    ...getWidth(width),
    ...getHeight(height),
    position: "relative",
    display: "inline-flex",
    alignItems: "center",
    justifyContent: "center",
    boxSizing: "border-box",
  };

  return (
    <div ref={ref} style={style}>
      {sketch && (
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
          {filled && <path d={sketch.fill} fill={wash} stroke="none" />}
          {sketch.second.map((d, i) => (
            <path
              key={`s${i}`}
              d={d}
              fill="none"
              stroke={ink}
              strokeWidth={strokeWidth * 0.8}
              strokeOpacity={0.42}
              strokeLinecap="round"
            />
          ))}
          {sketch.strokes.map((d, i) => (
            <path
              key={`e${i}`}
              d={d}
              fill="none"
              stroke={ink}
              strokeWidth={strokeWidth}
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          ))}
        </svg>
      )}
      {text && (
        <span
          style={{
            position: "relative",
            padding: "0 6px",
            color: ink,
            fontFamily: "'Comic Sans MS', 'Segoe Print', 'Bradley Hand', cursive",
            fontSize: `${fontSize}px`,
            fontWeight: 700,
            lineHeight: 1.35,
            textAlign: "center",
            whiteSpace: "pre-wrap",
            wordBreak: "break-word",
            userSelect: "none",
          }}
        >
          {text}
        </span>
      )}
    </div>
  );
};
