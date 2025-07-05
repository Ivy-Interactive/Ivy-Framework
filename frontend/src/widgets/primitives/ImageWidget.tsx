import { getHeight, getWidth } from "@/lib/styles";
import { getIvyHost } from "@/lib/utils";
import React from "react";

interface ImageWidgetProps {
  id: string;
  src: string;
  width?: string;
  height?: string;
}

const getImageUrl = (url: string) => {
  if (url.startsWith("http://") || url.startsWith("https://")) {
    return url;
  }
  return `${getIvyHost()}${url.startsWith("/") ? "" : "/"}${url}`;
};

export const ImageWidget: React.FC<ImageWidgetProps> = ({
  id,
  src,
  width,
  height,
}) => {
  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  return <img src={getImageUrl(src)} key={id} style={styles} />;
};
