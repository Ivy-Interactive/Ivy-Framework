'use client';

import { useState, useCallback } from 'react';
import { cn } from '@/lib/utils';
import { InvalidIcon } from '@/components/InvalidIcon';
import { Densities } from '@/types/density';

interface EmojiRatingProps {
  value: number;
  onRate?: (rating: number) => void;
  density?: Densities;
  className?: string;
  disabled?: boolean;
  invalid?: string;
  allowHalf?: boolean;
}

const emojis = ['😢', '😕', '😐', '🙂', '😊'];

export function EmojiRating({
  value = 0,
  onRate,
  density = Densities.Medium,
  className,
  disabled = false,
  invalid,
  allowHalf = false,
}: EmojiRatingProps) {
  const [hover, setHover] = useState(0);

  const getHalfValue = useCallback(
    (rating: number, e: React.MouseEvent<HTMLButtonElement>) => {
      if (!allowHalf) return rating;
      const rect = e.currentTarget.getBoundingClientRect();
      const isLeftHalf = e.clientX - rect.left < rect.width / 2;
      return isLeftHalf ? rating - 0.5 : rating;
    },
    [allowHalf]
  );

  const handleRating = useCallback(
    (rating: number, e: React.MouseEvent<HTMLButtonElement>) => {
      if (disabled) return;
      const val = getHalfValue(rating, e);
      onRate?.(value === val ? 0 : val);
    },
    [disabled, onRate, value, getHalfValue]
  );

  const handleMouseMove = useCallback(
    (rating: number, e: React.MouseEvent<HTMLButtonElement>) => {
      if (disabled) return;
      setHover(getHalfValue(rating, e));
    },
    [disabled, getHalfValue]
  );

  const emojiSizes = {
    Small: 'text-lg',
    Medium: 'text-2xl',
    Large: 'text-4xl',
  };

  const displayValue = hover || value;

  return (
    <div className="flex items-center gap-2">
      <div
        className={cn(
          'flex items-center gap-1',
          disabled && 'opacity-50',
          className
        )}
      >
        {emojis.map((emoji, index) => {
          const rating = index + 1;
          const fillRatio = Math.max(0, Math.min(1, displayValue - (rating - 1)));
          const isActive = fillRatio >= 1;
          const isPartial = fillRatio > 0 && fillRatio < 1;

          return (
            <button
              key={index}
              type="button"
              className={cn(
                'relative focus-visible:outline-none focus-visible:ring-2',
                'focus-visible:ring-ring focus-visible:ring-offset-2',
                'transition-transform duration-200',
                'hover:scale-125 active:scale-90 cursor-pointer',
                disabled && 'cursor-not-allowed hover:scale-100',
                emojiSizes[density]
              )}
              onClick={e => handleRating(rating, e)}
              onMouseEnter={e => !disabled && handleMouseMove(rating, e)}
              onMouseMove={e => allowHalf && !disabled && handleMouseMove(rating, e)}
              onMouseLeave={() => !disabled && setHover(0)}
              disabled={disabled}
            >
              <span
                className={cn(
                  'transition-opacity duration-200',
                  isActive
                    ? 'text-primary opacity-100'
                    : isPartial
                      ? 'text-primary opacity-70'
                      : 'text-muted-foreground opacity-40'
                )}
              >
                {emoji}
              </span>
            </button>
          );
        })}
      </div>
      {invalid && <InvalidIcon message={invalid} />}
    </div>
  );
}
