import { getHeight, getWidth } from "@/lib/styles";
import React from "react";

interface SvgWidgetProps {
  id: string;
  content: string;
  width?: string;
  height?: string;
}

export const SvgWidget: React.FC<SvgWidgetProps> = ({
  id,
  content,
  width = "Auto",
  height = "Auto",
}) => {
  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  const ref = React.useRef<HTMLDivElement>(null);
  React.useEffect(() => {
    if (ref.current) ref.current.innerHTML = content;
  }, [content]);

  return <div key={id} ref={ref} style={styles} />;
};
