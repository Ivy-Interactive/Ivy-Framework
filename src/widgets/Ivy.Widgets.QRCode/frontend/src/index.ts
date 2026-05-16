import { QRCode } from "./QRCode";

if (typeof window !== "undefined") {
  (window as unknown as Record<string, unknown>).Ivy_Widgets_QRCode = {
    QRCode,
  };
}

export { QRCode };
