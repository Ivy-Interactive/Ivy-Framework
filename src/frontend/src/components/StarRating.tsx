'use client';

import { useState, useCallback } from 'react';
import { Star } from 'lucide-react';
import { m, LazyMotion, domAnimation } from 'framer-motion';
import { cn } from '@/lib/utils';
import { InvalidIcon } from '@/components/InvalidIcon';
import { Densities } from '@/types/density';

interface StarRatingProps {
  totalStars?: number;
  value: number;
  onRate?: (rating: number) => void;
  density?: Densities;
  className?: string;
  disabled?: boolean;
  invalid?: string;
  allowHalf?: boolean;
}

export function StarRating({
  totalStars = 5,
  value = 0,
  onRate,
  density = Densities.Medium,
  className,
  disabled = false,
  invalid,
  allowHalf = false,
}: StarRatingProps) {
  const [hover, setHover] = useState(0);

  const getHalfValue = useCallback(
    (star: number, e: React.MouseEvent<HTMLButtonElement>) => {
      if (!allowHalf) return star;
      const rect = e.currentTarget.getBoundingClientRect();
      const isLeftHalf = e.clientX - rect.left < rect.width / 2;
      return isLeftHalf ? star - 0.5 : star;
    },
    [allowHalf]
  );

  const handleRating = useCallback(
    (star: number, e: React.MouseEvent<HTMLButtonElement>) => {
      if (disabled) return;
      const rating = getHalfValue(star, e);
      onRate?.(value === rating ? 0 : rating);
    },
    [disabled, onRate, value, getHalfValue]
  );

  const handleMouseMove = useCallback(
    (star: number, e: React.MouseEvent<HTMLButtonElement>) => {
      if (disabled) return;
      setHover(getHalfValue(star, e));
    },
    [disabled, getHalfValue]
  );

  const starSizes = {
    Small: 'h-4 w-4',
    Medium: 'h-6 w-6',
    Large: 'h-8 w-8',
  };

  const displayValue = hover || value;

  return (
    <div className="flex items-center gap-2">
      <LazyMotion features={domAnimation}>
        <div
          className={cn(
            'flex items-center gap-1',
            disabled && 'opacity-50',
            className
          )}
        >
          {Array.from({ length: totalStars }, (_, index) => index + 1).map(
            star => {
              const fillRatio = Math.max(
                0,
                Math.min(1, displayValue - (star - 1))
              );
              const isFull = fillRatio >= 1;
              const isPartial = fillRatio > 0 && fillRatio < 1;

              return (
                <m.button
                  key={star}
                  type="button"
                  className={cn(
                    'relative focus-visible:outline-none focus-visible:ring-2',
                    'focus-visible:ring-ring focus-visible:ring-offset-2',
                    'cursor-pointer',
                    disabled && 'cursor-not-allowed'
                  )}
                  onClick={e => handleRating(star, e)}
                  onMouseEnter={e => !disabled && handleMouseMove(star, e)}
                  onMouseMove={e =>
                    allowHalf && !disabled && handleMouseMove(star, e)
                  }
                  onMouseLeave={() => !disabled && setHover(0)}
                  whileHover={
                    !disabled ? { scale: 1.3, rotate: -10 } : undefined
                  }
                  whileTap={!disabled ? { scale: 0.9, rotate: 15 } : undefined}
                  disabled={disabled}
                >
                  <m.div
                    className="relative"
                    initial={{ scale: 1 }}
                    animate={{
                      scale: fillRatio > 0 ? 1.2 : 1,
                    }}
                    transition={{
                      duration: 0.3,
                      ease: 'easeOut',
                    }}
                  >
                    {/* Background (unfilled) star */}
                    <Star
                      className={cn(
                        starSizes[density],
                        'fill-current stroke-[1.5px] text-border transition-colors duration-300'
                      )}
                    />
                    {/* Filled overlay */}
                    {(isFull || isPartial) && (
                      <div
                        className="absolute inset-0 overflow-hidden transition-colors duration-300"
                        style={{ width: `${fillRatio * 100}%` }}
                      >
                        <Star
                          className={cn(
                            starSizes[density],
                            'fill-current stroke-[1.5px] text-primary'
                          )}
                        />
                      </div>
                    )}
                  </m.div>
                </m.button>
              );
            }
          )}
        </div>
      </LazyMotion>
      {invalid && <InvalidIcon message={invalid} />}
    </div>
  );
}
