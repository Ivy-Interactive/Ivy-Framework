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
  width,
  height,
}) => {
  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  return (
    <div
      key={id}
      dangerouslySetInnerHTML={{ __html: content }}
      style={styles}
    />
  );
};
