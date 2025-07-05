import IvyLogo from "@/components/IvyLogo";
import { getColor, getHeight, getWidth } from "@/lib/styles";

interface IvyLogoWidgetProps {
  width?: string;
  height?: string;
  color?: string;
}

export const IvyLogoWidget: React.FC<IvyLogoWidgetProps> = ({
  width,
  height,
  color,
}) => {
  const styles = {
    ...getWidth(width),
    ...getHeight(height),
    ...getColor(color, "color", "background"),
  };

  return <IvyLogo style={styles} />;
};
