import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { getColor, getHeight, getWidth } from "@/lib/styles";
import React from "react";

interface AvatarWidgetProps {
  image: string;
  fallback: string;
  color?: string;
  width?: string;
  height?: string;
}

const getInitials = (name: string): string => {
  const words = name.split(" ");
  return words.map((word) => word.charAt(0).toUpperCase()).join("");
};

export const AvatarWidget: React.FC<AvatarWidgetProps> = ({
  image,
  fallback,
  color,
  width,
  height,
}) => {
  const displayFallback = fallback?.length === 2 ? fallback : getInitials(fallback || "");

  const colorStyles: React.CSSProperties = color
    ? {
        ...getColor(color, "backgroundColor", "background"),
        ...getColor(color, "color", "foreground"),
      }
    : {};

  const sizeStyle: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  return (
    <Avatar style={sizeStyle}>
      <AvatarImage src={image} title={fallback} />
      <AvatarFallback title={fallback} style={colorStyles}>
        {displayFallback}
      </AvatarFallback>
    </Avatar>
  );
};
