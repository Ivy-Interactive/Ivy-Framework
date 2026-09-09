import { getHeight, getWidth } from "@/lib/styles";
import { getWireframePalette } from "./wireframeColors";
import { Densities } from "@/types/density";
import React, { useLayoutEffect, useMemo, useRef, useState } from "react";

interface WireframePlaceholderWidgetProps {
  id: string;
  text?: string;
  color?: string;
  width?: string;
  height?: string;
  density?: Densities;
}

const DEFAULT_WIDTH = 240;
const DEFAULT_HEIGHT = 140;

/** Deterministic PRNG so a given placeholder keeps the same wobble across renders. */
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
 * A cubic from (x1,y1) to (x2,y2) whose control points are pushed off the straight
 * line perpendicularly, giving a slightly unsteady, drawn-by-hand stroke. Offsets are
 * in real pixels, so the wobble reads the same on a thumbnail and on a hero banner.
 */
const sketchEdge = (
  x1: number,
  y1: number,
  x2: number,
  y2: number,
  rnd: () => number,
  amp: number,
  overshoot = 0,
) => {
  const dx = x2 - x1;
  const dy = y2 - y1;
  const len = Math.hypot(dx, dy) || 1;
  const ux = dx / len;
  const uy = dy / len;
  const px = -uy;
  const py = ux;

  const sx = x1 - ux * overshoot;
  const sy = y1 - uy * overshoot;
  const ex = x2 + ux * overshoot;
  const ey = y2 + uy * overshoot;

  const o1 = (rnd() - 0.5) * amp;
  const o2 = (rnd() - 0.5) * amp;

  const c1x = sx + (ex - sx) / 3 + px * o1;
  const c1y = sy + (ey - sy) / 3 + py * o1;
  const c2x = sx + ((ex - sx) * 2) / 3 + px * o2;
  const c2y = sy + ((ey - sy) * 2) / 3 + py * o2;

  return {
    move: `M ${sx} ${sy}`,
    curve: `C ${c1x} ${c1y}, ${c2x} ${c2y}, ${ex} ${ey}`,
  };
};

const edgePath = (
  x1: number,
  y1: number,
  x2: number,
  y2: number,
  rnd: () => number,
  amp: number,
  overshoot = 0,
) => {
  const { move, curve } = sketchEdge(x1, y1, x2, y2, rnd, amp, overshoot);
  return `${move} ${curve}`;
};

export const WireframePlaceholderWidget: React.FC<WireframePlaceholderWidgetProps> = ({
  id,
  text,
  // Mirrors the Colors.Violet default on the C# widget: an unchanged Color prop is
  // not serialised, so the fallback has to be restated here.
  color = "Violet",
  width,
  height,
  density = Densities.Medium,
}) => {
  const palette = getWireframePalette(color);
  const ref = useRef<HTMLDivElement>(null);
  const [size, setSize] = useState({ w: 0, h: 0 });

  // The sketch geometry is built in pixel space, so it has to be measured rather than
  // drawn into a stretched viewBox -- that is what distorted the corners previously.
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

  let fontSize = 13;
  let strokeWidth = 2;

  switch (density) {
    case Densities.Small:
      fontSize = 11;
      strokeWidth = 1.5;
      break;
    case Densities.Large:
      fontSize = 16;
      strokeWidth = 2.5;
      break;
    default:
      break;
  }

  // A near-white wash of the palette colour, used for both the box fill and the
  // label backdrop so the label appears to break the diagonals.
  const tint = `color-mix(in srgb, ${palette.bg} 22%, #ffffff)`;

  const { w, h } = size;
  const measured = w > 0 && h > 0;

  const sketch = useMemo(() => {
    if (!measured) return null;

    const amp = 3;
    const pad = strokeWidth + 1.5;
    const seed = hash(id);
    const cornerRng = makeRng(seed);

    const jitter = () => (cornerRng() - 0.5) * 1.6;
    const tl: [number, number] = [pad + jitter(), pad + jitter()];
    const tr: [number, number] = [w - pad + jitter(), pad + jitter()];
    const br: [number, number] = [w - pad + jitter(), h - pad + jitter()];
    const bl: [number, number] = [pad + jitter(), h - pad + jitter()];

    // Closed outline carrying the wash. Built from the same corners as the strokes
    // below so the fill lines up with the visible edge.
    const fillRng = makeRng(seed);
    const corners: [number, number][] = [tl, tr, br, bl, tl];
    let fill = `M ${tl[0]} ${tl[1]}`;
    for (let i = 0; i < 4; i++) {
      const [x1, y1] = corners[i];
      const [x2, y2] = corners[i + 1];
      fill += " " + sketchEdge(x1, y1, x2, y2, fillRng, amp).curve;
    }
    fill += " Z";

    const strokeRng = makeRng(seed);
    const edges = [
      edgePath(tl[0], tl[1], tr[0], tr[1], strokeRng, amp, 1.5),
      edgePath(tr[0], tr[1], br[0], br[1], strokeRng, amp, 1.5),
      edgePath(br[0], br[1], bl[0], bl[1], strokeRng, amp, 1.5),
      edgePath(bl[0], bl[1], tl[0], tl[1], strokeRng, amp, 1.5),
    ];

    // A lighter second pass over each edge -- the doubled-up line of a pen going
    // round twice, which is most of what reads as hand drawn.
    const secondRng = makeRng(seed ^ 0x9e3779b9);
    const secondPass = [
      edgePath(tl[0], tl[1], tr[0], tr[1], secondRng, amp * 1.4, 2.5),
      edgePath(tr[0], tr[1], br[0], br[1], secondRng, amp * 1.4, 2.5),
      edgePath(br[0], br[1], bl[0], bl[1], secondRng, amp * 1.4, 2.5),
      edgePath(bl[0], bl[1], tl[0], tl[1], secondRng, amp * 1.4, 2.5),
    ];

    const diagRng = makeRng(seed ^ 0x85ebca6b);
    const diagonals = [
      edgePath(tl[0], tl[1], br[0], br[1], diagRng, amp * 1.2),
      edgePath(tr[0], tr[1], bl[0], bl[1], diagRng, amp * 1.2),
    ];

    return { fill, edges, secondPass, diagonals };
  }, [measured, w, h, id, strokeWidth]);

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
    // Until the element has been measured a plain border stands in, so the box is
    // never blank. useLayoutEffect measures before paint, so this rarely shows.
    ...(sketch
      ? {}
      : {
          background: tint,
          border: `${strokeWidth}px solid ${palette.border}`,
          borderRadius: 3,
        }),
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
          <path d={sketch.fill} fill={tint} stroke="none" />
          {sketch.diagonals.map((d, i) => (
            <path
              key={`d${i}`}
              d={d}
              fill="none"
              stroke={palette.border}
              strokeWidth={strokeWidth}
              strokeLinecap="round"
            />
          ))}
          {sketch.secondPass.map((d, i) => (
            <path
              key={`s${i}`}
              d={d}
              fill="none"
              stroke={palette.border}
              strokeWidth={strokeWidth * 0.7}
              strokeOpacity={0.35}
              strokeLinecap="round"
            />
          ))}
          {sketch.edges.map((d, i) => (
            <path
              key={`e${i}`}
              d={d}
              fill="none"
              stroke={palette.border}
              strokeWidth={strokeWidth}
              strokeLinecap="round"
            />
          ))}
        </svg>
      )}
      {text && (
        <span
          style={{
            position: "relative",
            padding: "2px 10px",
            background: tint,
            color: palette.border,
            fontFamily: "'Comic Sans MS', 'Segoe Print', 'Bradley Hand', cursive",
            fontSize: `${fontSize}px`,
            fontWeight: 700,
            lineHeight: 1.4,
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
