import React from "react";
import { Progress } from "@/components/ui/progress";
import { Badge } from "@/components/ui/badge";
import { Check, Target } from "lucide-react";
import { cn } from "@/lib/utils";
import { getWidth } from "@/lib/styles";
import { Densities } from "@/types/density";

interface ProgressWidgetProps {
  id: string;
  goal?: string;
  value?: number;
  color?: string;
  width?: string;
  indeterminate?: boolean;
  density?: Densities;
}

const SparkleStyles = () => (
  <style>
    {`
      @keyframes sparkle {
        0% {
          box-shadow:
            1px 1px 1px #fff,
            1px 1px 1px #fff;
        }
        50% {
          box-shadow:
            1px 1px 1px #fff,
            2px 2px 2px var(--primary);
        }
        100% {
          box-shadow:
            1px 1px 1px #fff,
            1px 1px 1px #fff;
        }
      }

      .sparkle-glow {
        animation: sparkle 3s infinite;
      }
    `}
  </style>
);

export const ProgressWidget: React.FC<ProgressWidgetProps> = ({
  value,
  goal,
  color,
  width = "Full",
  indeterminate = false,
  density = Densities.Medium,
}) => {
  const isIndeterminate = indeterminate || value === null || value === undefined;
  const isCompleted = !isIndeterminate && value && value >= 100;

  const targetSize = density === Densities.Small ? 12 : density === Densities.Large ? 16 : 14;
  const checkSize = density === Densities.Small ? 14 : density === Densities.Large ? 18 : 16;
  const badgeClasses =
    density === Densities.Small
      ? "px-1.5 py-1 text-xs"
      : density === Densities.Large
        ? "px-2.5 py-2 text-base"
        : "px-2 py-1.5 text-sm";

  const containerStyles: React.CSSProperties = {
    ...getWidth(width),
    ...(color && color.toLowerCase() !== "primary"
      ? { "--primary": `var(--${color.toLowerCase()})` }
      : {}),
  };

  return (
    <>
      <SparkleStyles />
      <div className="w-full group relative" style={containerStyles}>
        {goal && (
          <Badge
            variant="secondary"
            className={cn(
              badgeClasses,
              "absolute bottom-full right-0 mb-2 transition-opacity pointer-events-none font-medium",
              "opacity-0 group-hover:opacity-100",
            )}
          >
            {!isCompleted && <Target size={targetSize} className="mr-1" strokeWidth={1.5} />}
            {goal}
            {isCompleted && (
              <Check size={checkSize} className="ml-1" strokeWidth={4} color="var(--primary)" />
            )}
          </Badge>
        )}
        <Progress
          value={isIndeterminate ? undefined : value}
          indeterminate={isIndeterminate}
          className="bg-neutral/10"
        />
      </div>
    </>
  );
};
