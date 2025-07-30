import React from 'react';
import { Progress } from '@/components/ui/progress';
import { Check, Target } from 'lucide-react';
import { cn } from '@/lib/utils';
import { getWidth } from '@/lib/styles';

interface ProgressWidgetProps {
  id: string;
  goal?: string;
  value?: number;
  colorVariant: 'Primary';
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
  colorVariant,
  width,
}) => {
  const isCompleted = value && value >= 100;
  const styles = getWidth(width);

  return (
    <>
      <SparkleStyles />
      <div className="w-full group" style={styles}>
        {goal && (
          <div
            className={cn(
              'rounded-xl rounded-br-none bg-muted p-2 mb-2 text-small-label w-fit ml-auto',
              'text-muted-foreground flex flex-row items-center',
              !isCompleted && 'opacity-50 group-hover:opacity-100',
              isCompleted && 'sparkle-glow'
            )}
          >
            {!isCompleted && (
              <Target size={14} className="mr-1" strokeWidth={1.5} />
            )}
            <span className="">{goal}</span>
            {isCompleted && (
              <Check
                size={14}
                className="ml-1"
                strokeWidth={4}
                color="var(--primary)"
              />
            )}
          </div>
        )}
        <Progress
          value={value}
          className="bg-muted"
          style={
            {
              '--progress-background': colorVariant
                ? `var(--${colorVariant})`
                : 'var(--primary)',
            } as React.CSSProperties
          }
        />
      </div>
    </>
  );
};
