import { getWireframePalette } from "./wireframeColors";
import React from "react";

interface WireframeCalloutWidgetProps {
  id: string;
  label?: string;
  color?: string;
}

export const WireframeCalloutWidget: React.FC<WireframeCalloutWidgetProps> = ({ label, color }) => {
  const palette = getWireframePalette(color);
  const size = 44;

  return (
    <div
      style={{
        display: "inline-flex",
        alignItems: "center",
        justifyContent: "center",
        width: size,
        height: size,
        filter: `drop-shadow(2px 3px 4px ${palette.shadow})`,
      }}
    >
      <svg width={size} height={size} viewBox="0 0 44 44" style={{ position: "absolute" }}>
        <path
          d={`M 22 2
              C 30 1, 40 6, 42 14
              C 44 22, 42 32, 36 38
              C 30 44, 18 44, 10 38
              C 3 32, 1 22, 3 14
              C 6 6, 14 1, 22 2 Z`}
          fill={palette.bg}
          stroke={palette.border}
          strokeWidth="2"
          strokeLinejoin="round"
        />
      </svg>
      <span
        style={{
          position: "relative",
          color: palette.text,
          fontFamily: "'Comic Sans MS', 'Segoe Print', 'Bradley Hand', cursive",
          fontSize: "16px",
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
