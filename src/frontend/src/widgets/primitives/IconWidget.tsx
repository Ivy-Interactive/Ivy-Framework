import Icon from "@/components/Icon";
import { getColor, getHeight, getWidth } from "@/lib/styles";
import { Densities } from "@/types/density";
import React from "react";

interface IconWidgetProps {
  id: string;
  name: string;
  color?: string;
  width?: string;
  height?: string;
  density?: Densities;
}

export const IconWidget: React.FC<IconWidgetProps> = ({
  id,
  name,
  color,
  height,
  width,
  density: _density = Densities.Medium,
}) => {
  const styles = {
    ...getWidth(width),
    ...getHeight(height),
    ...getColor(color, "color", "background"),
  };

  return <Icon style={styles} name={name} key={id} />;
};
