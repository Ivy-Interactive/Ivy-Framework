import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { getColor } from "@/lib/styles";
import { Densities } from "@/types/density";
import React from "react";

interface AvatarWidgetProps {
  image: string;
  fallback: string;
  color?: string;
  density?: Densities;
  width?: string;
  height?: string;
}

const getInitials = (name: string): string => {
  const words = name.split(" ");
  return words.map((word) => word.charAt(0).toUpperCase()).join("");
};

const getSizeClass = (density?: Densities): string => {
  switch (density) {
    case Densities.Small:
      return "h-6 w-6 text-xs";
    case Densities.Large:
      return "h-12 w-12 text-lg";
    default:
      return "h-10 w-10";
  }
};

export const AvatarWidget: React.FC<AvatarWidgetProps> = ({
  image,
  fallback,
  color,
  density,
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

  const sizeStyles: React.CSSProperties = {
    ...(width && { width }),
    ...(height && { height }),
  };

  return (
    <Avatar className={getSizeClass(density)} style={sizeStyles}>
      <AvatarImage src={image} title={fallback} />
      <AvatarFallback title={fallback} style={colorStyles}>
        {displayFallback}
      </AvatarFallback>
    </Avatar>
  );
};
