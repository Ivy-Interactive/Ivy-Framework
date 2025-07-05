import Icon from "@/components/Icon";
import { getColor, getHeight, getWidth } from "@/lib/styles";
import React from "react";

interface IconWidgetProps {
  id: string;
  name: string;
  color?: string;
  width?: string;
  height?: string;
}

export const IconWidget: React.FC<IconWidgetProps> = ({
  id,
  name,
  color,
  height,
  width,
}) => {
  const styles = {
    ...getWidth(width),
    ...getHeight(height),
    ...getColor(color, "color", "background"),
  };

  return <Icon style={styles} name={name} key={id} />;
};
