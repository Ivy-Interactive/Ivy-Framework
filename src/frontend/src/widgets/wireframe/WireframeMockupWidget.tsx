import { getHeight, getWidth } from "@/lib/styles";
import { getWireframePalette } from "./wireframeColors";
import { Densities } from "@/types/density";
import React, { useLayoutEffect, useMemo, useRef, useState } from "react";

type MockupVariant = "Mobile" | "Website" | "Tablet" | "Desktop";

interface WireframeMockupWidgetProps {
  id: string;
  variant?: MockupVariant;
  color?: string;
  title?: string;
  url?: string;
  width?: string;
  height?: string;
  density?: Densities;
  children?: React.ReactNode;
}

const DEFAULT_SIZE: Record<MockupVariant, [number, number]> = {
  Mobile: [300, 600],
  Tablet: [520, 700],
  Desktop: [660, 500],
  Website: [660, 460],
};

/** Deterministic PRNG so a given mockup keeps the same hand across renders. */
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
type Stroke = { d: string; width: number; opacity?: number; fill?: string };

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
 * Perimeter of a rounded rectangle, sampled at a roughly even spacing the whole way
 * round. Even spacing is the point: the spline is uniformly parameterised, so where a
 * densely sampled corner arc meets a sparsely sampled straight edge it overshoots and
 * throws a cusp -- which shows up as a small cross drawn over the corner.
 */
const roundedRectLoop = (x: number, y: number, w: number, h: number, radius: number): Pt[] => {
  const r = Math.max(0, Math.min(radius, Math.min(w, h) / 2));
  const spacing = 7;
  const pts: Pt[] = [];

  const corners: [number, number, number, number][] = [
    [x + r, y + r, Math.PI, 1.5 * Math.PI],
    [x + w - r, y + r, 1.5 * Math.PI, 2 * Math.PI],
    [x + w - r, y + h - r, 0, 0.5 * Math.PI],
    [x + r, y + h - r, 0.5 * Math.PI, Math.PI],
  ];
  // Straight run following each corner: top, right, bottom, left.
  const edgeLengths = [w - 2 * r, h - 2 * r, w - 2 * r, h - 2 * r];

  for (let c = 0; c < 4; c++) {
    const [cx, cy, from, to] = corners[c];
    const arcSteps = Math.max(2, Math.ceil((r * Math.PI) / 2 / spacing));
    for (let i = 0; i <= arcSteps; i++) {
      const a = from + (to - from) * (i / arcSteps);
      pts.push([cx + Math.cos(a) * r, cy + Math.sin(a) * r]);
    }

    const [ncx, ncy, nfrom] = corners[(c + 1) % 4];
    const last = pts[pts.length - 1];
    const next: Pt = [ncx + Math.cos(nfrom) * r, ncy + Math.sin(nfrom) * r];
    const edgeSteps = Math.max(1, Math.ceil(Math.max(0, edgeLengths[c]) / spacing));
    for (let i = 1; i < edgeSteps; i++) {
      const t = i / edgeSteps;
      pts.push([last[0] + (next[0] - last[0]) * t, last[1] + (next[1] - last[1]) * t]);
    }
  }
  return pts;
};

/**
 * Wanders the loop off true. The offsets come from a handful of control values eased
 * between, not from one draw per point: the loop is sampled every few pixels, and
 * per-point randomness at that density reads as a fuzzy line rather than as a hand.
 */
const jitterLoop = (pts: Pt[], rnd: () => number, amount: number): string => {
  const waves = 7;
  const cx = Array.from({ length: waves }, () => (rnd() - 0.5) * amount);
  const cy = Array.from({ length: waves }, () => (rnd() - 0.5) * amount);

  return smoothClosed(
    pts.map(([x, y], i) => {
      const u = (i / pts.length) * waves;
      const i0 = Math.floor(u) % waves;
      const i1 = (i0 + 1) % waves;
      const t = u - Math.floor(u);
      const ease = t * t * (3 - 2 * t);
      return [
        x + cx[i0] * (1 - ease) + cx[i1] * ease,
        y + cy[i0] * (1 - ease) + cy[i1] * ease,
      ] as Pt;
    }),
  );
};

/** A single unsteady line, optionally running past its ends. */
const sketchLine = (a: Pt, b: Pt, rnd: () => number, amp: number, overshoot = 0): string => {
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

  return `M ${sx} ${sy} C ${sx + (ex - sx) / 3 + px * o1} ${sy + (ey - sy) / 3 + py * o1}, ${
    sx + ((ex - sx) * 2) / 3 + px * o2
  } ${sy + ((ey - sy) * 2) / 3 + py * o2}, ${ex} ${ey}`;
};

type Rect = { x: number; y: number; w: number; h: number };
type Frame = { strokes: Stroke[]; screen: Rect; titleBar?: Rect; addressBar?: Rect };

const buildFrame = (
  variant: MockupVariant,
  w: number,
  h: number,
  rnd: () => number,
  sw: number,
): Frame | null => {
  const strokes: Stroke[] = [];
  const wobble = 2.4;
  const body = sw * 1.4;

  const shell = (x: number, y: number, bw: number, bh: number, radius: number) => {
    strokes.push({
      d: jitterLoop(roundedRectLoop(x, y, bw, bh, radius), rnd, wobble),
      width: body,
    });
  };
  const inner = (x: number, y: number, bw: number, bh: number, radius: number) => {
    strokes.push({
      d: jitterLoop(roundedRectLoop(x, y, bw, bh, radius), rnd, wobble * 0.8),
      width: sw,
    });
  };

  if (variant === "Mobile" || variant === "Tablet") {
    const isPhone = variant === "Mobile";
    const side = isPhone ? 9 : 14;
    const top = isPhone ? 30 : 26;
    const bottom = isPhone ? 26 : 26;
    const pad = body;
    const radius = isPhone ? Math.min(w, h) * 0.09 : 20;

    const bw = w - pad * 2;
    const bh = h - pad * 2;
    if (bw < side * 2 + 40 || bh < top + bottom + 40) return null;

    shell(pad, pad, bw, bh, radius);

    const sx = pad + side;
    const sy = pad + top;
    const sWidth = bw - side * 2;
    const sHeight = bh - top - bottom;
    inner(sx, sy, sWidth, sHeight, Math.max(3, radius - side));

    const midTop = pad + top / 2;
    if (isPhone) {
      // Speaker slot and camera, sitting in the top bezel.
      const pillW = bw * 0.2;
      const pillX = pad + bw / 2 - pillW / 2 - 6;
      strokes.push({
        d: jitterLoop(roundedRectLoop(pillX, midTop - 2.5, pillW, 5, 2.5), rnd, 0.8),
        width: sw * 0.8,
      });
      strokes.push({
        d: jitterLoop(roundedRectLoop(pillX + pillW + 8, midTop - 3.5, 7, 7, 3.5), rnd, 0.7),
        width: sw * 0.8,
      });
      // Home indicator.
      const hiW = bw * 0.3;
      strokes.push({
        d: sketchLine(
          [pad + bw / 2 - hiW / 2, pad + bh - bottom / 2],
          [pad + bw / 2 + hiW / 2, pad + bh - bottom / 2],
          rnd,
          1,
        ),
        width: sw * 1.2,
      });
      // Side buttons.
      strokes.push({
        d: sketchLine([pad, pad + bh * 0.24], [pad, pad + bh * 0.32], rnd, 0.8),
        width: body,
      });
      strokes.push({
        d: sketchLine([pad, pad + bh * 0.37], [pad, pad + bh * 0.47], rnd, 0.8),
        width: body,
      });
      strokes.push({
        d: sketchLine([pad + bw, pad + bh * 0.28], [pad + bw, pad + bh * 0.4], rnd, 0.8),
        width: body,
      });
    } else {
      strokes.push({
        d: jitterLoop(roundedRectLoop(pad + bw / 2 - 3.5, midTop - 3.5, 7, 7, 3.5), rnd, 0.7),
        width: sw * 0.8,
      });
    }

    return { strokes, screen: { x: sx, y: sy, w: sWidth, h: sHeight } };
  }

  if (variant === "Desktop") {
    // A macOS application window: rounded shell, title bar with traffic lights, and
    // the content sitting directly under it. No bezel and no stand -- this is the
    // window, not the machine it is displayed on.
    const pad = body;
    const barH = 32;
    const bw = w - pad * 2;
    const bh = h - pad * 2;
    if (bw < 160 || bh < barH + 40) return null;

    shell(pad, pad, bw, bh, 11);

    const lightY = pad + barH / 2;
    const lightR = 5.5;
    for (let i = 0; i < 3; i++) {
      const lx = pad + 16 + i * 17;
      strokes.push({
        d: jitterLoop(
          roundedRectLoop(lx - lightR, lightY - lightR, lightR * 2, lightR * 2, lightR),
          rnd,
          0.7,
        ),
        width: sw * 0.9,
      });
    }

    strokes.push({
      d: sketchLine([pad, pad + barH], [pad + bw, pad + barH], rnd, 1.2),
      width: sw,
    });

    return {
      strokes,
      screen: { x: pad + 1, y: pad + barH + 1, w: bw - 2, h: bh - barH - 2 },
      titleBar: { x: pad, y: pad, w: bw, h: barH },
    };
  }

  // Website: a browser window -- toolbar with nav glyphs and an address pill, no tab.
  const pad = body;
  const barH = 38;
  const bw = w - pad * 2;
  const bh = h - pad * 2;
  if (bw < 160 || bh < barH + 40) return null;

  shell(pad, pad, bw, bh, 8);

  strokes.push({
    d: sketchLine([pad, pad + barH], [pad + bw, pad + barH], rnd, 1.2),
    width: sw,
  });

  // Back, forward and reload.
  const gy = pad + barH / 2;
  const glyph = (gx: number, dir: number) => {
    strokes.push({ d: sketchLine([gx - 5 * dir, gy], [gx + 5 * dir, gy], rnd, 0.6), width: sw });
    strokes.push({
      d: sketchLine([gx + 5 * dir, gy], [gx + 1 * dir, gy - 4], rnd, 0.6),
      width: sw,
    });
    strokes.push({
      d: sketchLine([gx + 5 * dir, gy], [gx + 1 * dir, gy + 4], rnd, 0.6),
      width: sw,
    });
  };
  glyph(pad + 22, -1);
  glyph(pad + 48, 1);
  const rx = pad + 74;
  strokes.push({
    d: `M ${rx + 5} ${gy - 3} A 5 5 0 1 0 ${rx + 6} ${gy + 4}`,
    width: sw,
    fill: "none",
  });

  const pillX = pad + 92;
  const pillW = bw - 92 - 16;
  const pillH = 22;
  const pillY = gy - pillH / 2;
  strokes.push({
    d: jitterLoop(roundedRectLoop(pillX, pillY, pillW, pillH, pillH / 2), rnd, 0.9),
    width: sw,
  });

  return {
    strokes,
    screen: { x: pad + 1, y: pad + barH + 1, w: bw - 2, h: bh - barH - 2 },
    addressBar: { x: pillX, y: pillY, w: pillW, h: pillH },
  };
};

export const WireframeMockupWidget: React.FC<WireframeMockupWidgetProps> = ({
  id,
  // Mirrors the C# defaults: props still at their default are not serialised, so the
  // fallbacks have to be restated here.
  variant = "Mobile",
  color = "Black",
  title,
  url,
  width,
  height,
  density = Densities.Medium,
  children,
}) => {
  const palette = getWireframePalette(color);
  const ref = useRef<HTMLDivElement>(null);
  const [size, setSize] = useState({ w: 0, h: 0 });

  // Measured rather than drawn into a stretched viewBox: a frame stretched to a tall box
  // would smear its corner radii and its notch out of shape.
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

  let strokeWidth = 1.8;
  let fontSize = 11;

  switch (density) {
    case Densities.Small:
      strokeWidth = 1.4;
      fontSize = 10;
      break;
    case Densities.Large:
      strokeWidth = 2.4;
      fontSize = 13;
      break;
    default:
      break;
  }

  // Deepened towards black rather than towards palette.text, which is the light contrast
  // colour on the dark palettes and would wash the frame out to grey.
  const ink = `color-mix(in srgb, ${palette.border} 75%, #000000)`;

  const { w, h } = size;
  const measured = w > 0 && h > 0;

  const frame = useMemo(
    () => (measured ? buildFrame(variant, w, h, makeRng(hash(id)), strokeWidth) : null),
    [measured, w, h, id, variant, strokeWidth],
  );

  const [defaultW, defaultH] = DEFAULT_SIZE[variant] ?? DEFAULT_SIZE.Mobile;

  const style: React.CSSProperties = {
    width: defaultW,
    height: defaultH,
    ...getWidth(width),
    ...getHeight(height),
    position: "relative",
    display: "inline-block",
    boxSizing: "border-box",
  };

  const chromeFont = "'Comic Sans MS', 'Segoe Print', 'Bradley Hand', cursive";

  return (
    <div ref={ref} style={style}>
      {frame && (
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
          {frame.strokes.map((s, i) => (
            <path
              key={i}
              d={s.d}
              fill={s.fill ?? "none"}
              stroke={ink}
              strokeWidth={s.width}
              strokeOpacity={s.opacity ?? 1}
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          ))}
        </svg>
      )}

      {frame && variant === "Desktop" && title && frame.titleBar && (
        <span
          style={{
            position: "absolute",
            left: frame.titleBar.x + 70,
            top: frame.titleBar.y,
            width: frame.titleBar.w - 140,
            height: frame.titleBar.h,
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            color: ink,
            fontFamily: chromeFont,
            fontSize: `${fontSize}px`,
            fontWeight: 700,
            whiteSpace: "nowrap",
            overflow: "hidden",
            textOverflow: "ellipsis",
            userSelect: "none",
          }}
        >
          {title}
        </span>
      )}

      {frame && variant === "Website" && url && frame.addressBar && (
        <span
          style={{
            position: "absolute",
            left: frame.addressBar.x,
            top: frame.addressBar.y,
            width: frame.addressBar.w,
            height: frame.addressBar.h,
            display: "flex",
            alignItems: "center",
            paddingLeft: 12,
            boxSizing: "border-box",
            color: ink,
            opacity: 0.75,
            fontFamily: chromeFont,
            fontSize: `${fontSize}px`,
            lineHeight: 1,
            whiteSpace: "nowrap",
            overflow: "hidden",
            textOverflow: "ellipsis",
            userSelect: "none",
          }}
        >
          {url}
        </span>
      )}

      {frame && (
        <div
          style={{
            position: "absolute",
            left: frame.screen.x,
            top: frame.screen.y,
            width: frame.screen.w,
            height: frame.screen.h,
            overflow: "hidden",
            boxSizing: "border-box",
          }}
        >
          {children}
        </div>
      )}
    </div>
  );
};
