'use client';

import { useState } from 'react';
import { cn } from "@/lib/utils";
import { inputStyles } from '@/lib/styles';
import { InvalidIcon } from "@/components/InvalidIcon";

interface EmojiRatingProps {
  value: number;
  onRate?: (rating: number) => void;
  size?: 'sm' | 'md' | 'lg';
  className?: string;
  disabled?: boolean;
  invalid?: string;
}

const emojis = ['😢', '😕', '😐', '🙂', '😊'];

export function EmojiRating({ 
  value = 0,
  onRate,
  size = 'md',
  className,
  disabled = false,
  invalid,
}: EmojiRatingProps) {
  const [hover, setHover] = useState(0);

  const handleRating = (rating: number) => {
    if (disabled) return;
    onRate?.(value === rating ? 0 : rating);
  };

  const emojiSizes = {
    sm: 'text-lg',
    md: 'text-2xl',
    lg: 'text-4xl'
  };

  return (
    <div className="relative">
      <div className={cn(
        "flex items-center gap-1",
        disabled && "opacity-50",
        className
      )}>
        {emojis.map((emoji, index) => (
          <button
            key={index}
            type="button"
            className={cn(
              "relative focus-visible:outline-none focus-visible:ring-2",
              "focus-visible:ring-ring focus-visible:ring-offset-2",
              "transition-transform duration-200",
              "hover:scale-125 active:scale-90",
              disabled && "cursor-not-allowed hover:scale-100",
              invalid && inputStyles.invalid,
              emojiSizes[size]
            )}
            onClick={() => handleRating(index + 1)}
            onMouseEnter={() => !disabled && setHover(index + 1)}
            onMouseLeave={() => !disabled && setHover(0)}
            disabled={disabled}
          >
            <span className={cn(
              "transition-opacity duration-200",
              (hover || value) >= index + 1 ? "opacity-100" : "opacity-40"
            )}>
              {emoji}
            </span>
          </button>
        ))}
      </div>
      {invalid && (
        <div className="absolute right-0 top-1/2 -translate-y-1/2 h-4 w-4">
          <InvalidIcon message={invalid} />
        </div>
      )}
    </div>
  );
}