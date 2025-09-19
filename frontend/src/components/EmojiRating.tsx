'use client';

import { useState } from 'react';
import { cn } from '@/lib/utils';
import { InvalidIcon } from '@/components/InvalidIcon';

interface EmojiRatingProps {
  value: number;
  onRate?: (rating: number) => void;
  size?: 'sm' | 'md' | 'lg';
  className?: string;
  disabled?: boolean;
  invalid?: string;
  label?: string;
  description?: string;
}

const emojis = ['😢', '😕', '😐', '🙂', '😊'];

export function EmojiRating({
  value = 0,
  onRate,
  size = 'md',
  className,
  disabled = false,
  invalid,
  label,
  description,
}: EmojiRatingProps) {
  const [hover, setHover] = useState(0);

  const handleRating = (rating: number) => {
    if (disabled) return;
    onRate?.(value === rating ? 0 : rating);
  };

  const emojiSizes = {
    sm: 'text-lg',
    md: 'text-2xl',
    lg: 'text-4xl',
  };

  const emojiElement = (
    <div className="flex items-center gap-2">
      <div
        className={cn(
          'flex items-center gap-1',
          disabled && 'opacity-50',
          className
        )}
      >
        {emojis.map((emoji, index) => (
          <button
            key={index}
            type="button"
            className={cn(
              'relative focus-visible:outline-none focus-visible:ring-2',
              'focus-visible:ring-ring focus-visible:ring-offset-2',
              'transition-transform duration-200',
              'hover:scale-125 active:scale-90 cursor-pointer',
              disabled && 'cursor-not-allowed hover:scale-100',
              emojiSizes[size]
            )}
            onClick={() => handleRating(index + 1)}
            onMouseEnter={() => !disabled && setHover(index + 1)}
            onMouseLeave={() => !disabled && setHover(0)}
            disabled={disabled}
          >
            <span
              className={cn(
                'transition-opacity duration-200',
                (hover || value) >= index + 1
                  ? 'text-primary opacity-100'
                  : 'text-muted-foreground opacity-40'
              )}
            >
              {emoji}
            </span>
          </button>
        ))}
      </div>
      {invalid && <InvalidIcon message={invalid} />}
    </div>
  );

  // If no label or description, return just the emojis
  if (!label && !description) {
    return emojiElement;
  }

  // Otherwise, wrap with label and description structure
  return (
    <div className="flex flex-col gap-2 flex-1 min-w-0">
      {label && (
        <label className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70">
          {label}
        </label>
      )}
      {emojiElement}
      {description && (
        <p className="text-sm text-muted-foreground">{description}</p>
      )}
    </div>
  );
}
