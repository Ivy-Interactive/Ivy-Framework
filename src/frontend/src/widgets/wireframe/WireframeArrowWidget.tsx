import { getHeight, getWidth } from "@/lib/styles";
import { getWireframePalette } from "./wireframeColors";
import { Densities } from "@/types/density";
import React, { useLayoutEffect, useMemo, useRef, useState } from "react";

type ArrowHeads = "None" | "Start" | "End" | "Both";
type ArrowBend = "None" | "Left" | "Right";
type ArrowDirection =
  | "Right"
  | "Left"
  | "Up"
  | "Down"
  | "UpLeft"
  | "UpRight"
  | "DownLeft"
  | "DownRight";

interface WireframeArrowWidgetProps {
  id: string;
  color?: string;
  direction?: ArrowDirection;
  heads?: ArrowHeads;
  bend?: ArrowBend;
  dashed?: boolean;
  width?: string;
  height?: string;
  density?: Densities;
}

const DEFAULT_WIDTH = 160;
const DEFAULT_HEIGHT = 60;

/** Deterministic PRNG so a given arrow keeps the same hand across renders. */
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

/** Catmull-Rom through an open run of points, as cubics. */
const smooth = (pts: Pt[]) => {
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

/** Where the shaft starts and ends inside the box, for each direction. */
const endpointsFor = (direction: ArrowDirection, w: number, h: number, inset: number): [Pt, Pt] => {
  const l = inset;
  const r = w - inset;
  const t = inset;
  const b = h - inset;
  const cx = w / 2;
  const cy = h / 2;

  switch (direction) {
    case "Left":
      return [
        [r, cy],
        [l, cy],
      ];
    case "Up":
      return [
        [cx, b],
        [cx, t],
      ];
    case "Down":
      return [
        [cx, t],
        [cx, b],
      ];
    case "UpLeft":
      return [
        [r, b],
        [l, t],
      ];
    case "UpRight":
      return [
        [l, b],
        [r, t],
      ];
    case "DownLeft":
      return [
        [r, t],
        [l, b],
      ];
    case "DownRight":
      return [
        [l, t],
        [r, b],
      ];
    case "Right":
    default:
      return [
        [l, cy],
        [r, cy],
      ];
  }
};

/**
 * Points along the shaft. A bend pushes the middle off to one side via a quadratic
 * control point; the per-point jitter is what keeps even a straight run from looking
 * ruler-drawn. Jitter is in pixels, so the hand reads the same at any length.
 */
const shaftPoints = (from: Pt, to: Pt, bend: number, rnd: () => number, wobble: number): Pt[] => {
  const steps = 9;
  const dx = to[0] - from[0];
  const dy = to[1] - from[1];
  const len = Math.hypot(dx, dy) || 1;
  const px = -dy / len;
  const py = dx / len;

  // Control point for the bend, offset perpendicular from the midpoint.
  const mx = from[0] + dx / 2 + px * bend;
  const my = from[1] + dy / 2 + py * bend;

  const pts: Pt[] = [];
  for (let i = 0; i <= steps; i++) {
    const t = i / steps;
    const it = 1 - t;
    // Quadratic bezier through the bend control point.
    const bx = it * it * from[0] + 2 * it * t * mx + t * t * to[0];
    const by = it * it * from[1] + 2 * it * t * my + t * t * to[1];
    // Taper the wobble to nothing at the ends so the heads sit on the shaft cleanly.
    const fade = Math.sin(Math.PI * t);
    const off = (rnd() - 0.5) * wobble * fade;
    pts.push([bx + px * off, by + py * off]);
  }
  return pts;
};

/** Splits a run of points into hand-measured dashes, rather than an even dasharray. */
const dashRuns = (pts: Pt[], dash: number, gap: number, rnd: () => number): Pt[][] => {
  const runs: Pt[][] = [];
  let current: Pt[] = [];
  let drawing = true;
  let remaining = dash * (0.8 + rnd() * 0.4);

  for (let i = 0; i < pts.length - 1; i++) {
    let [x1, y1] = pts[i];
    const [x2, y2] = pts[i + 1];
    let segment = Math.hypot(x2 - x1, y2 - y1);

    if (drawing && current.length === 0) current.push([x1, y1]);

    while (segment > remaining) {
      const t = remaining / segment;
      const nx = x1 + (x2 - x1) * t;
      const ny = y1 + (y2 - y1) * t;

      if (drawing) {
        current.push([nx, ny]);
        runs.push(current);
        current = [];
      } else {
        current = [[nx, ny]];
      }

      drawing = !drawing;
      remaining = (drawing ? dash : gap) * (0.8 + rnd() * 0.4);
      segment -= Math.hypot(nx - x1, ny - y1);
      x1 = nx;
      y1 = ny;
    }

    remaining -= segment;
    if (drawing) current.push([x2, y2]);
  }

  if (drawing && current.length > 1) runs.push(current);
  return runs;
};

/** Two barbs at a tip, angled back along the incoming direction. */
const headPaths = (tip: Pt, from: Pt, size: number, rnd: () => number) => {
  const dx = tip[0] - from[0];
  const dy = tip[1] - from[1];
  const angle = Math.atan2(dy, dx);
  const spread = 0.42 + (rnd() - 0.5) * 0.1;

  return [1, -1].map((side) => {
    const a = angle + Math.PI + side * spread + (rnd() - 0.5) * 0.08;
    const len = size * (0.85 + rnd() * 0.3);
    const end: Pt = [tip[0] + Math.cos(a) * len, tip[1] + Math.sin(a) * len];
    // A slight bow on each barb, so the head is not two straight matchsticks.
    const mx = (tip[0] + end[0]) / 2 + (rnd() - 0.5) * size * 0.18;
    const my = (tip[1] + end[1]) / 2 + (rnd() - 0.5) * size * 0.18;
    return `M ${tip[0]} ${tip[1]} Q ${mx} ${my}, ${end[0]} ${end[1]}`;
  });
};

export const WireframeArrowWidget: React.FC<WireframeArrowWidgetProps> = ({
  id,
  // Mirrors the C# defaults: props still at their default are not serialised, so the
  // fallbacks have to be restated here.
  color = "Black",
  direction = "Right",
  heads = "End",
  bend = "None",
  dashed = false,
  width,
  height,
  density = Densities.Medium,
}) => {
  const palette = getWireframePalette(color);
  const ref = useRef<HTMLDivElement>(null);
  const [size, setSize] = useState({ w: 0, h: 0 });

  // Measured rather than drawn into a stretched viewBox, so the line weight, wobble
  // and head size stay constant whatever the arrow is sized to.
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

  let strokeWidth = 2.5;
  let headSize = 14;

  switch (density) {
    case Densities.Small:
      strokeWidth = 1.8;
      headSize = 11;
      break;
    case Densities.Large:
      strokeWidth = 3.5;
      headSize = 18;
      break;
    default:
      break;
  }

  // Deepened towards black rather than towards palette.text: on the dark palettes
  // (Black, Zinc) text is the *light* contrast colour, so mixing with it washed the
  // line out to grey instead of darkening it.
  const ink = `color-mix(in srgb, ${palette.border} 75%, #000000)`;

  const { w, h } = size;
  const measured = w > 0 && h > 0;

  const geometry = useMemo(() => {
    if (!measured) return null;

    const seed = hash(id);
    const rnd = makeRng(seed);
    const inset = strokeWidth + 2;
    const [from, to] = endpointsFor(direction, w, h, inset);

    const span = Math.hypot(to[0] - from[0], to[1] - from[1]);
    if (span < 4) return null;

    const bendAmount =
      bend === "None"
        ? 0
        : Math.min(span * 0.22, Math.min(w, h) * 0.9) * (bend === "Left" ? -1 : 1);

    const pts = shaftPoints(from, to, bendAmount, rnd, Math.min(5, span * 0.05));
    const shaft = dashed
      ? dashRuns(pts, headSize * 0.75, headSize * 0.5, rnd).map(smooth)
      : [smooth(pts)];

    // A lighter second pass over the whole shaft, the doubled line of a pen going
    // back over its own stroke. Skipped when dashed, where it would just muddy it.
    const secondPass = dashed
      ? []
      : [
          smooth(
            shaftPoints(from, to, bendAmount, makeRng(seed ^ 0x9e3779b9), Math.min(6, span * 0.06)),
          ),
        ];

    const barbs: string[] = [];
    if (heads === "End" || heads === "Both") {
      barbs.push(...headPaths(to, pts[pts.length - 3] ?? from, headSize, rnd));
    }
    if (heads === "Start" || heads === "Both") {
      barbs.push(...headPaths(from, pts[2] ?? to, headSize, rnd));
    }

    return { shaft, secondPass, barbs };
  }, [measured, w, h, id, direction, heads, bend, dashed, strokeWidth, headSize]);

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
      {geometry && (
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
          {geometry.secondPass.map((d, i) => (
            <path
              key={`p${i}`}
              d={d}
              fill="none"
              stroke={ink}
              strokeWidth={strokeWidth * 0.75}
              strokeOpacity={0.4}
              strokeLinecap="round"
            />
          ))}
          {geometry.shaft.map((d, i) => (
            <path
              key={`s${i}`}
              d={d}
              fill="none"
              stroke={ink}
              strokeWidth={strokeWidth}
              strokeLinecap="round"
            />
          ))}
          {geometry.barbs.map((d, i) => (
            <path
              key={`b${i}`}
              d={d}
              fill="none"
              stroke={ink}
              strokeWidth={strokeWidth}
              strokeLinecap="round"
            />
          ))}
        </svg>
      )}
    </div>
  );
};
