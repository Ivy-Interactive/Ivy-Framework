import IvyLogo from "@/components/IvyLogo";
import { getColor, getHeight, getWidth } from "@/lib/styles";

interface IvyLogoWidgetProps {
  width?: string;
  height?: string;
  color?: string;
}

export const IvyLogoWidget: React.FC<IvyLogoWidgetProps> = ({
  width = "Units:25",
  height = "Auto",
  color,
}) => {
  const styles = {
    ...getWidth(width),
    ...getHeight(height),
    ...getColor(color || "ivy-green", "color", "background"),
  };

  return <IvyLogo style={styles} className="text-brand" />;
};
