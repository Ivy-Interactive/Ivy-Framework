import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import React from "react";

interface AvatarWidgetProps {
  image: string;
  fallback: string;
}

// Utility function to extract initials from a full name
const getInitials = (name: string): string => {
  const words = name.split(" ");
  const initials = words.map((word) => word.charAt(0).toUpperCase()).join("");
  return initials;
};

export const AvatarWidget: React.FC<AvatarWidgetProps> = ({
  image,
  fallback,
}) => {
  const displayFallback =
    fallback.length === 2 ? fallback : getInitials(fallback);
  return (
    <Avatar>
      <AvatarImage src={image} title={fallback} />
      <AvatarFallback title={fallback}>{displayFallback}</AvatarFallback>
    </Avatar>
  );
};
