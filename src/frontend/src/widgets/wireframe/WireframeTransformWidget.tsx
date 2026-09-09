import { getHeight, getWidth } from "@/lib/styles";
import React, { useLayoutEffect, useRef, useState } from "react";

type TransformOrigin =
  | "Center"
  | "TopLeft"
  | "Top"
  | "TopRight"
  | "Left"
  | "Right"
  | "BottomLeft"
  | "Bottom"
  | "BottomRight";

interface WireframeTransformWidgetProps {
  id: string;
  rotate?: number;
  scale?: number;
  scaleX?: number;
  scaleY?: number;
  skewX?: number;
  skewY?: number;
  offsetX?: number;
  offsetY?: number;
  flipHorizontal?: boolean;
  flipVertical?: boolean;
  origin?: TransformOrigin;
  opacity?: number;
  fit?: boolean;
  width?: string;
  height?: string;
  children?: React.ReactNode;
}

const ORIGINS: Record<TransformOrigin, string> = {
  Center: "center center",
  TopLeft: "left top",
  Top: "center top",
  TopRight: "right top",
  Left: "left center",
  Right: "right center",
  BottomLeft: "left bottom",
  Bottom: "center bottom",
  BottomRight: "right bottom",
};

type Matrix = [number, number, number, number];

const multiply = (a: Matrix, b: Matrix): Matrix => [
  a[0] * b[0] + a[1] * b[2],
  a[0] * b[1] + a[1] * b[3],
  a[2] * b[0] + a[3] * b[2],
  a[2] * b[1] + a[3] * b[3],
];

/**
 * The same composition CSS performs for `translate rotate scale skewX skewY`, as a 2x2
 * matrix. Needed only for `fit`: to size the box to what the child actually covers, the
 * transformed corners have to be computed rather than guessed.
 */
const composeMatrix = (
  rotate: number,
  sx: number,
  sy: number,
  skewX: number,
  skewY: number,
): Matrix => {
  const rad = (rotate * Math.PI) / 180;
  const cos = Math.cos(rad);
  const sin = Math.sin(rad);

  const r: Matrix = [cos, -sin, sin, cos];
  const s: Matrix = [sx, 0, 0, sy];
  const kx: Matrix = [1, Math.tan((skewX * Math.PI) / 180), 0, 1];
  const ky: Matrix = [1, 0, Math.tan((skewY * Math.PI) / 180), 1];

  return multiply(multiply(multiply(r, s), kx), ky);
};

export const WireframeTransformWidget: React.FC<WireframeTransformWidgetProps> = ({
  // Mirrors the C# defaults: props still at their default are not serialised, so the
  // fallbacks have to be restated here.
  rotate = 0,
  scale = 1,
  scaleX,
  scaleY,
  skewX = 0,
  skewY = 0,
  offsetX = 0,
  offsetY = 0,
  flipHorizontal = false,
  flipVertical = false,
  origin = "Center",
  opacity = 1,
  fit = false,
  width,
  height,
  children,
}) => {
  const innerRef = useRef<HTMLDivElement>(null);
  const [natural, setNatural] = useState({ w: 0, h: 0 });

  // Only measured for `fit`. offsetWidth/Height report the untransformed layout box,
  // which is exactly what has to be fed through the matrix.
  useLayoutEffect(() => {
    const el = innerRef.current;
    if (!el || !fit) return;

    const measure = () => {
      const w = el.offsetWidth;
      const h = el.offsetHeight;
      setNatural((prev) =>
        Math.abs(prev.w - w) < 0.5 && Math.abs(prev.h - h) < 0.5 ? prev : { w, h },
      );
    };

    measure();
    const observer = new ResizeObserver(measure);
    observer.observe(el);
    return () => observer.disconnect();
  }, [fit]);

  // Per-axis scale falls back to the uniform one; a flip is just a negative scale, so it
  // composes with an explicit ScaleX/ScaleY rather than fighting it.
  const sx = (scaleX ?? scale) * (flipHorizontal ? -1 : 1);
  const sy = (scaleY ?? scale) * (flipVertical ? -1 : 1);

  const parts: string[] = [];
  if (offsetX !== 0 || offsetY !== 0) parts.push(`translate(${offsetX}px, ${offsetY}px)`);
  if (rotate !== 0) parts.push(`rotate(${rotate}deg)`);
  if (sx !== 1 || sy !== 1) parts.push(`scale(${sx}, ${sy})`);
  if (skewX !== 0) parts.push(`skewX(${skewX}deg)`);
  if (skewY !== 0) parts.push(`skewY(${skewY}deg)`);

  const bounds =
    fit && natural.w > 0 && natural.h > 0
      ? (() => {
          const m = composeMatrix(rotate, sx, sy, skewX, skewY);
          const corners: [number, number][] = [
            [0, 0],
            [natural.w, 0],
            [natural.w, natural.h],
            [0, natural.h],
          ];
          const mapped = corners.map(([x, y]) => [
            m[0] * x + m[1] * y + offsetX,
            m[2] * x + m[3] * y + offsetY,
          ]);
          const xs = mapped.map((p) => p[0]);
          const ys = mapped.map((p) => p[1]);
          const minX = Math.min(...xs);
          const minY = Math.min(...ys);
          return {
            minX,
            minY,
            w: Math.max(...xs) - minX,
            h: Math.max(...ys) - minY,
          };
        })()
      : null;

  const outerStyle: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
    display: "inline-block",
    // With `fit` the box is the transformed bounds, so the child takes up the room it
    // visually occupies. Without it the box stays the child's own, which is plain CSS
    // behaviour: the transform is visual only and neighbours are not pushed around.
    ...(bounds ? { width: bounds.w, height: bounds.h } : {}),
    opacity: opacity === 1 ? undefined : opacity,
  };

  const innerStyle: React.CSSProperties = {
    display: "inline-block",
    // Applied in order translate, rotate, scale, skew, so rotation is about the chosen
    // origin rather than about wherever a preceding scale left it.
    transform: bounds
      ? `translate(${-bounds.minX}px, ${-bounds.minY}px) ${parts.join(" ")}`.trim()
      : parts.length
        ? parts.join(" ")
        : undefined,
    // Fitting measures corners from the child's own top-left, so the pivot has to match;
    // Origin stops mattering because the bounds get recentred either way.
    transformOrigin: bounds ? "0 0" : (ORIGINS[origin] ?? ORIGINS.Center),
  };

  return (
    <div style={outerStyle}>
      <div ref={innerRef} style={innerStyle}>
        {children}
      </div>
    </div>
  );
};
