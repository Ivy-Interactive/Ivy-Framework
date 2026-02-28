import React from 'react';
import { Progress } from '@/components/ui/progress';
import { Badge } from '@/components/ui/badge';
import { Check, Target } from 'lucide-react';
import { cn } from '@/lib/utils';
import { getWidth } from '@/lib/styles';

interface ProgressWidgetProps {
  id: string;
  goal?: string;
  value?: number;
  color?: string;
  width?: string;
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
  width = 'Full',
}) => {
  const isCompleted = value && value >= 100;

  const containerStyles: React.CSSProperties = {
    ...getWidth(width),
    ...(color && color.toLowerCase() !== 'primary'
      ? { '--primary': `var(--${color.toLowerCase()})` }
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
              'px-2 py-1.5 text-sm absolute bottom-full right-0 mb-2 transition-opacity pointer-events-none font-medium',
              'opacity-0 group-hover:opacity-100'
            )}
          >
            {!isCompleted && (
              <Target size={14} className="mr-1" strokeWidth={1.5} />
            )}
            {goal}
            {isCompleted && (
              <Check
                size={16}
                className="ml-1"
                strokeWidth={4}
                color="var(--primary)"
              />
            )}
          </Badge>
        )}
        <Progress value={value} className="bg-neutral/10" />
      </div>
    </>
  );
};
