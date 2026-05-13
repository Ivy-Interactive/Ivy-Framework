import React from "react";
import { QRCodeSVG } from "qrcode.react";

type EventHandler = (eventName: string, widgetId: string, args: unknown[]) => void;

interface QRCodeProps {
  id: string;
  value?: string;
  pixelSize?: number;
  level?: "L" | "M" | "Q" | "H";
  includeMargin?: boolean;
  bgColor?: string;
  fgColor?: string;
  eventHandler?: EventHandler;
  onIvyEvent?: EventHandler;
  events?: string[];
}

export const QRCode: React.FC<QRCodeProps> = ({
  value = "",
  pixelSize = 256,
  level = "L",
  includeMargin = true,
  bgColor,
  fgColor,
}) => {
  return (
    <QRCodeSVG
      value={value || " "}
      size={pixelSize}
      level={level}
      marginSize={includeMargin ? 4 : 0}
      bgColor={bgColor}
      fgColor={fgColor}
    />
  );
};
