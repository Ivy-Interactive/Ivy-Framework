import { getHeight, getWidth } from "@/lib/styles";
import { getWireframePalette } from "./wireframeColors";
import { Densities } from "@/types/density";
import React from "react";

interface WireframeNoteWidgetProps {
  id: string;
  text?: string;
  color?: string;
  width?: string;
  height?: string;
  density?: Densities;
}

const FOLD_SIZE = 24;

export const WireframeNoteWidget: React.FC<WireframeNoteWidgetProps> = ({
  text,
  color,
  width,
  height,
  density = Densities.Medium,
}) => {
  const palette = getWireframePalette(color);

  let padding = "14px 16px 13px 14px";
  let fontSize = 12;
  let minHeight = 48;

  switch (density) {
    case Densities.Small:
      padding = "11px 13px 10px 11px";
      fontSize = 10;
      minHeight = 38;
      break;
    case Densities.Large:
      padding = "18px 20px 16px 18px";
      fontSize = 15;
      minHeight = 60;
      break;
    default:
      break;
  }

  const style: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
    position: "relative",
    display: "inline-flex",
    flexDirection: "column",
    filter: `drop-shadow(2px 3px 5px ${palette.shadow})`,
    transform: "rotate(-1.2deg)",
  };

  return (
    <div style={style}>
      <svg
        style={{
          position: "absolute",
          inset: 0,
          width: "100%",
          height: "100%",
          pointerEvents: "none",
        }}
        preserveAspectRatio="none"
        viewBox="0 0 200 200"
      >
        <path
          d={`M 2 1
              Q 100 -1, 176 1.5
              L 176 1.5
              L 200 ${FOLD_SIZE}
              Q 201 100, 199 199
              Q 100 201, 1 199
              Q -0.5 100, 2 1 Z`}
          fill={palette.bg}
          stroke={palette.text}
          strokeWidth="1.2"
          strokeOpacity="0.18"
        />
        <path
          d={`M 176 1.5 L 176 ${FOLD_SIZE} L 200 ${FOLD_SIZE} Z`}
          fill={palette.border}
          stroke={palette.text}
          strokeWidth="0.8"
          strokeOpacity="0.15"
        />
      </svg>
      <div
        style={{
          position: "relative",
          padding,
          color: palette.text,
          fontFamily: "'Comic Sans MS', 'Segoe Print', 'Bradley Hand', cursive",
          fontSize: `${fontSize}px`,
          lineHeight: "1.5",
          whiteSpace: "pre-wrap",
          wordBreak: "break-word",
          minHeight: `${minHeight}px`,
        }}
      >
        {text}
      </div>
    </div>
  );
};
