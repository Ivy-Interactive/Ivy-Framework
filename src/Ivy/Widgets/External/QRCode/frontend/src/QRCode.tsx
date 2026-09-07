import React from "react";
import { QRCodeSVG } from "qrcode.react";
import { resolveIvyColorForSvg } from "./ivyColor";

type EventHandler = (eventName: string, widgetId: string, args: unknown[]) => void;

type QrErrorCorrectionLevel = "Low" | "Medium" | "Quartile" | "High";

const correctionToLibraryLevel: Record<QrErrorCorrectionLevel, "L" | "M" | "Q" | "H"> = {
  Low: "L",
  Medium: "M",
  Quartile: "Q",
  High: "H",
};

interface QRCodeProps {
  id: string;
  value?: string;
  pixelSize?: number;
  errorCorrectionLevel?: QrErrorCorrectionLevel;
  background?: string;
  foreground?: string;
  eventHandler?: EventHandler;
  onIvyEvent?: EventHandler;
  events?: string[];
}

export const QRCode: React.FC<QRCodeProps> = ({
  value = "",
  pixelSize = 256,
  errorCorrectionLevel = "Low",
  background,
  foreground,
}) => {
  const level = correctionToLibraryLevel[errorCorrectionLevel] ?? "L";
  const bgColor = resolveIvyColorForSvg(background);
  const fgColor = resolveIvyColorForSvg(foreground);

  return (
    <QRCodeSVG
      value={value || " "}
      size={pixelSize}
      level={level}
      marginSize={0}
      {...(bgColor !== undefined ? { bgColor } : {})}
      {...(fgColor !== undefined ? { fgColor } : {})}
    />
  );
};
