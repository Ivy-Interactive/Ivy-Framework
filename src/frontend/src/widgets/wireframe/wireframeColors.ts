export interface WireframePalette {
  bg: string;
  border: string;
  shadow: string;
  text: string;
}

const WIREFRAME_COLORS: Record<string, WireframePalette> = {
  Yellow: { bg: "#f5d949", border: "#d4b82e", shadow: "rgba(180,150,30,0.25)", text: "#3a3000" },
  Blue: { bg: "#87ceeb", border: "#5a9aba", shadow: "rgba(80,140,180,0.25)", text: "#1a2e40" },
  Green: { bg: "#90d890", border: "#60a860", shadow: "rgba(80,160,80,0.25)", text: "#1a3a1a" },
  Pink: { bg: "#f5a0c0", border: "#c87898", shadow: "rgba(180,80,120,0.25)", text: "#3a1020" },
  Orange: { bg: "#f5b870", border: "#c89048", shadow: "rgba(180,130,50,0.25)", text: "#3a2000" },
  Purple: { bg: "#c8a8e8", border: "#9878b8", shadow: "rgba(140,100,180,0.25)", text: "#2a1040" },
  Red: { bg: "#f5a0a0", border: "#c87070", shadow: "rgba(180,80,80,0.25)", text: "#3a1010" },
  Amber: { bg: "#f5d080", border: "#c8a850", shadow: "rgba(180,150,50,0.25)", text: "#3a2800" },
  Lime: { bg: "#c8e878", border: "#a0c050", shadow: "rgba(120,180,50,0.25)", text: "#283a10" },
  Emerald: { bg: "#80d8b0", border: "#58b088", shadow: "rgba(60,160,120,0.25)", text: "#103028" },
  Teal: { bg: "#80d0d0", border: "#58a8a8", shadow: "rgba(60,150,150,0.25)", text: "#102a2a" },
  Cyan: { bg: "#80d8e8", border: "#58b0c0", shadow: "rgba(60,150,180,0.25)", text: "#102830" },
  Sky: { bg: "#90c8f0", border: "#68a0c8", shadow: "rgba(60,130,180,0.25)", text: "#102040" },
  Indigo: { bg: "#a0a8e8", border: "#7880c0", shadow: "rgba(80,80,160,0.25)", text: "#1a1840" },
  Violet: { bg: "#b8a0e8", border: "#9078c0", shadow: "rgba(100,80,160,0.25)", text: "#201040" },
  Fuchsia: { bg: "#e0a0e0", border: "#b878b8", shadow: "rgba(160,80,160,0.25)", text: "#301030" },
  Rose: { bg: "#f5a0b0", border: "#c87888", shadow: "rgba(180,80,100,0.25)", text: "#3a1018" },
  Slate: { bg: "#b0b8c8", border: "#8890a0", shadow: "rgba(100,110,130,0.25)", text: "#202830" },
  Gray: { bg: "#c0c0c0", border: "#989898", shadow: "rgba(120,120,120,0.25)", text: "#282828" },
  Zinc: { bg: "#b8b8b8", border: "#909090", shadow: "rgba(110,110,110,0.25)", text: "#282828" },
  Neutral: { bg: "#c8c8c0", border: "#a0a098", shadow: "rgba(120,120,110,0.25)", text: "#282820" },
  Stone: { bg: "#c8c0b8", border: "#a09888", shadow: "rgba(120,110,100,0.25)", text: "#282018" },
  Black: { bg: "#505050", border: "#383838", shadow: "rgba(30,30,30,0.3)", text: "#e8e8e8" },
  White: { bg: "#f8f8f0", border: "#d0d0c8", shadow: "rgba(150,150,140,0.2)", text: "#303028" },
};

const DEFAULT_PALETTE: WireframePalette = WIREFRAME_COLORS.Yellow;

export function getWireframePalette(color?: string): WireframePalette {
  if (!color) return DEFAULT_PALETTE;
  return WIREFRAME_COLORS[color] || DEFAULT_PALETTE;
}
