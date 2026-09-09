import { getHeight, getWidth } from "@/lib/styles";
import { getWireframePalette } from "./wireframeColors";
import { Densities } from "@/types/density";
import React, { useLayoutEffect, useMemo, useRef, useState } from "react";

type CurlyBraceVariant = "Horizontal" | "Vertical";

interface WireframeCurlyBraceWidgetProps {
  id: string;
  variant?: CurlyBraceVariant;
  color?: string;
  width?: string;
  height?: string;
  density?: Densities;
}

/** Deterministic PRNG so a given brace keeps the same hand across renders. */
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

/** Samples an open Catmull-Rom spline through the anchors into a dense point run. */
const resample = (anchors: Pt[], count: number): Pt[] => {
  const at = (i: number) => anchors[Math.max(0, Math.min(anchors.length - 1, i))];
  const out: Pt[] = [];
  const spans = anchors.length - 1;

  for (let s = 0; s < spans; s++) {
    const [x0, y0] = at(s - 1);
    const [x1, y1] = at(s);
    const [x2, y2] = at(s + 1);
    const [x3, y3] = at(s + 2);
    const steps = Math.max(2, Math.round(count / spans));

    for (let i = 0; i < steps; i++) {
      const t = i / steps;
      const t2 = t * t;
      const t3 = t2 * t;
      out.push([
        0.5 *
          (2 * x1 +
            (x2 - x0) * t +
            (2 * x0 - 5 * x1 + 4 * x2 - x3) * t2 +
            (-x0 + 3 * x1 - 3 * x2 + x3) * t3),
        0.5 *
          (2 * y1 +
            (y2 - y0) * t +
            (2 * y0 - 5 * y1 + 4 * y2 - y3) * t2 +
            (-y0 + 3 * y1 - 3 * y2 + y3) * t3),
      ]);
    }
  }
  out.push(anchors[anchors.length - 1]);
  return out;
};

/**
 * The brace in a canonical frame: it spans downwards along y from 0 to length, with the
 * nub pointing towards -x at depth `depth`. Everything else is this shape mapped into
 * place, which keeps the two variants from drifting apart.
 *
 * The pair of anchors either side of the nub is what keeps the centre a crisp point --
 * a single anchor there gets rounded off by the spline into a shapeless bulge.
 */
const braceAnchors = (length: number, depth: number, rnd: () => number): Pt[] => {
  const arm = depth * 0.46;
  const jitter = () => (rnd() - 0.5) * 1.4;

  return [
    [0 + jitter(), 0],
    [-arm * 0.85 + jitter(), length * 0.08 + jitter()],
    [-arm + jitter(), length * 0.3 + jitter()],
    [-arm * 0.95 + jitter(), length * 0.43 + jitter()],
    [-depth, length * 0.5],
    [-arm * 0.95 + jitter(), length * 0.57 + jitter()],
    [-arm + jitter(), length * 0.7 + jitter()],
    [-arm * 0.85 + jitter(), length * 0.92 + jitter()],
    [0 + jitter(), length],
  ];
};

/**
 * Wraps a centre line in a closed outline whose width follows a profile. SVG cannot vary
 * a stroke's width, so the calligraphic swell of a brace has to be built as a filled
 * shape: thin at both tips and at the nub, fullest through the two arms.
 */
const taperedOutline = (pts: Pt[], halfWidth: number): string => {
  const n = pts.length;
  const left: Pt[] = [];
  const right: Pt[] = [];

  for (let i = 0; i < n; i++) {
    const prev = pts[Math.max(0, i - 1)];
    const next = pts[Math.min(n - 1, i + 1)];
    const dx = next[0] - prev[0];
    const dy = next[1] - prev[1];
    const len = Math.hypot(dx, dy) || 1;
    const px = -dy / len;
    const py = dx / len;

    const t = i / (n - 1);
    // Zeroes at t = 0, 0.5 and 1 -- the two tips and the nub -- peaking through the arms.
    const w = halfWidth * (0.26 + 0.74 * Math.abs(Math.sin(2 * Math.PI * t)));

    left.push([pts[i][0] + px * w, pts[i][1] + py * w]);
    right.push([pts[i][0] - px * w, pts[i][1] - py * w]);
  }

  return smoothClosed([...left, ...right.reverse()]);
};

export const WireframeCurlyBraceWidget: React.FC<WireframeCurlyBraceWidgetProps> = ({
  id,
  // Mirrors the C# defaults: props still at their default are not serialised, so the
  // fallbacks have to be restated here.
  variant = "Horizontal",
  color = "Black",
  width,
  height,
  density = Densities.Medium,
}) => {
  const palette = getWireframePalette(color);
  const ref = useRef<HTMLDivElement>(null);
  const [size, setSize] = useState({ w: 0, h: 0 });

  // Measured rather than drawn into a stretched viewBox: a brace stretched to a tall box
  // would smear its nub and tips out of shape.
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

  let halfWidth = 3;

  switch (density) {
    case Densities.Small:
      halfWidth = 2.2;
      break;
    case Densities.Large:
      halfWidth = 4;
      break;
    default:
      break;
  }

  // Deepened towards black rather than towards palette.text, which is the light contrast
  // colour on the dark palettes and would wash the brace out to grey.
  const ink = `color-mix(in srgb, ${palette.border} 75%, #000000)`;

  const { w, h } = size;
  const measured = w > 0 && h > 0;

  const path = useMemo(() => {
    if (!measured) return null;

    const rnd = makeRng(hash(id));
    const inset = halfWidth + 1;
    // Horizontal: nub points left, so the brace spans the height and the width is depth.
    // Vertical: the same shape turned a quarter, spanning the width with the nub up.
    const length = (variant === "Vertical" ? w : h) - inset * 2;
    const depth = (variant === "Vertical" ? h : w) - inset * 2;
    if (length < 8 || depth < 4) return null;

    const canonical = resample(braceAnchors(length, depth, rnd), 90);

    const placed: Pt[] = canonical.map(([cx, cy]) =>
      variant === "Vertical" ? [inset + cy, inset + depth + cx] : [inset + depth + cx, inset + cy],
    );

    return taperedOutline(placed, halfWidth);
  }, [measured, w, h, id, variant, halfWidth]);

  const style: React.CSSProperties = {
    width: variant === "Vertical" ? 140 : 26,
    height: variant === "Vertical" ? 26 : 140,
    ...getWidth(width),
    ...getHeight(height),
    position: "relative",
    display: "inline-block",
    boxSizing: "border-box",
  };

  return (
    <div ref={ref} style={style}>
      {path && (
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
          <path d={path} fill={ink} stroke="none" />
        </svg>
      )}
    </div>
  );
};
